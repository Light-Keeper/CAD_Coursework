#include <cad_module.h>
#include <cad_object.h>
#include <map>

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

struct cad_map_generator_private
{
	cad_kernel *kernel;
	std::map<uint32_t, uint32_t> *Wires;
};

bool validate_place(cad_map_generator *self, cad_route_map *map, uint32_t r, uint32_t c, uint32_t rr, uint32_t cc)
{
	if (r < 0 || c < 0 || r + rr > map->height || c + cc > map->width) return false;

	for (uint32_t i = r; i < r + rr; i++)
		for (uint32_t j = c; j < c + cc; j++)
			if ( MapElement(map, i, j) != MAP_EMPTY) return false; 
	
	return true;
}

bool generator_PlaceChip_Right(cad_map_generator *self, cad_route_map *map, cad_chip *chip)
{
	uint32_t one_side_pins = (chip->package_type & NUMBER_MASK) / 2;

	if (false == validate_place( self, map, chip->top_border, chip->left_border,	4, one_side_pins * 2 - 1))
	{
		self->sys->kernel->PrintDebug( "No place for element %d\n", chip->num );
		return false;
	}

	uint32_t px1 = one_side_pins;
	uint32_t px2 = px1 + 1;

	for (uint32_t i = chip->left_border; i < chip->left_border + one_side_pins * 2; i += 2)
	{
		auto f = [&](uint32_t px, uint32_t row )
		{
			int code = MAP_UNUSED;
			auto x = self->sys->Wires->find( (chip->num << 16) |  px );
			if (x != self->sys->Wires->end()) code = MAP_PIN | x->second;
			for (uint32_t j = 0; j < map->depth; j++)
				MapElement3D(map, row, i, j) = code;
		};
		
		f( px1--, chip->top_border ); 
		f( px2++, chip->top_border + 3);
	}

	return true;
}

bool generator_PlaceSocket(cad_map_generator *self, cad_route_map *map, cad_chip *chip)
{
	if (false == validate_place( self, map, chip->top_border, chip->left_border,	
					(chip->package_type & NUMBER_MASK) * 2 - 1 , 2 ))
	{
		self->sys->kernel->PrintDebug( "No place for slot %d\n", chip->num );
		return false;
	}

	uint32_t px = 1;

	for(uint32_t i = chip->top_border; i < chip->top_border + (chip->package_type & NUMBER_MASK) * 2; i += 2 )
	{
		int code = MAP_UNUSED;
		auto x = self->sys->Wires->find( (chip->num << 16) |  px );
		if (x != self->sys->Wires->end()) code = MAP_PIN | x->second;
		for (uint32_t j = 0; j < map->depth; j++)
			MapElement3D(map, i, chip->left_border + (i / 2) % 2 , j) = code;
	
		px++;
	}

	return true;
}

bool generator_FillMap(cad_map_generator *self, cad_route_map *map)
{
	for (uint32_t i = 0; i < map->height; i++)
		for (uint32_t j = 0; j < map->width; j++)
			for(uint32_t z = 0; z < map->depth; z++)
				MapElement3D(map, i, j, z) = MAP_EMPTY;

	self->sys->Wires->clear();
	for (uint32_t i = 0; i < map->sheme->connections.number_of_wires; i++)
	{
		for(uint32_t j = 0; j < map->sheme->connections.wire[ i ].endpoints_number; j++)
		{
			uint32_t x  =  map->sheme->connections.wire[ i ].chips[ j ]->num << 16;
			x |=  map->sheme->connections.wire[ i ].pin_numbers[ j ];
			self->sys->Wires->insert(std::pair<uint32_t, uint32_t>( x, i ));
		}
	}

	for (uint32_t i = 0; i < map->sheme->chip_number; i++)
	{
		cad_chip *c = &map->sheme->chips [ i ];
		
		if ((c->package_type & CODE_MASK) == PT_SOCKET)
		{
			c->orientation = CO_UP;
			if ( ! generator_PlaceSocket(self, map, c)) return false;
		}
		else // if PT_DIP
		{
			// support only CO_RIGHT
			c->orientation = CO_RIGHT;
			if (c->orientation == CO_UP		) if ( ! generator_PlaceChip_Right(self, map, c)) return false;
			if (c->orientation == CO_LEFT	) if ( ! generator_PlaceChip_Right(self, map, c)) return false;
			if (c->orientation == CO_RIGHT	) if ( ! generator_PlaceChip_Right(self, map, c)) return false;
			if (c->orientation == CO_DOWN	) if ( ! generator_PlaceChip_Right(self, map, c)) return false;
		}
	}

	self->sys->Wires->clear();
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

cad_map_generator * Open(cad_kernel * kernel, void *)
{
	cad_map_generator *gen = (cad_map_generator *)malloc( sizeof(cad_map_generator) );
	gen->sys = (cad_map_generator_private *)malloc( sizeof( cad_map_generator_private ) );
	gen->sys->kernel = kernel;
	gen->sys->Wires = new std::map<uint32_t, uint32_t>();

	gen->ReinitializeRouteMap = generator_ReinitializeRouteMap;
	return gen;
}


void *Close(cad_kernel*, cad_map_generator *self)
{
	delete self->sys->Wires;
	free( self->sys );
	free( self );
	return NULL;
}
