#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>

cad_GUI * Open(cad_kernel *, void *);
void *Close(cad_kernel*, cad_GUI *self);

cad_module_begin()
	set_module_name( ".NET User interface" )
	set_module_priority( 1 )
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
		break;
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