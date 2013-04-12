#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>
#include <math.h>

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
	m->sys->height = 0;
	m->sys->width = 0;
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
	int w = 70; 
	int h = 60; 
	SetPitcureSize(self, ceil((double)self->sys->width/w)*w+w, ceil((double)self->sys->width/w)*h+h);
	auto picture = allocate_picture(self);
	int size_square=picture->width/w; 
	memset(picture->data, 220, picture->width * picture->height * sizeof( uint32_t ));
	

	for (int y=size_square-1; y < picture->height; y++) // horizontal lines
		{
			for (int i=0; i<picture->width; i++)
			picture->data[picture->width * y + i] = 0xeeeeee; 
			y+=(size_square-1);
		}
	
	for (int z=size_square-1; z < picture->width; z++) //vertical lines
		{
			for (int i=0; i<picture->height; i++)
			picture->data[picture->width*i+z] = 0xeeeeee; 
			z+=(size_square-1);
		}
	//int n = MapElement3D(map, 0,0,map->currerntLayer);	
	return picture;

}
