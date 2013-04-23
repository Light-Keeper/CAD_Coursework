#include <windows.h>
#include <cad_object.h>

#include <gl/gl.h>
#include <gl/glu.h>

HWND window;
HGLRC hRC;
HDC hDC;

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
}

void DisableOpenGL()
{
    wglMakeCurrent( NULL, NULL );
    wglDeleteContext( hRC );
    ReleaseDC( window, hDC );
}


int DrawGL(double x1, double y1, double x2, double y2, cad_picture *picture)
{
	glDrawPixels(picture->width, picture->height, GL_RGBA, GL_UNSIGNED_BYTE, picture->data);	
	SwapBuffers( hDC );
	return 1;
}

