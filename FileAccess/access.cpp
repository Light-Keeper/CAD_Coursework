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


cad_scheme * access_ReadSchme( cad_access_module *self )
{
	return NULL;
}


cad_route_map * access_ReadRouteMap( cad_access_module *self )
{
	return NULL;
}
