#include <cad_object.h>
#include <cad_module.h>
#include <Windows.h>

struct cad_kernel_private
{
	uint32_t			module_cout;
	cad_module_info	*	modules;
	uint32_t			current_state;
	cad_scheme		*	current_sheme;
	cad_route_map	*	current_route;
	cad_render_module *	render;
	cad_map_generator *	map_generator;

	cad_GUI			*	gui;
};

void kernel_Delete(cad_kernel *self)
{
	free(self);
}

typedef uint32_t ( *init_module_f)(cad_module_info *) ;

cad_module_info *internal_find_module_by_name(cad_kernel *self, const char *force_module_name, uint32_t type)
{
	cad_module_info *module = NULL;

	for (uint32_t i = 0; i < self->sys->module_cout; i++)
	{
		if (self->sys->modules[ i ].module_capability != type) continue;

		if (force_module_name != NULL && 
			strcmp( force_module_name, self->sys->modules[ i ].module_name ) != 0) continue;

		if (force_module_name != NULL) 
		{
			module = &self->sys->modules[ i ];
			break;
		}

		if (module == NULL || module->module_priority < self->sys->modules[ i ].module_priority)
			module = &self->sys->modules[ i ];
	} 

	if ( module == NULL )
		self->PrintDebug( "no module found, %s module requestd\n", force_module_name );

	return module;
}

void kernel_Exec(cad_kernel *self)
{
	WIN32_FIND_DATAA find ;
	char ExecutableName[2048];
	GetModuleFileNameA(NULL, ExecutableName, sizeof(ExecutableName));
	ExecutableName[sizeof(ExecutableName) - 1] = 0;
	
	for (int i = strlen( ExecutableName ) - 1; i >= 0; i--)
		if (ExecutableName[i] == '\\' || i == 0)
		{
			ExecutableName[i] = 0;
			break;
		}

	strcat_s(ExecutableName, "\\plugins\\*.dll");

	HANDLE hFind = FindFirstFileA(ExecutableName, &find );
	
	if (hFind == INVALID_HANDLE_VALUE)
	{
		self->PrintDebug("loading: no plagins foud. can not start.\n");
		return;
	}

	self->sys->module_cout = 0;
	self->sys->modules = NULL;

	do 
	{
		{
			uint32_t pos = strlen( ExecutableName );
			while (ExecutableName[pos] != '\\') pos--;
			strcpy_s(ExecutableName + pos + 1, sizeof(ExecutableName) - pos - 1, find.cFileName);
		}

		HMODULE hModule = LoadLibraryA( ExecutableName );
		if (hModule == NULL) continue;
		cad_module_info info;
		init_module_f init = (init_module_f)GetProcAddress(hModule, "startup_module_function") ;

		if (init != NULL)
		{
			init( &info );
			self->sys->modules = (cad_module_info *)realloc(self->sys->modules, 
				sizeof(cad_module_info) * (self->sys->module_cout + 1));
			self->sys->modules[self->sys->module_cout++] = info;
		}
		else 
		{
			self->PrintDebug("loading %s error: cad_module_begin declaration not found\n", ExecutableName );
		}

	} while ( FindNextFileA( hFind, &find) ) ;

	FindClose( hFind );
	cad_module_info *info;

	info = internal_find_module_by_name(self, NULL, CAP_MAP_GENERATOR );
	if (info) self->sys->map_generator = (cad_map_generator *)info->Open(self, NULL);

	info = internal_find_module_by_name(self, NULL, CAP_RENDER );
	if (info) self->sys->render = (cad_render_module *)info->Open(self, NULL);

	cad_module_info *gui = internal_find_module_by_name(self, NULL, CAP_GUI);
	if (gui == NULL) 
		self->PrintDebug("no GUI module found. can not start.");
	else 
	{
		cad_GUI *gui_module = (cad_GUI *)gui->Open(self, NULL);
		if ( !gui_module ) 
		{
			self->PrintDebug( "can not initialize GUI module \"%s\"\n", gui->module_name);
			gui->Close(self, gui_module);
			return ;
		}
		self->sys->gui = gui_module;
		gui_module->SetCMDArgs(gui_module, GetCommandLineA());
		gui_module->Exec( gui_module );

		gui->Close(self, gui_module);
	}
}


bool kernel_LoadFile(cad_kernel *self, const char *path)
{
	cad_kernel_private *sys = self->sys;

	for (uint32_t i = 0; i < sys->module_cout; i++)
	{
		if (sys->modules[ i ].module_capability != CAP_ACCESS) continue;

		cad_access_module *reader = (cad_access_module *)sys->modules[ i ].Open(self, (void *)path);
		if (reader == NULL) continue;
		
		cad_scheme *scheme ;
		cad_route_map *map ; 
		
		reader->ReadAll(reader, &scheme, &map);

		sys->modules[ i ].Close(self, reader);

		if (scheme == NULL ) continue;
	
		if ( map )
		{
			self->sys->current_state = KERNEL_STATE_TRACE;
			if ( self->sys->map_generator )
				self->sys->map_generator->ReinitializeRouteMap(self->sys->map_generator, scheme, &map);
			else map->Delete( map ), map = NULL;
		} else 
			self->sys->current_state = KERNEL_STATE_PLACE;

		if (self->sys->current_sheme != NULL)
		{
			if ( self->sys->current_sheme->AboutToDestroy )
				self->sys->current_sheme->AboutToDestroy( self->sys->current_sheme );
			self->sys->current_sheme->Delete( self->sys->current_sheme );
		}

		if (self->sys->current_route != NULL)
		{
			if ( self->sys->current_route->AboutToDestroy )
				self->sys->current_route->AboutToDestroy( self->sys->current_route );
			self->sys->current_route->Delete( self->sys->current_route );
		}

		self->sys->current_sheme = scheme;
		self->sys->current_route = map;
		return true;
	}
	return false;
}


uint32_t kernel_GetCurrentState(cad_kernel *self)
{
	return self->sys->current_state;
}


uint32_t kernel_GetModuleList(cad_kernel *self, cad_module_info **modules)
{
	*modules = self->sys->modules;
	return self->sys->module_cout;
}


uint32_t kernel_RunToEnd( cad_kernel *self) 
{
	uint32_t code ;
	while ( (code = self->NextStep(self, false)) == MORE_ACTIONS );
	return code;
}


uint32_t kernel_NextStep( cad_kernel *self,  bool demo_mode)
{
	if (self->sys->current_state == KERNEL_STATE_PLACING)
	{
		uint32_t result = self->sys->current_sheme->MakeStep( self->sys->current_sheme, demo_mode );
		if ( result != MORE_ACTIONS ) self->sys->current_state = KERNEL_STATE_PLACE;
		self->sys->gui->UpdatePictureEvent( self->sys->gui );
		return result;
	} 

	if (self->sys->current_state == KERNEL_STATE_TRACING)
	{
		uint32_t result = self->sys->current_route->MakeStep( self->sys->current_route, demo_mode );
		if ( result != MORE_ACTIONS ) self->sys->current_state = KERNEL_STATE_TRACE;
		self->sys->gui->UpdatePictureEvent( self->sys->gui );
		return result;
	} 

	if (self->sys->current_state == KERNEL_STATE_PLACE)
	{
		return self->StartPlaceMoule( self, NULL, false) ? MORE_ACTIONS : LAST_ACTION_ERROR;
	} 

	if (self->sys->current_state == KERNEL_STATE_TRACE)
	{
		return self->StartTraceModule( self, NULL, false) ? MORE_ACTIONS : LAST_ACTION_ERROR;
	} 

	return LAST_ACTION_ERROR;
}


bool kernel_StartPlaceMoule( cad_kernel *self, const char *force_module_name, bool demo_mode)
{
	if ( self->sys->current_state == KERNEL_STATE_EMPTY ) return false;
	
	if (self->sys->current_route && self->sys->current_route->AboutToDestroy)
		self->sys->current_route->AboutToDestroy( self->sys->current_route );
	
	if (self->sys->current_route)
	self->sys->current_route->Delete( self->sys->current_route );
	self->sys->current_route = NULL;

	if (self->sys->current_sheme && self->sys->current_sheme->AboutToDestroy != NULL)
		self->sys->current_sheme->AboutToDestroy( self->sys->current_sheme );
	
	self->sys->current_sheme->sys = NULL;
	self->sys->current_sheme->AboutToDestroy = NULL;

	cad_module_info *module = internal_find_module_by_name(self, force_module_name, CAP_PLACEMENT);
	if (module == NULL) return false;

	module->Open( self, self->sys->current_sheme );
	self->sys->current_sheme->Clear( self->sys->current_sheme );
	self->sys->current_state = KERNEL_STATE_PLACING;
	self->sys->gui->UpdatePictureEvent( self->sys->gui );
	return true;
}


bool kernel_StartTraceModule(cad_kernel *self, const char *force_module_name, bool demo_mode)
{
	if ( self->sys->current_state == KERNEL_STATE_EMPTY  || 
		self->sys->current_state == KERNEL_STATE_PLACING ) return false;

	if (! self->sys->map_generator) return false;

	if (! self->sys->map_generator->ReinitializeRouteMap( self->sys->map_generator, self->sys->current_sheme, 
		&self->sys->current_route)) return false;

	self->sys->current_state = KERNEL_STATE_TRACING;
	self->sys->gui->UpdatePictureEvent( self->sys->gui );
	return true;	
}


cad_picture * kernel_RenderPicture(cad_kernel *self, bool forceDrawLayer, uint32_t forceDrawLayerNunber)
{
	if (! self->sys->render ) return NULL;

 	if (self->sys->current_state == KERNEL_STATE_PLACE || 
		self->sys->current_state == KERNEL_STATE_PLACING )
		return self->sys->render->RenderSchme(self->sys->render, self->sys->current_sheme );

	if (self->sys->current_state == KERNEL_STATE_TRACE || 
		self->sys->current_state == KERNEL_STATE_TRACING )
		return self->sys->render->RenderMap(self->sys->render, self->sys->current_route, forceDrawLayer,  forceDrawLayerNunber);

	return NULL;
}

void kernel_StopCurrentModule( cad_kernel *self )
{
	if (self->sys->current_state == KERNEL_STATE_PLACING)
	{
		self->sys->current_sheme->AboutToDestroy( self->sys->current_sheme );
		self->sys->current_state = KERNEL_STATE_PLACE;
	}
	if (self->sys->current_state == KERNEL_STATE_TRACING)
	{
		self->sys->current_sheme->AboutToDestroy( self->sys->current_sheme );
		self->sys->current_state = KERNEL_STATE_TRACE;
	}
}

cad_kernel * cad_kernel_New(uint32_t argc, char *argv[])
{
	cad_kernel *kernel = (cad_kernel *)malloc( sizeof(cad_kernel) ); 
	kernel->sys = (cad_kernel_private *)malloc( sizeof(cad_kernel_private) );
	memset(kernel->sys, 0, sizeof (cad_kernel_private));

	kernel->sys->current_state = KERNEL_STATE_EMPTY;
	
	kernel->PrintDebug			= printf;
	kernel->PrintInfo			= printf;
	kernel->Delete				= kernel_Delete;
	kernel->Exec				= kernel_Exec;
	kernel->LoadFile			= kernel_LoadFile;
	kernel->GetCurrentState		= kernel_GetCurrentState;
	kernel->GetModuleList		= kernel_GetModuleList;
	kernel->RunToEnd			= kernel_RunToEnd;
	kernel->NextStep			= kernel_NextStep;
	kernel->StartPlaceMoule		= kernel_StartPlaceMoule;
	kernel->StartTraceModule	= kernel_StartTraceModule;
	kernel->RenderPicture		= kernel_RenderPicture;
	kernel->StopCurrentModule	= kernel_StopCurrentModule;
	return kernel;
}

