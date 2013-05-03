#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>

cad_access_module * Open(cad_kernel *, char *fileName);
void *Close(cad_kernel* kernel, cad_access_module *self);

cad_module_begin()
	set_module_name( "File access module. File version 1.0" )
	set_module_priority( 0 )
	set_module_capability( CAP_ACCESS )
	set_module_callbacks(Open, Close)
cad_module_end()

struct cad_access_module_private
{
	static const int BUFFER_SIZE = 10 * 1024;
	cad_kernel *kernel;
	char *fileName;
	char buffer[BUFFER_SIZE];

	uint32_t position;
};

void access_delete_scheme( cad_scheme *scheme);
void access_delete_map( cad_route_map *map);
void access_ReadAll( cad_access_module *self,cad_scheme **scheme, cad_route_map **route_map );
bool access_realloc_map(cad_route_map *self, uint32_t newDepth);

cad_access_module * Open(cad_kernel * kernel, char *fileName)
{
	cad_access_module *access = (cad_access_module *)malloc( sizeof(cad_access_module) );
	
	access->sys = (cad_access_module_private *) malloc( sizeof(cad_access_module_private) );
	access->ReadAll = access_ReadAll;

	access->sys->kernel = kernel;
	access->sys->fileName = (char *)malloc( strlen( fileName ) + 1);
	strcpy(access->sys->fileName, fileName);

	return access;
}


void *Close(cad_kernel* kernel, cad_access_module *self)
{
	free( self->sys->fileName );
	free( self->sys );
	free( self );

	return NULL;
}

bool access_ReadElement(cad_access_module *self, char *buffer, cad_scheme *scheme, cad_route_map *map)
{
	uint32_t number, type1(0), type2(0);

	sscanf(buffer, "D%d DIP%d\n", &number, &type1);
	sscanf(buffer, "D%d SOCKET%d\n", &number, &type2);
		
	if (type1 == 0 && type2 == 0)
	{
		self->sys->kernel->PrintDebug( "Invalid file %s.\nPackage type must be DIPx or SOCKETx, where x > 0", 
			self->sys->fileName);
		return false;
	}

	uint32_t type = (type1 != 0) ? (PT_DIP | type1) : (PT_SOCKET | type2);
	scheme->chip_number++;
	scheme->chips = (cad_chip *)realloc(scheme->chips, sizeof(cad_chip) * scheme->chip_number);
	cad_chip *c = &scheme->chips[ scheme->chip_number - 1 ];
	
	c->left_border	= UNDEFINED_VALUE;
	c->top_border	= UNDEFINED_VALUE;
	c->orientation	= UNDEFINED_VALUE;
	c->position		= UNDEFINED_VALUE;
	c->num			= number;
	c->package_type = type;

	return true;
}

bool access_ReadConnection(cad_access_module *self, char *buffer, cad_scheme *s, cad_route_map *map)
{
	s->connections.number_of_wires++;
	s->connections.wire = (cad_wire *)realloc( s->connections.wire, sizeof( cad_wire ) * s->connections.number_of_wires );
	cad_wire *w = &s->connections.wire[ s->connections.number_of_wires - 1];
	w->chips = NULL;
	w->pin_numbers = NULL;
	w->endpoints_number = 0;
	w->number = s->connections.number_of_wires - 1;

	for (char *pos = buffer; *pos != 0; pos++)
	{
		if (*pos != 'D') continue;
		uint32_t number = UNDEFINED_VALUE;
		uint32_t pin = UNDEFINED_VALUE;

		// read data from string, add it to wire
		sscanf(pos, "D%d.%d", &number, &pin );
		
		if (number == UNDEFINED_VALUE || pin == UNDEFINED_VALUE)
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\n  D%%d.%%d format expected\n", self->sys->fileName );
			return false;
		}

		w->endpoints_number++;
		w->pin_numbers = (uint32_t *)realloc(w->pin_numbers, sizeof(uint32_t) * w->endpoints_number);
		w->chips = (cad_chip **)realloc(w->chips, sizeof(cad_chip *) * w->endpoints_number);
		w->pin_numbers[ w->endpoints_number - 1 ] = pin;
		w->chips[ w->endpoints_number - 1] = NULL;
		for (uint32_t i = 0; i < s->chip_number && w->chips[ w->endpoints_number - 1] == NULL; i++)
			if (s->chips[ i ].num == number) w->chips[ w->endpoints_number  - 1] = &s->chips[ i ];

		if (w->chips[ w->endpoints_number - 1] == NULL)
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\n  chip D%d undefined in section [Connections]\n", 
				self->sys->fileName, number );
			return false;
		}
	}

	return (w->pin_numbers != NULL);
}

bool access_ReadLayout(cad_access_module *self, char *str, cad_scheme *s, cad_route_map *map)
{
	uint32_t t;
	if (sscanf(str, "field-rows=%d", &t) == 1)
	{
		s->height = t;
		return true;
	}

	if (sscanf(str, "field-columns=%d", &t) == 1)
	{
		s->width = t;
		return true;
	}

	if (s->height == 0 || s->width == 0) return false;

	for (char *pos = str; *pos != 0; pos++)
	{	
		if (*pos == 'X')
		{
			self->sys->position += s->height;
			if (self->sys->position > s->width * s->height) 
				self->sys->position = self->sys->position - s->width * s->height + 1;
		}

		if (*pos != 'D') continue;
		uint32_t number = UNDEFINED_VALUE;

		sscanf(pos, "D%d", &number);
		
		if (number == UNDEFINED_VALUE )
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\n  Dx format expected in [Positions] section\n", self->sys->fileName );
			return false;
		}

		cad_chip *c = NULL;
		for (uint32_t i = 0; i < s->chip_number && c == NULL; i++)
			if ( s->chips[i].num == number) c = &s->chips[i];
		
		if (c == NULL)
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\n undefined chip №%d referenced in [Positions] section\n", 
				self->sys->fileName, number );
			return false;
		}
		
		if (c->position != UNDEFINED_VALUE)
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\n"
					"chip %d placed at %d and %d positions\n",
					self->sys->fileName, c->num, c->position, self->sys->position );
			return false;
		}

		c->position = self->sys->position;
		self->sys->position += s->height;
			if (self->sys->position > s->width * s->height) 
				self->sys->position = self->sys->position - s->width * s->height + 1;
	}

	return true;
}

bool access_ReadMap(cad_access_module *self, char *str, cad_scheme *s, cad_route_map *map)
{
	uint32_t t;
	if (sscanf(str, "map-height=%d", &t) == 1)
	{
		map->height = t;
		return true;
	}

	if (sscanf(str, "map-width=%d", &t) == 1)
	{
		map->width = t;
		return true;
	}

	if (sscanf(str, "map-depth=%d", &t) == 1)
	{
		map->depth = t;
		return true;
	}
	if ( map->depth == 0 ) map->depth = 1;
	if ( map->height == 0 || map->width == 0 ) return false;

	if (map->map == NULL)
	{
		map->map = (uint32_t *)malloc(sizeof(uint32_t) *  map->depth * map->height * map->width);
		if (map->map == NULL)
		{
			self->sys->kernel->PrintDebug( "Can not allocate memory for route_map" ); 
			return false;
		}
	}

	uint32_t d, left, top;

	if ( sscanf(str, "D%d%d%d", &d, &left, &top) != 3) return false;
	
	cad_chip *c = NULL;
		for (uint32_t i = 0; i < s->chip_number && c == NULL; i++)
			if ( s->chips[i].num == d) c = &s->chips[i];
	
	if (c == NULL) return false;
	c->orientation = CO_DEFAULT;
	c->top_border = top;
	c->left_border = left;
	return true;
}

uint32_t access_ReadBlock(cad_access_module *self, FILE *f, char *blockName, 
				bool (* read_f)(cad_access_module *self, char *str, cad_scheme *s, cad_route_map *map), 
				cad_scheme *s, cad_route_map *map)
{
	int BUFFER_SIZE = cad_access_module_private::BUFFER_SIZE;
	char *buffer = self->sys->buffer;
	char *utf8Hack = buffer;

	do {
		if ( fgets(buffer, BUFFER_SIZE, f) == NULL )
			return E_NOT_FOUND;
		utf8Hack = ( *(unsigned short *)buffer) == 0xBBEF ? buffer + 3 : buffer; 
	} while( memcmp(utf8Hack, blockName, sizeof( blockName )) != 0 );
	
	while ( 1 ) 
	{
		if ( fgets(buffer, BUFFER_SIZE, f) == NULL ) return E_OK; // end of file
		if ( buffer[0] == '\n' ) return E_OK; // end of block
		if ( buffer[0] == '#') continue; // comment
		if (! read_f(self, buffer, s, map) ) return E_ERROR; 
	}
}

void access_ReadAll( cad_access_module *self,cad_scheme **scheme, cad_route_map **route_map )
{
	*scheme = NULL;
	*route_map = NULL;
	self->sys->position = 1;

	FILE *file = fopen(self->sys->fileName, "rt");
	if (file == NULL) return ;

	cad_scheme *_scheme = (cad_scheme *)malloc( sizeof(cad_scheme) );
	memset( _scheme, 0, sizeof( cad_scheme ));
	_scheme->Delete = access_delete_scheme;

	cad_route_map * _map = (cad_route_map *)malloc( sizeof(cad_route_map) );
	memset( _map, 0, sizeof( cad_route_map ));
	_map->Delete = access_delete_map;
	_map->ReallocMap = access_realloc_map;

	__try
	{
		if (E_OK != access_ReadBlock(self, file, "[Scheme description 1.0]"	, access_ReadElement	, _scheme, _map) ) return;
		if (E_OK != access_ReadBlock(self, file, "[Connections]"			, access_ReadConnection	, _scheme, _map) ) return;
		if (E_ERROR == access_ReadBlock(self, file, "[Positions]"			, access_ReadLayout		, _scheme, _map) ) return;
		if (E_ERROR == access_ReadBlock(self, file, "[Map Coordinates]"		, access_ReadMap		, _scheme, _map) ) return;
		*scheme = _scheme;
		*route_map = _map;
	}
	__finally
	{
		if ( *scheme == NULL ) _scheme->Delete( _scheme );
		if ( *route_map == NULL || _map->height == 0) _map->Delete( _map ), *route_map = NULL;
		else _map->sheme = _scheme;
		fclose( file );
	}
}

bool access_realloc_map(cad_route_map *self, uint32_t newDepth)
{
	if (newDepth == self->depth) return true;
	self->map = (uint32_t *)::realloc( self->map, newDepth * self->width * self->height * sizeof(* self->map ) );
	if (self->map == NULL) return false;

	for (uint32_t t = self->depth; t < newDepth; t++)
	{
		for (uint32_t i = 0; i < self->height; i++)
			for (uint32_t j = 0; j < self->width; j++)
			{
				MapElement3D(self,i,j,t) = 
					MapElement3D(self,i,j,0) & MAP_PIN ? MapElement3D(self,i,j,0) : MAP_EMPTY;
			}	
	}
	self->depth = newDepth;
	return true;
}

void access_delete_scheme( cad_scheme *scheme)
{
	if (scheme->chips) free(scheme->chips);
	for (uint32_t i = 0; i < scheme->connections.number_of_wires; i++)
	{
		free ( scheme->connections.wire[i].chips );
		free ( scheme->connections.wire[i].pin_numbers );
	}

	free (scheme->connections.wire );
	free ( scheme );
}

void access_delete_map( cad_route_map *map)
{
	if ( map->map ) free( map->map );
	free( map );
}
