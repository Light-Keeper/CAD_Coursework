#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>

cad_render_module * Open(cad_kernel *, void *);
void *Close(cad_kernel* kernel, cad_render_module *self);

cad_module_begin()
	set_module_name( "Map generator module. WinAPI implementation." )
	set_module_priority( 0 )
	set_module_capability( CAP_RENDER )
	set_module_callbacks(Open, Close)
cad_module_end()

void SetPitcureSize(cad_render_module *self, uint32_t width, uint32_t height);
cad_picture *RenderSchme(cad_render_module *self, cad_scheme * scheme);
cad_picture *RenderMap(cad_render_module *self, cad_route_map * map, bool forceDrawLayer, uint32_t forceDrawLayerNunber);

struct cad_render_module_private 
{
	cad_kernel *kernel;
	uint32_t width;
	uint32_t height;
};

cad_render_module * Open(cad_kernel *, void *)
{
	cad_render_module *m = (cad_render_module *)malloc( sizeof(cad_render_module) );
	m->sys = (cad_render_module_private *)malloc( sizeof(cad_render_module_private) );
	m->SetPitcureSize = SetPitcureSize;
	m->RenderMap = RenderMap;
	m->RenderSchme = RenderSchme;
	// default values
	m->sys->height = 786;
	m->sys->width = 1024;
	return m;
}

void *Close(cad_kernel* kernel, cad_render_module *self)
{
	free(self->sys);
	free(self);
	return NULL;
}


void SetPitcureSize(cad_render_module *self, uint32_t width, uint32_t height)
{
	self->sys->width = width;
	self->sys->height = height;
}

void DeletePicture(cad_picture *p)
{
	if (p->sys) free(p->sys );
	free( p );
}

cad_picture *allocate_picture(cad_render_module *self)
{
	cad_picture *picture = (cad_picture *)malloc( sizeof(cad_picture) );
	picture->Delete = DeletePicture;
	picture->height = self->sys->height;
	picture->width = self->sys->width;
	picture->data = (uint32_t *)malloc( sizeof( uint32_t) *picture->width * picture->height);
	picture->sys = NULL;
	return picture;
}

cad_picture *RenderSchme(cad_render_module *self, cad_scheme * scheme)
{
	return allocate_picture(self);
}

cad_picture *RenderMap(cad_render_module *self, cad_route_map * map, bool forceDrawLayer, uint32_t forceDrawLayerNunber)
{
	return allocate_picture(self);
}
