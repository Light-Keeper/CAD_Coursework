#include <cad_module.h>
#include <cad_object.h>

#include <windows.h>
#include <clocale>
#include <metahost.h>

#include <stdio.h>
#include <string.h>
#include <stdarg.h>

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

int DrawGL(uint32_t x, uint32_t y, double scale, cad_picture *picture);
void EnableOpenGL(HWND hWnd);

bool InitCLR(cad_GUI *self);
bool DestroyCLR(cad_GUI *self);
int my_printf(const char * format, ... );

static cad_GUI *self = NULL;

struct cad_GUI_private
{
	cad_kernel *kernel;
	cad_picture *current_picture;

	ICLRMetaHost *pMetaHost ;
	ICLRRuntimeInfo *pRuntimeInfo ;
	ICLRRuntimeHost *pClrRuntimeHost;
	HWND window;
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
	kernel->PrintDebug = my_printf;
	kernel->PrintInfo = my_printf; 
	self = gui;
	self->sys->window = 0;
	self->sys->current_picture = 0;
	return gui;
}

void DisableOpenGL();

void *Close(cad_kernel*, cad_GUI *self)
{
	DestroyCLR( self );
	DisableOpenGL();
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
        L"WPF_GUI.StaticLoader", L"Exec", L"Called Exec method in WPF_GUI", &dwRetCode);
    if (FAILED(hr)) {
        self->sys->kernel->PrintDebug("Failed to call StaticLoader::Exec\n" );
		return 0;
     }

	return dwRetCode;
}

void gui_UpdatePictureEvent( cad_GUI *self )
{
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

int my_printf(const char * format, ... )
{ 
	char str[2048];
	wchar_t wc_str[2048];
	DWORD dwRetCode;
	va_list args;
    va_start(args, format);
    int result = vsprintf(str,format, args);
    va_end(args);

	setlocale( LC_ALL,"Russian" );
	mbstowcs(wc_str, str, 2048);
	
	HRESULT hr = self->sys->pClrRuntimeHost->ExecuteInDefaultAppDomain(L"plugins\\WPF_GUI.dll", 
        L"WPF_GUI.StaticLoader", L"CoreMessage", wc_str, &dwRetCode);
    if (FAILED(hr)) {
		return 0;
     }
	return result;
}


// .NET assembly interface 

extern "C" __declspec( dllexport ) uint32_t	__stdcall GetKernelState();
extern "C" __declspec( dllexport ) int32_t	__stdcall GetModuleList( uint32_t bufferSize, char *buffer );
extern "C" __declspec( dllexport ) bool		__stdcall LoadFile( const char *path );
extern "C" __declspec( dllexport ) bool		__stdcall StartPlaceModule( char *name, bool inDemoMode );
extern "C" __declspec( dllexport ) bool		__stdcall StartTraceModule( char *name, bool inDemoMode );
extern "C" __declspec( dllexport ) uint32_t __stdcall RunToEnd();
extern "C" __declspec( dllexport ) uint32_t __stdcall NextStep( bool inDemoMode );

extern "C" __declspec( dllexport ) bool		__stdcall CloseCurrentFile();
extern "C" __declspec( dllexport ) void		__stdcall RenderPicture(HWND hWnd, uint32_t x, uint32_t y, double scale,  
											bool RenderNewPicture, bool renderLayer, uint32_t layer);

extern "C" __declspec( dllexport ) uint32_t	__stdcall GetMapWidth();
extern "C" __declspec( dllexport ) uint32_t	__stdcall GetMapHeight();
extern "C" __declspec( dllexport ) uint32_t	__stdcall GetRenderWindowWidth();
extern "C" __declspec( dllexport ) uint32_t	__stdcall GetRenderWindowHeight();


uint32_t __stdcall GetKernelState()
{
	return self->sys->kernel->GetCurrentState( self->sys->kernel );
}


int32_t __stdcall GetModuleList( uint32_t bufferSize, char *buffer )
{
	cad_module_info *mod;
	uint32_t count = self->sys->kernel->GetModuleList( self->sys->kernel, &mod);
	char *pos = buffer;

	for (uint32_t i = 0; i < count; i++)
	{
		if (mod[i].module_capability != CAP_PLACEMENT &&
			mod[i].module_capability != CAP_TRACEROUTE)
			continue;
		uint32_t l = strlen( mod[i].module_name ) + 2;
		if (pos + l >= buffer + bufferSize) return -1;
		sprintf(pos, "%c%s\n", mod[i].module_capability == CAP_PLACEMENT ? 'P' : 'T', mod[i].module_name);
		pos += l;
	}
	return pos - buffer;
}


bool __stdcall LoadFile( const char *path )
{
	return self->sys->kernel->LoadFile( self->sys->kernel, path );
}


bool __stdcall StartPlaceModule( char *name, bool inDemoMode )
{
	return self->sys->kernel->StartPlaceMoule( self->sys->kernel, name, inDemoMode );
}


bool __stdcall StartTraceModule( char *name, bool inDemoMode )
{
	return self->sys->kernel->StartTraceModule( self->sys->kernel, name, inDemoMode );
}


uint32_t __stdcall RunToEnd()
{
	return self->sys->kernel->RunToEnd( self->sys->kernel );
}


uint32_t __stdcall NextStep( bool inDemoMode )
{
	return self->sys->kernel->NextStep( self->sys->kernel, inDemoMode);
}


bool __stdcall CloseCurrentFile()
{
	return self->sys->kernel->CloseCurrentFile( self->sys->kernel );
}


void __stdcall RenderPicture(HWND hWnd, uint32_t x, uint32_t y, double scale,  
											bool RenderNewPicture, bool renderLayer, uint32_t layer)
{
	int xx = 0;
	xx = -GetTickCount();
	if (self->sys->window != hWnd)
	{
		EnableOpenGL( hWnd );
		self->sys->window = hWnd;
	}

	if (RenderNewPicture)
	{
		if (self->sys->current_picture) 
			self->sys->current_picture->Delete( self->sys->current_picture );

		self->sys->current_picture = self->sys->kernel->RenderPicture(self->sys->kernel, renderLayer, layer);
		//self->sys->kernel->PrintDebug("Render New Picture!!!");
	}

	DrawGL(x, y, scale, self->sys->current_picture);

	xx += GetTickCount();
	//self->sys->kernel->PrintDebug("RenderPicture time = %d ms, RenderNewPicture = %d", xx, RenderNewPicture); 
}


uint32_t __stdcall GetMapWidth()
{
	uint32_t x;
	self->sys->kernel->GetMapSize( self->sys->kernel, &x, NULL);
	return x;
}


uint32_t __stdcall GetMapHeight()
{
	uint32_t x;
	self->sys->kernel->GetMapSize( self->sys->kernel, NULL, &x);
	return x;
}