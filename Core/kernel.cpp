#include <cad_object.h>
#include <cad_module.h>
#include <memory>
#include <Windows.h>

struct cad_kernel_private
{
	uint32_t module_cout;
	cad_module_info *modules;
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




cad_kernel * cad_kernel_New(uint32_t argc, char *argv[])
{
	cad_kernel *kernel = (cad_kernel *)malloc( sizeof(cad_kernel) ); 
	kernel->sys = (cad_kernel_private *)malloc( sizeof(cad_kernel_private) );

	kernel->PrintDebug = printf;
	kernel->PrintInfo = printf;
	kernel->Delete = kernel_Delete;
	kernel->Exec = kernel_Exec;

	return kernel;
}

