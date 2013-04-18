#include <cad_object.h>

// CAD entry point 
int __stdcall WinMain(
  uint32_t hInstance,
  uint32_t hPrevInstance,
  uint32_t lpCmdLine,
  uint32_t nCmdShow
)
{
	cad_kernel *kernel = cad_kernel_New();
	kernel->Exec( kernel );
	kernel->Delete( kernel );
	_CrtDumpMemoryLeaks();
	return 0;

}