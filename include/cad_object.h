// this file defines common structures for data exchange 
#pragma once

#include <stdint.h>
#include <memory>

#define NULL 0

struct	cad_scheme ;
struct  cad_scheme_private ;

struct cad_route_map ;
struct cad_route_map_private ;

struct cad_render_pixmap;
struct cad_module_info;

// describe 1 chip on the scheme
struct cad_chip 
{
	uint32_t num;			// chip number in the current schme
	uint32_t package_type;  // PT_DIP8, PT_DIP16 etc

	// next is filled by PLACEMENT module 

	uint32_t position;		// current position on the scheme 1, 2, 3.... 
							// real 2D coordinate depends on size of sheme. 
							// for plate 4 * 3 positions will be:
							//   1  5  9 
							// 0 2  6 10
							// 0 3  7 11
							//   4  8 12
							// where 0 is place for socket. 
	uint32_t orientation;	// possible values: CO_UP, CO_DOWN, CO_LEFT, CO_RIGHT, CO_DEFAULT

	// next is filled by MAP_GENERATOR module 

	uint32_t left_border;
	uint32_t top_border;

};

// pakage type
#define PT_DIP		0x01000000
#define PT_DIP8		(PT_DIP | 8)
#define PT_DIP12	(PT_DIP | 12)
#define PT_DIP16	(PT_DIP | 16)

#define PT_SLOT		0x02000000


// chip orientation
#define CO_UP		0
#define CO_DOWN		1
#define CO_LEFT		2
#define CO_RIGHT	3
#define CO_DEFAULT	CO_RIGHT

#define UNDEFINED_VALUE (0xFFFFFFFF)


// defines 1 wire, connecting many pins
struct cad_wire
{
	uint32_t endpoints_number;	

	//	chip chip[i] is connected to this wire with pin pin_number[i] 
	//	(i in range 0 .. endpoints_number - 1)

	uint32_t *pin_number;		
	cad_chip *chip;

};

// describe all connections on the scheme
struct cad_connections
{
	uint32_t number_of_wires;
	cad_wire *wire;
};

// describe whole sheme for PLACEMENT modules
// PLACEMENT module must initialize methods in this structure
// data must be filled by loader module before PLACEMENT initialization

struct	cad_scheme 
{
	cad_scheme_private *sys; // private data for Placement module

	// all data you need is here. 
	// thuse fields are filled before Open method call for PLACEMENT module
	
	uint32_t chip_number;			
	cad_chip *chips;				
	cad_connections connections;
	uint32_t	height;				// how many chips you can place in 1 column
	uint32_t	width;				// .... in 1 row


	// PLACEMENT module must initialize these methods :

	uint32_t ( * MakeStep)(cad_scheme *self);	// find place for 1 element and ruturn.
								// return MORE_ACTIONS if next MakeStep can be performed
								// return LAST_ACTION_OK if all is done.
								// return LAST_ACTION_ERROR if an error occured ( height * width < chip_number, etc. )

	uint32_t ( * Clear)(cad_scheme *self);		// clear current state. will be called before first MakeStep call
								// or by resert operation. All chips must be marked as unplaced

	uint32_t ( * AboutToDestroy)(cad_scheme *self);		// you must delete you private data here.
										// after call to it method structure will be deleted by kernel. 
										
};

#define MORE_ACTIONS		0x00000001
#define LAST_ACTION_OK		0x00000002
#define LAST_ACTION_ERROR	0x00000003


// all data for TRACEROUTE module

struct cad_route_map 
{
	cad_route_map_private *sys;

	// TRACEROUTE module doesn't need scheme actually
	// byt it is useful for RENDER module
	cad_scheme *sheme;

	// size of work field
	uint32_t height; 
	uint32_t width;
	uint32_t depth; // can be 1 or 2

	uint32_t *map;  // array. it is not very useful, so you can use macros RouteMap(row , column) 
					// instead of something like map[row][column]
					// for 3D routing use RouteMap(row , column, layer) 
					// possible values for this array are defined below

	// TRACEROUTE module must initialize these methods :

	uint32_t ( * MakeStep)(cad_route_map * self);	// make 1 algorithm step, fill data field 
								// return LAST_ACTION_OK if all is done.
								// return LAST_ACTION_ERROR if an error occured ( no path found, etc. )
								// LAST_ACTION_ERROR must be returned only if no one wire can be routed

	uint32_t ( * Clear)(cad_route_map * self);		// clear current state. will be called before first MakeStep call
								// or by resert operation. all places must be marked as unused 

	uint32_t ( * AboutToDestroy)(cad_route_map * self);		// you must delete you private data here.
										// after call to it method structure will be deleted by kernel. 

};


// values for cad_route_map::map

#define MAP_EMPTY				0x00000000
#define MAP_PIN					0x01000000		// MAP_PIN | wire_number
#define MAP_WIRE_HORISINTAL		0x02000000
#define MAP_WIRE_VERTICAL		0x03000000
#define MAP_WIRE_CROSS			0x04000000
#define MAP_WIRE_CORNER_UL		0x05000000
#define MAP_WIRE_CORNER_UR		0x06000000
#define MAP_WIRE_CORNER_DL		0x07000000
#define MAP_WIRE_CORNER_DR		0x08000000
#define MAP_ARROW_LEFT			0x09000000
#define MAP_ARROW_RIGHT			0x0A000000
#define MAP_ARROW_UP			0x0B000000
#define MAP_ARROW_DOWN			0x0C000000
#define MAP_NUMBER				0x0D000000		// MAP_NUMBER | number

// use RouteMap(i, j) = MAP_WIRE_HORISINTAL, 
// or  RouteMap(i, j) = MAP_NUMBER | ( number ); 

#define RouteMap(_i, _j) (self->map[(_i) * self->width + (_j)])
//#define RouteMap(_i, _j, _z) (self->map[(_z)* self->width * self->height  +  (_i) * self->width + (_j)])


struct cad_map_generator_private;

// map generator should have options for map size
struct cad_map_generator
{
	cad_map_generator_private *sys;
	
	cad_route_map * (* FillRouteMap)(cad_map_generator *self, cad_scheme *scheme);
	bool (* ReinitializeRouteMap)(cad_map_generator *self, cad_scheme *scheme, cad_route_map **map);

	void (* DestroyMap)(cad_map_generator *self, cad_route_map *map);
};


struct cad_render_module_private;
struct cad_picture_private;

struct cad_picture
{
	cad_picture_private *sys;
	
	uint32_t height;
	uint32_t width;

	uint32_t *data; // RGB array

	void (*Delete)(cad_picture *self);
};


struct cad_render_module
{
	cad_render_module_private *sys;
	
	void (* SetPitcureSize)(uint32_t width, uint32_t height);

	cad_picture *( *RenderSchme)(cad_scheme * scheme, float pos_x, float pos_y, float size_x, float size_y);
	cad_picture *( *RenderMap)(cad_route_map * map, float pos_x, float pos_y, float size_x, float size_y);

	void (* DeletePicture)(cad_picture *picture);
};


struct cad_GUI_private;

struct cad_GUI
{
	cad_GUI_private *sys;

	uint32_t ( * Exec)(cad_GUI *self);
	void (* SetCMDArgs)(cad_GUI *self, char *arg);

	void (* UpdatePictureEvent)( cad_GUI *self );
};


struct cad_access_module_private;

struct cad_access_module
{
	cad_access_module_private *sys;

	cad_scheme * ( *ReadSchme)( cad_access_module *self );
	cad_route_map * ( *ReadRouteMap)( cad_access_module *self );
};


struct  cad_kernel_private;

struct cad_kernel
{
	cad_kernel_private *sys;

	int (* PrintInfo)( const char *format ,...);
	int (* PrintDebug)( const char *format ,...);
	void (* Exec)(cad_kernel *self);
	void (* Delete)(cad_kernel *self);

	uint32_t (* GetCurrentState)(cad_kernel *self);
	uint32_t (* GetModuleList)(cad_kernel *self, cad_module_info **modules);

	bool (* LoadFile)(cad_kernel *self, const char *path);
	bool (* StartPlaceMoule)( cad_kernel *self, const char *force_module_name, bool demo_mode);
	bool (* StartTraceModule)( cad_kernel *self, const char *force_module_name, bool demo_mode);

	uint32_t (* NextStep)( cad_kernel *self) ;
	uint32_t (* RunToEnd)( cad_kernel *self) ;

	bool (* CloseCurrentFile)( cad_kernel *self);

	cad_picture * (*RenderPicture)(cad_kernel *self, float pos_x, float pos_y, float size_x, float size_y);
};

cad_kernel * cad_kernel_New(uint32_t argv, char *argc[]);

#define KERNEL_STATE_EMPTY			0x00000000
#define KERNEL_STATE_PLACE			0x00000001
#define KERNEL_STATE_TRACE			0x00000002
#define KERNEL_STATE_PLACING		0x00000003
#define KERNEL_STATE_TRACING		0x00000004
