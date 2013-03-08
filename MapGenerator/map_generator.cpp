#include <cad_module.h>
#include <cad_object.h>

cad_map_generator * Open(cad_kernel *, void *);
void *Close(cad_kernel*, cad_map_generator *self);


// this realisation can not create new map and ignore elements orientation. 
// but this should be implemented

cad_module_begin()
	set_module_name( "map generate module" )
	set_module_capability(CAP_MAP_GENERATOR)
	set_module_priority( 0 );
	set_module_callbacks(Open,Close)
cad_module_end()



bool generator_FillMap(cad_map_generator *self, cad_route_map *map)
{
	for (uint32_t i = 0; i < map->height; i++)
		for (uint32_t j = 0; j < map->height; j++)
			for(uint32_t z = 0; z < map->depth; z++)
//				MapElement3D(map, i, j, z) = MAP_EMPTY;
			;
	// place pins, only CO_RIGHT support
	// TODO: implement
	
	return true;

}

bool generator_ReinitializeRouteMap(cad_map_generator *self, cad_scheme *scheme, cad_route_map **map)
{
	cad_route_map *m = *map;
	if (m == NULL) return false;
	if (m->height == 0 || m->width == 0 || m->height == 0) return false;
	if (scheme != m->sheme) return false;

	for (uint32_t i = 0; i < m->sheme->chip_number; i++)
	{
		if (m->sheme->chips[ i ].left_border == UNDEFINED_VALUE ||
			m->sheme->chips[ i ].top_border == UNDEFINED_VALUE ) return false;
	}

	return generator_FillMap( self, m);	
}

cad_map_generator * Open(cad_kernel *, void *)
{
	cad_map_generator *gen = (cad_map_generator *)malloc( sizeof(cad_map_generator) );
	gen->sys = NULL;
	gen->ReinitializeRouteMap = generator_ReinitializeRouteMap;
	return gen;
}


void *Close(cad_kernel*, cad_map_generator *self)
{
	free( self );
	return NULL;
}
