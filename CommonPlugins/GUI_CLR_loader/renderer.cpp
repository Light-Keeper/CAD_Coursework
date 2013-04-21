#include <windows.h>
#include <cad_object.h>

#include <gl/gl.h>
#include <gl/glu.h>

HWND window;
HGLRC hRC;
HDC hDC;

void EnableOpenGL(HWND hWnd, HDC * hDC, HGLRC * hRC);

int InitWindow()
{
	WNDCLASSEX wc;

	//Step 1: Registering the Window Class
    wc.cbSize        = sizeof(WNDCLASSEX);
    wc.style         = 0;
	wc.lpfnWndProc   = DefWindowProc;
    wc.cbClsExtra    = 0;
    wc.cbWndExtra    = 0;
    wc.hInstance     = 0;
    wc.hIcon         = LoadIcon(NULL, IDI_APPLICATION);
    wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
    wc.lpszMenuName  = NULL;
    wc.lpszClassName = "asdfasdf";
    wc.hIconSm       = LoadIcon(NULL, IDI_APPLICATION);

    if(!RegisterClassEx(&wc))
    {
        MessageBox(NULL, "Window Registration Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
        return 0;
    }

    // Step 2: Creating the Window
    window = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        "asdfasdf",
        "The title of my window",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 240, 120,
        NULL, NULL, 0, NULL);

    if(window == NULL)
    {
        MessageBox(NULL, "Window Creation Failed!", "Error!",
            MB_ICONEXCLAMATION | MB_OK);
        return 0;
    }

	SetWindowPos(window,0,0,0, 1000,600, SWP_NOMOVE|SWP_NOZORDER|SWP_NOACTIVATE);
	ShowWindow(window, SW_SHOW);
    UpdateWindow(window);

	EnableOpenGL( window, &hDC, &hRC );


	return 1;
}


void EnableOpenGL(HWND hWnd, HDC * hDC, HGLRC * hRC)
{
    PIXELFORMATDESCRIPTOR pfd;
    int iFormat;

    // get the device context (DC)
    *hDC = GetDC( hWnd );

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
    iFormat = ChoosePixelFormat( *hDC, &pfd );
    SetPixelFormat( *hDC, iFormat, &pfd );

    // create and enable the render context (RC)
    *hRC = wglCreateContext( *hDC );
    wglMakeCurrent( *hDC, *hRC );
}

void DisableOpenGL()
{
    wglMakeCurrent( NULL, NULL );
    wglDeleteContext( hRC );
    ReleaseDC( window, hDC );
}


int DrawInWindow(int h, int w, double x1, double y1, double x2, double y2, cad_picture *picture)
{
	glDrawPixels(picture->width, picture->height, GL_RGBA, GL_UNSIGNED_BYTE, picture->data);	
	SwapBuffers( hDC );
	return 1;
}

