#include <cad_object.h>

// CAD entry point 
int main(int argc, char * argv[])
{
	cad_kernel *kernel = cad_kernel_New(argc, argv);
	kernel->Exec( kernel );
	kernel->Delete( kernel );
	return 0;
}