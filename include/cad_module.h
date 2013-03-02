// you need it for intermodule communication
#pragma once

#include <cad_object.h>

#define OPTT_CHECKBOX 0x00000001

struct cad_module_option
{
	char *		option_header;
	uint32_t	option_type;
	uint32_t	option_state;

} ;

struct cad_module_info
{
	char *	module_name;

	uint32_t module_priority;
	uint32_t module_capability;

	void * ( *Open)(cad_kernel *, void *);
	void * ( *Close)(cad_kernel *, void *);

} ;


// each module must be declared with following macroses

#define cad_module_begin() \
		uint32_t startup_module_function(cad_module_info * mf ) { \
				mf->module_name = NULL; \
				mf->module_priority =  mf->module_capability = 0;


#define cad_module_end() }

#define set_module_capability( cap ) mf->module_capability = (cap);
#define set_module_name( name ) mf->module_name = (name);
#define set_module_priority( prior ) mf->module_priority = (prior);

#define set_module_callbacks(open, close) mf->Open = (uint32_t (*)(cad_kernel *, void *)) open; \
											mf->Close = (uint32_t (*)(cad_kernel *, void *)) close;

// module capabilities
#define CAP_PLACEMENT		0x00000001
#define CAP_TRACEROUTE		0x00000002
#define CAP_MAP_GENERATOR	0x00000003
#define CAP_RENDER			0x00000004
#define CAP_GUI				0x00000005
#define CAP_ACCESS			0x00000006

// example of use:

//uint32_t Open(cad_kernel * char *);
//uint32_t Close(cad_kernel * char *);
//
//cad_module_begin()
//	set_module_name( "размещение с1 к3 (Янушкевич В.А.)" )
//	set_module_capability( CAP_PLACEMENT )
//	set_module_priority( 0 )
//	set_module_callbacks(Open, Close)
//cad_module_end()


