#include <cad_module.h>
#include <cad_object.h>
#include <cstdio>
#include <math.h>
#include <string>
#define S1 200
#define S2 100

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

int DrawLine(cad_picture * picture, int coord, int coordinf, int datainfo, int cycleinfobig, int cycleinfosmall, int iparam, int sqs_div2, int xcoord, int ycoord, uint32_t color, int cis, int active)
			{
				if (active == 1)
				{
			for (int j=0; j<=sqs_div2+cycleinfobig; j++)
			{ 
				for (int i = 0; i<cycleinfosmall; i++)
				picture->data[coord+datainfo+i*iparam] = color; 
				coord = coord + coordinf;
			}
			coord = picture->width*(ycoord)+xcoord;
			cycleinfosmall = cycleinfosmall - cis;
				}
			return coord;			
}

int DrawErrow(cad_picture * picture, int coord, int coordinf, int cycleinfobig, int cycleinfosmall, int iparam, int sqs_div2, int xcoord, int ycoord, uint32_t color, int cis, int par)
			{
			for (int j=0; j<=sqs_div2+cycleinfobig; j++)
			{ 
				for (int i = 0; i<cycleinfosmall; i++)
				picture->data[coord-cycleinfosmall/2*par+i*iparam] = color; 
				coord = coord + coordinf;
				cycleinfosmall = cycleinfosmall - cis;
			}
			coord = picture->width*(ycoord)+xcoord;
			return coord;			
}

void DrawSym(cad_picture * picture, int coord, int sqs_div2, int xcoord, int ycoord, uint32_t colour, int lu, int ld, int ru, int rd, int m, int u, int d, int su, int sd)
		{	int h = sqs_div2;
		DrawLine(picture, coord, -int(picture->width), -h/2, -1, 3, 1, sqs_div2, xcoord, ycoord, colour, 0, lu); // | left up
		DrawLine(picture, coord, picture->width, -h/2, -1, 3, 1, sqs_div2, xcoord, ycoord, colour, 0, ld); // | left down
		DrawLine(picture, coord, -int(picture->width), h/2-2, -1, 3, -1, sqs_div2, xcoord, ycoord, colour, 0, ru); // | right up	
		DrawLine(picture, coord, picture->width, h/2-2, -1, 3, -1, sqs_div2, xcoord, ycoord, colour, 0, rd); // right down
		DrawLine(picture, coord, 1, -int(picture->width)-h/2, -2, 3, picture->width, sqs_div2, xcoord, ycoord, colour, 0 ,m); // mid
		DrawLine(picture, coord, 1, -int(picture->width)*(h-1)-h/2, -2, 3, picture->width, sqs_div2, xcoord, ycoord, colour, 0, u);// up
		DrawLine(picture, coord, 1, picture->width*(h-3)-h/2, -2, 3, picture->width, sqs_div2, xcoord, ycoord, colour, 0, d);//down
		DrawLine(picture, coord, 1-int(picture->width), -int(picture->width)-h/2, -4, 3, 1, sqs_div2, xcoord, ycoord, colour, 0, su);// \up
		DrawLine(picture, coord, 1-int(picture->width), -int(picture->width)+picture->width*h-h/2-1, -3, 3, 1, sqs_div2, xcoord, ycoord, colour, 0, sd); // \down
		}



cad_picture *RenderMap(cad_render_module *self, cad_route_map * map, bool forceDrawLayer, uint32_t forceDrawLayerNunber)
{
	int w = S1; 
	int h = S2; 
	int width, height; uint32_t value; int xcoord, ycoord, coord,addw, addh;
	long map_test[S1][S2]; 
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
		map_test[0][16] = 0x30000000;
		map_test[0][17] = 0x40000000;
		map_test[0][18] = 0x10000000;
		map_test[0][19] = 0x20000000;
		map_test[0][20] = 0x60000000;
		map_test[0][21] = 0x80000020;
		uint32_t NUM = 0x80000000;
		for (int i=0; i<80; i++, NUM++)
			map_test[1][i] = NUM;
		for (int i=0; i<80; i++, NUM++)
			map_test[4][i] = NUM+100;

	if (ceil((double)self->sys->width/w)<25) //standartization if field is too little
	{
		width = 25*w+w-1; 
		height = 25*h+h-1;
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
	
//=================================================================================
	// HORIZONTAL LINES
//=================================================================================
	for (int y=size_square; y < (int)picture->height; ) // horizontal lines
		{
			for (int i=0;  i<(int)picture->width; i++)
				picture->data[(int)picture->width * y + i] = 0xeeeeee; 
			y+=(size_square+1);
		}
//=================================================================================
	// VERTICAL LINES
//=================================================================================
	for (int z=size_square; z < (int)picture->width;) //vertical lines
		{
			for (int i=0; i<(int)picture->height; i++)
			picture->data[(int)picture->width*i+z] = 0xeeeeee; 
			z+=(size_square+1);
		}
	//int n = MapElement3D(map, 0,0,map->currerntLayer);	

		int sqs = ((int)picture->width-2*w+1)/w+1;
	for (int r1 =0 ; r1<w; r1 ++ )
		for (int r2=0; r2<h; r2++)
		{
			int sqs_div2 = (sqs-1)/2;
			value = map_test[r1][r2];
			xcoord = sqs*(r2+1)+r2-(sqs_div2);
			ycoord = sqs*(r1+1)+r1-(sqs_div2);
			coord = picture->width*(ycoord)+xcoord;
	
//=====================================================
			//WIRE_UP
//=====================================================
if ((value & MAP_WIRE_UP) == MAP_WIRE_UP)			
coord = DrawLine(picture, coord, -int(picture->width), -1, 1, 3, 1, sqs_div2, xcoord, ycoord, 0xFF4500, 0,1); 	
//=====================================================
			//WIRE_DOWN
//=====================================================
if ((value & MAP_WIRE_DOWN) == MAP_WIRE_DOWN)	
coord = DrawLine(picture, coord, picture->width, -1, 0, 3, 1, sqs_div2, xcoord, ycoord, 0xFF4500, 0,1); 			
//=====================================================
			//WIRE_LEFT
//=====================================================
if ((value & 0xF4000000) == MAP_WIRE_LEFT)	
coord = DrawLine(picture, coord, -1, -int(picture->width), 1, 3, picture->width, sqs_div2, xcoord, ycoord, 0xFF4500, 0,1);			
//=====================================================
			//WIRE_RIGHT
//=====================================================
if ((value & 0xF8000000) == MAP_WIRE_RIGHT)	
coord = DrawLine(picture, coord, 1, -int(picture->width), 0, 3, picture->width, sqs_div2, xcoord, ycoord, 0xFF4500, 0,1);			
//=====================================================
			//ARROW_UP
//=====================================================
int h = (sqs+1)/2;
if ((value & CODE_MASK) == MAP_ARROW_UP)	
{
	coord = DrawErrow(picture, coord, -int(picture->width), 0, h, 1, sqs_div2, xcoord, ycoord, 0x000000, 2, 1);			
	coord = DrawLine(picture, coord, picture->width, -1, -sqs_div2/2, 3, 1, sqs_div2, xcoord, ycoord, 0x000000, 0,1);
}
//=====================================================
			//ARROW_DOWN
//=====================================================
if ((value & CODE_MASK) == MAP_ARROW_DOWN)	
{
	coord = DrawErrow(picture, coord, picture->width, 0, h, 1, sqs_div2, xcoord, ycoord, 0x000000, 2, 1);	
	coord = DrawLine(picture, coord, -int(picture->width), -1, -sqs_div2/2, 3, 1, sqs_div2, xcoord, ycoord, 0x000000, 0,1);			
}
//=====================================================
			//ARROW_LEFT
//=====================================================
if ((value & CODE_MASK) == MAP_ARROW_LEFT)	
{
	coord = DrawErrow(picture, coord, -1, 0, h, picture->width, sqs_div2, xcoord, ycoord, 0x000000, 2, picture->width);	
	coord = DrawLine(picture, coord, 1, -int(picture->width), -sqs_div2/2, 3, picture->width, sqs_div2, xcoord, ycoord, 0x000000, 0,1);
}
//=====================================================
			//ARROW_RIGHT
//=====================================================
if ((value & CODE_MASK) == MAP_ARROW_RIGHT)	
{
	coord = DrawErrow(picture, coord, 1, 0, h, picture->width, sqs_div2, xcoord, ycoord, 0x000000, 2, picture->width);	
	coord = DrawLine(picture, coord, -1, -int(picture->width), -sqs_div2/2, 3, picture->width, sqs_div2, xcoord, ycoord, 0x000000, 0,1);
}
//=====================================================
			//PIN
//=====================================================
h = (sqs_div2);
if ((value & CODE_MASK) == MAP_PIN)	
{
	coord = DrawErrow(picture, coord, -int(picture->width), 0, h, 1, sqs_div2, xcoord, ycoord, 0xFFD700, 2, 1);	
	coord = DrawErrow(picture, coord, picture->width, 0, h, 1, sqs_div2, xcoord, ycoord, 0xDAA520, 2, 1);	
}		
//=====================================================
			//NUMBER
//=====================================================
h = (sqs_div2);
			
		
	if ((value & CODE_MASK) == MAP_NUMBER)				
	{	
		char arr[]="   "; int cd; int n;
		uint32_t colour = 0x993344;
		int num = NUMBER_MASK & value;
		_itoa_s(num, arr,10);
		int col = strlen(arr);
		if (col==2)
		{
			cd = coord-h/2; 
			sqs_div2-=sqs_div2/3;
			n = h-h/3;
		}
		else if
			(col==3)
		{
			cd = coord-h+h/3; 
			n = sqs/3; 
			sqs_div2-=sqs_div2/3;
		} 
		else {
			cd = coord; 
			n = 0;
			sqs_div2-=sqs_div2/3;
		}
		for (int i=0; i<col; i++)
		{
		 if (arr[i]=='0')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 1, 1, 1,1,0,1,1,0,0);
		 else
		if (arr[i]=='1')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 0, 1,1,0,0,0,1,0);
		 else
			 if (arr[i]=='2')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 1, 1,0,1,1,1,0,0);
		 else
			 if (arr[i]=='3')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 0, 0,1,1,1,1,1,0);
		 else
			 if (arr[i]=='4')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 0, 1,1,1,0,0,1,0);
		 else
			 if (arr[i]=='5')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 1, 0, 0,1,1,1,1,0,0);
		 else
			 if (arr[i]=='6')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 1, 0,1,1,0,1,1,0);
		 else
			 if (arr[i]=='7')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 0, 1, 0,0,0,1,0,1,0);
		 else
			 if (arr[i]=='8')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 1, 1, 1,1,1,1,1,0,0);
		 else
			 if (arr[i]=='9')
			DrawSym(picture, cd, sqs_div2, xcoord, ycoord, colour, 1, 0, 1,0,1,1,0,0,1);
		 cd+=n;
		}
		}
		else continue;
		}  
	return picture;

}
