#include <cad_module.h>
#include <cad_object.h>

cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "ћетод длинной бороды. пример, как заполн€ть карту" )
	set_module_priority( 0 )
	set_module_capability( CAP_TRACEROUTE )
	set_module_callbacks( Open, Close )
cad_module_end()

struct cad_route_map_private
{
	cad_kernel *kernel;
};

uint32_t AboutToDestroy(cad_route_map *self)
{
	delete self->sys;
	self->sys = NULL;
	self->AboutToDestroy = NULL;
	self->MakeStepInDemoMode = NULL;
	return 0;
}

uint32_t Clear(cad_route_map *self)
{
	return 0;
}

uint32_t MakeStepInDemoMode( cad_route_map *self);

cad_route_map *Open(cad_kernel *c, cad_route_map *m)
{
	m->sys = new cad_route_map_private;
	m->sys->kernel = c;
	m->AboutToDestroy = AboutToDestroy;
	m->MakeStepInDemoMode = MakeStepInDemoMode; 
	m->Clear = Clear;
	m->Clear( m );
	return m;
}

cad_route_map *Close(cad_kernel *c, cad_route_map *m)
{
	return NULL;
}

uint32_t MakeStepInDemoMode( cad_route_map *self)
{
	for (int i = 0; i < 11; i++)
		for (int j = 0; j < 10; j++)
			MapElement3D(self, i, j, self->currerntLayer) = MAP_NUMBER | (i * 10 + j);

	return MORE_ACTIONS;
}