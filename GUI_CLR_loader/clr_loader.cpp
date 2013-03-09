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
	cad_picture *p = self->sys->kernel->RenderPicture(self->sys->kernel, 0,0,1,1);
	if ( p )
	{
		p->Delete( p );
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