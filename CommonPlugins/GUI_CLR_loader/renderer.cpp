#include <windows.h>
#include <cad_object.h>

#include <gl/gl.h>
#include <gl/glu.h>

HWND window;
HGLRC hRC;
HDC hDC;

uint32_t	__stdcall GetMapWidth();
uint32_t	__stdcall GetMapHeight();

extern "C" __declspec( dllexport ) uint32_t	__stdcall GetRenderWindowWidth();
extern "C" __declspec( dllexport ) uint32_t	__stdcall GetRenderWindowHeight();

void EnableOpenGL(HWND hWnd)
{
    PIXELFORMATDESCRIPTOR pfd;
    int iFormat;

    // get the device context (DC)
    hDC = GetDC( hWnd );

    // set the pixel format for the DC
    ZeroMemory( &pfd, sizeof( pfd ) );
    pfd.nSize = sizeof( pfd );
    pfd.nVersion = 1;
    pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL |
                  PFD_DOUBLEBUFFER;
    pfd.iPixelType = PFD_TYPE_RGBA;
    pfd.cColorBits = 24;
    pfd.cDepthBits = 16;
    pfd.iLayerType = PFD_MAIN_PLANE;
    iFormat = ChoosePixelFormat( hDC, &pfd );
    SetPixelFormat( hDC, iFormat, &pfd );

    // create and enable the render context (RC)
    hRC = wglCreateContext( hDC );
    wglMakeCurrent( hDC, hRC );
	window = hWnd;
}

void DisableOpenGL()
{
    wglMakeCurrent( NULL, NULL );
    wglDeleteContext( hRC );
    ReleaseDC( window, hDC );
}

int DrawGL(uint32_t x, uint32_t y, double scale, cad_picture *picture)
{
	RECT r;
	GetWindowRect(window, &r);
	glViewport(0,0, r.right - r.left, r.bottom - r.top);
	
	glLoadIdentity();
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	
	if (y >= picture->height || x >= picture->width) return 0;

	uint32_t *data = (uint32_t *)malloc(picture->width * picture->height * sizeof(uint32_t));
	uint32_t *p1 = data;
	uint32_t *p2 = picture->data + x + y * picture->width;

	for (uint32_t i = y; i < picture->height; i++)
	{
		memcpy(p1, p2, sizeof(uint32_t) *( picture->width - y));
		p1 += picture->width - y;
		p2 += picture->width;
	}

	glRasterPos2d(-1, 1);
	glPixelZoom((GLfloat)scale, -(GLfloat)scale);
	glDrawPixels(picture->width - y, picture->height - x, GL_BGRA_EXT, GL_UNSIGNED_BYTE, data);	
	free(data);
	SwapBuffers( hDC );
	return 1;
}

uint32_t	__stdcall GetRenderWindowWidth()
{
	RECT r;
	GetWindowRect(window, &r);
	return r.right - r.left;
}

uint32_t	__stdcall GetRenderWindowHeight()
{
	RECT r;
	GetWindowRect(window, &r);
	return r.bottom - r.top;
}