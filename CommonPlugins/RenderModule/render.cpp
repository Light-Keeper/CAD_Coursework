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
	int w = 100; 
	int h = 50; 
	int width, height, value, xcoord, ycoord, coord,addw, addh;
	long map_test[100][50]; 
		map_test[0][0] = 0x00000000; 
		map_test[0][1] = 0x01000000; 
		map_test[0][2] = 0x02000000; 
		map_test[0][3] = 0x03000000; 
		map_test[0][4] = 0x04000000; 
		map_test[0][5] = 0x05000000; 
		map_test[0][6] = 0x06000000; 
		map_test[0][7] = 0x07000000; 
		map_test[0][8] = 0x08000000; 
		map_test[0][9] = 0x09000000; 
		map_test[0][10] = 0x0A000000; 
		map_test[0][11] = 0x0B000000; 
		map_test[0][12] = 0x0C000000; 
		map_test[0][13] = 0x0D000000;
		map_test[0][14] = 0x0E000000;
		map_test[0][15] = 0x0F000000;
	if (ceil((double)self->sys->width/w)<21) //standartization if field is too little
	{
		width = 21*w+w-1; 
		height = 21*h+h-1;
	}
	else 
	{
		if (((int)ceil((double)self->sys->width/w)%2)==0)
			{addw=w*2;
		addh=h*2;}
		else
		{	addw=w;
		addh=h;}
		width = (int)ceil((double)self->sys->width/w)*w+addw-1;
		height = (int)ceil((double)self->sys->width/w)*h+addh-1; 
	}

	SetPitcureSize(self, width, height);
	auto picture = allocate_picture(self);
	int size_square=(picture->width-w+1)/w; 
	memset(picture->data, 220, picture->width * picture->height * sizeof( uint32_t ));
	

	for (int y=size_square; y < (int)picture->height; ) // horizontal lines
		{
			for (int i=0;  i<(int)picture->width; i++)
				picture->data[(int)picture->width * y + i] = 0xeeeeee; 
			y+=(size_square+1);
		}
	
	for (int z=size_square; z < (int)picture->width;) //vertical lines
		{
			for (int i=0; i<(int)picture->height; i++)
			picture->data[(int)picture->width*i+z] = 0xeeeeee; 
			z+=(size_square+1);
		}
	//int n = MapElement3D(map, 0,0,map->currerntLayer);	

		int sqs = ((int)picture->width-2*w+1)/w+1;
		int sqs_div2 = (sqs-1)/2;
	for (int r1 =0 ; r1<w; r1 ++ )
		for (int r2=0; r2<h; r2++)
		{
			value = map_test[r1][r2];
			xcoord = sqs*(r2+1)+r2-(sqs_div2);
			ycoord = sqs*(r1+1)+r1-(sqs_div2);
			coord = picture->width*(ycoord)+xcoord;
		if ((value & 0xF1000000) == 0x01000000)	
			for (int j=0; j<=sqs_div2; j++)
			{ 
				for (int i = 0; i<3; i++)
				picture->data[coord-1+i] = 0xFF4500; 
				coord = coord - picture->width;
			}
			coord = picture->width*(ycoord)+xcoord;
		if ((value & 0xF2000000) == 0x02000000)	
			for (int j=0; j<=sqs_div2; j++)
			{ 
				for (int i = 0; i<3; i++)
				picture->data[coord-1+i] = 0xFF4500; 
				coord = coord + picture->width;
			}
			coord = picture->width*(ycoord)+xcoord;
		if ((value & 0xF4000000) == 0x04000000)	
			for (int j=0; j<=sqs_div2; j++)
			{ 
				for (int i = 0; i<3; i++)
				picture->data[coord-picture->width+picture->width*i] = 0xFF4500; 
				coord = coord - 1;
			}
			coord = picture->width*(ycoord)+xcoord;
		if ((value & 0xF8000000) == 0x08000000)	
			for (int j=0; j<=sqs_div2; j++)
			{ 
				for (int i = 0; i<3; i++)
				picture->data[coord-picture->width+picture->width*i] = 0xFF4500; 
				coord = coord + 1;
			}
		else continue;
		} 
	return picture;

}
