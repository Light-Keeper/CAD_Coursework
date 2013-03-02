#include <cad_object.h>
#include <cad_module.h>
#include <memory>
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
};

void kernel_Delete(cad_kernel *self)
{
	free(self);
}

typedef uint32_t ( *init_module_f)(cad_module_info *) ;

void kernel_Exec(cad_kernel *self)
{
	WIN32_FIND_DATAA find ;
	HANDLE hFind = FindFirstFileA(".\\plugins\\*.dll", &find );
	
	if (hFind == INVALID_HANDLE_VALUE)
	{
			self->PrintDebug("loading: no plagins foud. can not start.\n");
			return;
	}

	self->sys->module_cout = 0;
	self->sys->modules = NULL;

	for (; FindNextFileA( hFind, &find) ; )
	{
		char name[1024];
		sprintf_s(name, ".\\plugins\\%s", find.cFileName);
		HMODULE hModule = LoadLibraryA( name );
		if (hModule == NULL) continue;
		cad_module_info info;
		init_module_f init = (init_module_f)GetProcAddress(hModule, "startup_module_function") ;

		if (init != NULL)
		{
			init( &info );
			self->sys->modules = (cad_module_info *)realloc(self->sys->modules, 
				sizeof(cad_module_info) * self->sys->module_cout + 1);
			self->sys->modules[self->sys->module_cout++] = info;
		}
		else 
		{
			self->PrintDebug("loading %s error: cad_module_begin declaration not found\n", name );
		}

	}

	FindClose( hFind );


	// find best GUI module and run it

	cad_module_info *gui = NULL;
	for(uint32_t i = 0; i < self->sys->module_cout; i++)
	{
		if (self->sys->modules[ i ].module_capability == CAP_GUI && 
			( gui == NULL || gui->module_priority <  self->sys->modules[ i ].module_priority ))
					gui = &self->sys->modules[ i ];
	}

	if (gui == NULL) 
		self->PrintDebug("no GUI module found. can not start.");
	else 
	{
		cad_GUI *gui_module = (cad_GUI *)gui->Open(self, NULL);
		gui_module->SetCMDArgs(GetCommandLineA());
		gui_module->Exec( gui_module );
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
			
		cad_scheme *scheme = reader->ReadSchme( reader );
		cad_route_map *map = reader->ReadrRouteMap( reader );
		if (scheme == NULL )
		{
			self->PrintDebug( "invalid input file %s\n", path);
			continue;
		} 
		if ( map )
		{
			map->sheme = scheme;
			self->sys->current_state = KERNEL_STATE_NEED_TRACE;
		} else 
			self->sys->current_state = KERNEL_STATE_NEED_PLACE;

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
	while ( (code = self->NextStep( self )) == MORE_ACTIONS );
	return code;
}


uint32_t kernel_NextStep( cad_kernel *self )
{
	if (self->sys->current_state == KERNEL_STATE_PLACING)
	{
		uint32_t result = self->sys->current_sheme->MakeStep( self->sys->current_sheme );
		if ( result == LAST_ACTION_ERROR ) self->sys->current_state = KERNEL_STATE_NEED_PLACE;
		if ( result == LAST_ACTION_OK ) self->sys->current_state = KERNEL_STATE_NEED_TRACE;
		return result;
	} 

	if (self->sys->current_state == KERNEL_STATE_TRACING)
	{
		uint32_t result = self->sys->current_route->MakeStep( self->sys->current_route );
		if ( result != MORE_ACTIONS ) self->sys->current_state = KERNEL_STATE_NEED_TRACE;
		return result;
	} 

	if (self->sys->current_state == KERNEL_STATE_NEED_PLACE)
	{
		return self->StartPlaceMoule( self, NULL, false);
	} 

	if (self->sys->current_state == KERNEL_STATE_NEED_TRACE)
	{
		return self->StartTraceModule( self, NULL, false);
	} 

	return LAST_ACTION_ERROR;
}


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


bool kernel_StartPlaceMoule( cad_kernel *self, const char *force_module_name, bool demo_mode)
{
	if ( self->sys->current_state == KERNEL_STATE_EMPTY ) return false;

	self->sys->map_generator->DestroyMap( self->sys->map_generator,  self->sys->current_route );
	self->sys->current_route = NULL;

	if (self->sys->current_sheme->AboutToDestroy != NULL)
		self->sys->current_sheme->AboutToDestroy( self->sys->current_sheme );
	
	self->sys->current_sheme->sys = NULL;
	self->sys->current_sheme->AboutToDestroy = NULL;

	cad_module_info *module = internal_find_module_by_name(self, force_module_name, CAP_PLACEMENT);
	if (module == NULL) return false;

	module->Open( self, self->sys->current_sheme );
	self->sys->current_sheme->Clear(self->sys->current_sheme );
	self->sys->current_state = KERNEL_STATE_PLACING;
	return true;
}


bool kernel_StartTraceModule(cad_kernel *self, const char *force_module_name, bool demo_mode)
{
	if ( self->sys->current_state != KERNEL_STATE_NEED_TRACE ) return false;

	// TODO: implement
	return false;	
}


cad_kernel * cad_kernel_New(uint32_t argc, char *argv[])
{
	cad_kernel *kernel = (cad_kernel *)malloc( sizeof(cad_kernel) ); 
	kernel->sys = (cad_kernel_private *)malloc( sizeof(cad_kernel_private) );

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

	return kernel;
}

