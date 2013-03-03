#include <cad_module.h>
#include <cad_object.h>

cad_GUI * Open(cad_kernel *, void *);
void *Close(cad_kernel*, void *);

cad_module_begin()
	set_module_name( "simple UI console module" )
	set_module_priority( 0 )
	set_module_capability( CAP_GUI )
	set_module_callbacks(Open, Close)
cad_module_end()


cad_GUI * Open(cad_kernel *, void *)
{
	return NULL;
}

void *Close(cad_kernel*, void *)
{
	return NULL;
}