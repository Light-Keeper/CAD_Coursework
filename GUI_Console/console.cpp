#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>

cad_GUI * Open(cad_kernel *, void *);
void *Close(cad_kernel*, cad_GUI *self);

cad_module_begin()
	set_module_name( "simple UI console module" )
	set_module_priority( 0 )
	set_module_capability( CAP_GUI )
	set_module_callbacks(Open, Close)
cad_module_end()

uint32_t gui_Exec(cad_GUI *self);
void gui_SetCMDArgs(cad_GUI *self, char *arg);
void gui_UpdatePictureEvent( cad_GUI *self );

struct cad_GUI_private
{
	cad_kernel *kernel;
	// other stuff...
};

cad_GUI * Open(cad_kernel * kernel, void *)
{
	cad_GUI *gui = ( cad_GUI* ) malloc( sizeof(cad_GUI) );
	gui->sys = (cad_GUI_private *) malloc( sizeof(cad_GUI_private) );
	cad_GUI_private *sys = gui->sys;

	sys->kernel = kernel;
	gui->Exec = gui_Exec;
	gui->SetCMDArgs = gui_SetCMDArgs;
	gui->UpdatePictureEvent = gui_UpdatePictureEvent;

	return gui;
}

void *Close(cad_kernel*, cad_GUI *self)
{
	free( self->sys );
	free( self );

	return NULL;
}

void gui_SetCMDArgs(cad_GUI *self, char *arg)
{
}

uint32_t gui_Exec(cad_GUI *self)
{
	cad_kernel *kernel = self->sys->kernel;
	uint32_t last_cmd = 0;

	while ( true )
	{
		while (last_cmd < 'a' || last_cmd > 'z') last_cmd = getc( stdin );
		if (last_cmd == 'q') break;

		switch (last_cmd)
		{
			case 'm': // show all placement and traceroute modules
				{
					cad_module_info *modules;
					uint32_t count = kernel->GetModuleList(kernel, &modules);
					uint32_t real_cout = 0;

					for (uint32_t i = 0; i < count; i++)
					{
						if (modules[i].module_capability != CAP_PLACEMENT ||
								modules[i].module_capability != CAP_TRACEROUTE) continue;
						char *type = modules[i].module_capability == CAP_PLACEMENT ? "CAP_PLACEMENT" : "CAP_TRACEROUTE";
						printf("module %3d %s\t priority = %d %s\n", i, type, modules[i].module_priority, modules[i].module_name);
						real_cout++;
					}
					printf("-----------------------------\ntotal modules: %d\n", real_cout);
				}
				break;

			case 'l': // load file
				kernel->LoadFile(kernel, "input_file.txt");
				break;
			case 't': // traceriute
				kernel->StartTraceModule( kernel, NULL, false);
				break;
			case 'p': // place 
				kernel->StartPlaceMoule( kernel, NULL, false);
				break;
			case 'n': // next step
				kernel->NextStep( kernel );
				break;
		}

		// skip rest of string
		while (last_cmd != '\n' ) last_cmd = getc( stdin );
	}
	
	return 0;
}

void gui_UpdatePictureEvent( cad_GUI *self )
{
	cad_picture *p = self->sys->kernel->RenderPicture(self->sys->kernel, 0,0,1,1);
	if ( p )
	{
		p->Delete( p );
	}
}