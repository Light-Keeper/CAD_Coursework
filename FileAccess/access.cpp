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
	cad_kernel *kernel;
	char *fileName;
};

cad_scheme * access_ReadSchme( cad_access_module *self );
cad_route_map * access_ReadRouteMap( cad_access_module *self );

cad_access_module * Open(cad_kernel * kernel, char *fileName)
{
	cad_access_module *access = (cad_access_module *)malloc( sizeof(cad_access_module) );
	
	access->sys = (cad_access_module_private *) malloc( sizeof(cad_access_module_private) );
	access->ReadRouteMap = access_ReadRouteMap;
	access->ReadSchme = access_ReadSchme;

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


bool internal_SeekToBlock(FILE *file, char *name)
{
	char *buffer = (char *)malloc( 1024 );	
	do{
		if ( fgets(buffer, 1024, file) == NULL ) break;
	} while( memcmp(buffer, name, sizeof(name) != 0) );

	bool result = memcmp(buffer, name, strlen( name ) ) != 0;
	free(buffer);
	return result;
}

cad_scheme * access_ReadSchme( cad_access_module *self )
{
	FILE *file = fopen(self->sys->fileName, "rt");
	if (file == NULL) return NULL;

	if (! internal_SeekToBlock(file, "Scheme description 1.0") )
	{
		fclose( file );
		return NULL;
	}

	char buffer[ 1024 ];	

	cad_scheme *scheme = (cad_scheme *)malloc( sizeof(cad_scheme) );
	memset( scheme, 0, sizeof( cad_scheme ));

	do
	{
		uint32_t number;
		uint32_t type1 = 0;
		uint32_t type2 = 0;

		if ( fgets(buffer, 1024, file) == NULL ) break;
		
		sscanf(buffer, "D%d DIP%d\n", &number, &type1);
		sscanf(buffer, "D%d SOCKET%d\n", &number, &type2);
		
		if (type1 == 0 && type2 == 0)
		{
			self->sys->kernel->PrintDebug( "Invalid file %s.\nPackage type must be DIPx or SOCKETx, where x > 0" );
			fclose( file );
			scheme->Delete( scheme );
			return NULL;
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
	
	} while ( *buffer != '\n' );	

	fclose( file );
	return NULL;
}


cad_route_map * access_ReadRouteMap( cad_access_module *self )
{
	return NULL;
}
