#include <cad_module.h>
#include <cad_object.h>

#include <windows.h>
#include <metahost.h>

#import "mscorlib.tlb" raw_interfaces_only				\
    high_property_prefixes("_get","_put","_putref")		\
    rename("ReportEvent", "InteropServices_ReportEvent")
using namespace mscorlib;
#pragma endregion


cad_GUI * Open(cad_kernel *, void *);
void *Close(cad_kernel*, cad_GUI *self);

cad_module_begin()
	set_module_name( ".NET user interface" )
	set_module_priority( 10 )
	set_module_capability( CAP_GUI )
	set_module_callbacks(Open, Close)
cad_module_end()

uint32_t gui_Exec(cad_GUI *self);
void gui_SetCMDArgs(cad_GUI *self, char *arg);
void gui_UpdatePictureEvent( cad_GUI *self );

bool InitCLR(cad_GUI *self);
bool DestroyCLR(cad_GUI *self);

static cad_GUI *self = NULL;

struct cad_GUI_private
{
	cad_kernel *kernel;

	ICLRMetaHost *pMetaHost ;
	ICLRRuntimeInfo *pRuntimeInfo ;
	ICLRRuntimeHost *pClrRuntimeHost;
};

cad_GUI * Open(cad_kernel * kernel, void *)
{
	cad_GUI *gui = ( cad_GUI* ) malloc( sizeof(cad_GUI) );
	gui->sys = (cad_GUI_private *) malloc( sizeof(cad_GUI_private) );
	cad_GUI_private *sys = gui->sys;

	sys->kernel = kernel;
	gui->Exec = gui_Exec;
	gui->SetCMDArgs = gui_SetCMDArgs;
	gui->UpdatePictureEvent = gui_UpdatePictureEvent;
	
	if (! InitCLR( gui ) )
	{
		kernel->PrintDebug("Can not start .NET, CLR 2.0 or 4.0 required.\n");
		free( gui->sys );
		free( gui );
		return NULL;
	}
	self = gui;
	return gui;
}

void *Close(cad_kernel*, cad_GUI *self)
{
	DestroyCLR( self );
	free( self->sys );
	free( self );
	return NULL;
}

void gui_SetCMDArgs(cad_GUI *self, char *arg)
{
}

uint32_t gui_Exec(cad_GUI *self)
{
	DWORD dwRetCode;
 	HRESULT  hr = self->sys->pClrRuntimeHost->ExecuteInDefaultAppDomain(L"plugins\\WPF_GUI.dll", 
        L"WPF_GUI.StaticLoader", L"Exec", L"dd", &dwRetCode);
    if (FAILED(hr)) {
        self->sys->kernel->PrintDebug("Failed to call StaticLoader::Exec\n" );
		return 0;
     }

	return dwRetCode;
}

void gui_UpdatePictureEvent( cad_GUI *self )
{
//	cad_picture *p = self->sys->kernel->RenderPicture(self->sys->kernel, 0,0,1,1);
//	if ( p )
//	{
//		p->Delete( p );
//	}

	DWORD dwRetCode;
 	HRESULT  hr = self->sys->pClrRuntimeHost->ExecuteInDefaultAppDomain(L"plugins\\WPF_GUI.dll", 
        L"WPF_GUI.StaticLoader", L"UpdatePictureEvent", L"", &dwRetCode);
    if (FAILED(hr)) {
        self->sys->kernel->PrintDebug("Failed to call StaticLoader::UpdatePictureEvent\n" );
     }

}

bool InitCLR(cad_GUI *self)
{
	HRESULT hr;  

	self->sys->pMetaHost = NULL;
	self->sys->pRuntimeInfo = NULL;
	self->sys->pClrRuntimeHost = NULL;

	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_PPV_ARGS(&self->sys->pMetaHost));
    if (FAILED(hr))
    {
		self->sys->kernel->PrintDebug("CLRCreateInstance failed w/hr 0x%08lx\n", hr);
        goto Cleanup; 
	}

	hr = self->sys->pMetaHost->GetRuntime(L"v4.0.30319", IID_PPV_ARGS(&self->sys->pRuntimeInfo));
    if (FAILED(hr))
    {
        self->sys->kernel->PrintDebug("ICLRMetaHost::GetRuntime failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

	BOOL fLoadable;
    hr = self->sys->pRuntimeInfo->IsLoadable(&fLoadable);
    if (FAILED(hr))
    {
        self->sys->kernel->PrintDebug("ICLRRuntimeInfo::IsLoadable failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }
	
	if (!fLoadable)
    {
        self->sys->kernel->PrintDebug(".NET runtime %s cannot be loaded\n", "v4.0.30319");
        goto Cleanup;
    }

	 hr = self->sys->pRuntimeInfo->GetInterface(CLSID_CLRRuntimeHost, 
        IID_PPV_ARGS(&self->sys->pClrRuntimeHost));
    if (FAILED(hr))
    {
        self->sys->kernel->PrintDebug("ICLRRuntimeInfo::GetInterface failed w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

	hr = self->sys->pClrRuntimeHost->Start();
    if (FAILED(hr))
    {
        self->sys->kernel->PrintDebug("CLR failed to start w/hr 0x%08lx\n", hr);
        goto Cleanup;
    }

	return true;

Cleanup: DestroyCLR( self );
    return false;
}


bool DestroyCLR(cad_GUI *self)
{
	if (self->sys->pMetaHost)
    {
        self->sys->pMetaHost->Release();
        self->sys->pMetaHost = NULL;
    }
    if (self->sys->pRuntimeInfo)
    {
        self->sys->pRuntimeInfo->Release();
        self->sys->pRuntimeInfo = NULL;
    }
	return true;
}

// .NET assembly interface 

extern "C" __declspec( dllexport ) uint32_t  GetCurrentState();
extern "C" __declspec( dllexport ) int GetModuleList(int bufferSize, char *buffer);
extern "C" __declspec( dllexport ) bool LoadFile( const char *path );
extern "C" __declspec( dllexport ) bool StartPlaceMoule( char *name, bool inDemoMode );
extern "C" __declspec( dllexport ) bool StartTraceModule(char *name, bool inDemoMode );
extern "C" __declspec( dllexport ) uint32_t RunToEnd();
extern "C" __declspec( dllexport ) uint32_t NextStep( bool inDemoMode );

extern "C" __declspec( dllexport ) bool CloseCurrentFile();
extern "C" __declspec( dllexport ) cad_picture * RenderPicture(float pos_x, float pos_y, float size_x, float size_y );
extern "C" __declspec( dllexport ) void FreePicture( cad_picture *p );

uint32_t  GetCurrentState()
{
	return self->sys->kernel->GetCurrentState( self->sys->kernel );
}

int GetModuleList(int bufferSize, char *buffer)
{
	cad_module_info *mod;
	uint32_t count = self->sys->kernel->GetModuleList( self->sys->kernel, &mod);
	char *pos = buffer;

	for (uint32_t i = 0; i < count; i++)
	{
//		if (mod[i].module_capability != CAP_PLACEMENT || 
//			mod[i].module_capability != CAP_TRACEROUTE) 	continue;

		int l = strlen( mod[i].module_name ) + 2;
		if (pos + l >= buffer + bufferSize) return -1;
		sprintf(pos, "%c%s\n", mod[i].module_capability == CAP_PLACEMENT ? 'P' : 'T', mod[i].module_name);
		pos += l;
	}
	return pos - buffer;
}

bool LoadFile( const char *path )
{
	return self->sys->kernel->LoadFile( self->sys->kernel, path );
}

bool StartPlaceMoule( char *name, bool inDemoMode )
{
	return self->sys->kernel->StartPlaceMoule( self->sys->kernel, name, inDemoMode );
}

bool StartTraceModule( char *name, bool inDemoMode )
{
	return self->sys->kernel->StartTraceModule( self->sys->kernel, name, inDemoMode );
}

uint32_t RunToEnd()
{
	return self->sys->kernel->RunToEnd( self->sys->kernel );
}

uint32_t NextStep( bool inDemoMode )
{
	return self->sys->kernel->NextStep( self->sys->kernel, inDemoMode);
}

bool CloseCurrentFile()
{
	return self->sys->kernel->CloseCurrentFile( self->sys->kernel );
}

cad_picture * RenderPicture(float pos_x, float pos_y, float size_x, float size_y )
{
	// temporary stuff for testing
	cad_picture* p = (cad_picture *)malloc( sizeof(cad_picture));
	p->height = 100;
	p->width = 200;
	p->sys = NULL;
	p->data = (uint32_t *)malloc(sizeof(uint32_t) * p->height * p->width);
	p->data[0] = 0xFFAFFFFF;
	p->Delete = NULL;
	return p;

	// normal code. use it!
//	return self->sys->kernel->RenderPicture(self->sys->kernel, pos_x, pos_y, size_x, size_y);
}

void FreePicture( cad_picture *p )
{
	if (p->Delete)
		p->Delete( p );
}