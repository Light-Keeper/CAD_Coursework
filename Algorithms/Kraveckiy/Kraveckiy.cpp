#include <cad_module.h>
#include <cad_object.h>

cad_route_map *Open(cad_kernel *c, cad_route_map *m);
cad_route_map *Close(cad_kernel *c, cad_route_map *m);

cad_module_begin()
	set_module_name( "Метод двух волн. Кравецкий" )
	set_module_priority( 0 )
	set_module_capability( CAP_TRACEROUTE )
	set_module_callbacks( Open, Close )
cad_module_end()

struct cad_route_map_private
{
	//допоши сюда свои данные
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
	m->sys = new cad_route_map_private;//structure cad_route_map_private
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
	
	for(int i=1;i<self->height-1;i++)
		for(int j=0; j<self->width;j++)
		{
			int x = MapElement3D(self, i, j, self->currerntLayer) ;
//			if (x) __asm int 3;
			if((MapElement3D(self, i, j, self->currerntLayer) & CODE_MASK) == MAP_PIN) 
			{
				MapElement3D(self, i-1, j, self->currerntLayer) = MAP_NUMBER | 1;
				MapElement3D(self,i+1,j,self->currerntLayer)=MAP_NUMBER | 2;
			}
		}
	return MORE_ACTIONS;
}