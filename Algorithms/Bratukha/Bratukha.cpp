#include <cad_module.h>
#include <cad_object.h>

cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "Трассировка методом Аккерса. Братуха" )
	set_module_priority( 0 )
	set_module_capability( CAP_TRACEROUTE )
	set_module_callbacks(Open, Close)
cad_module_end()

cad_route_map *Open(cad_kernel *c, cad_route_map *m)
{
	return NULL;
}

cad_route_map *Close(cad_kernel *c, cad_route_map *m)
{
	return NULL;
}