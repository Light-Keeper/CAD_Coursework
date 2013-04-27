using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace WPF_GUI.Helpers
{
    public class ImageHost : HwndHost
    {
        private IntPtr _hwndHost;
        
        private readonly int _hostWidth;
        private readonly int _hostHeight;

        private static IntPtr _cursorGrab;
        private static IntPtr _cursorGrabbing;

        private bool _isDragging;
        public bool IsDragging { get { return _isDragging; } }

        public double FirstVisibleX = 0;
        public double FirstVisibleY = 0;

        public double Scale = 1.0;

        public ImageHost() : this(0, 0)
        {
        }

        public ImageHost(int width, int height)
        {
            _hostWidth = width;
            _hostHeight = height;

            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream("WPF_GUI.Recources.Cursors.grab.cur");
            FlushCursorStreamToDisk(stream, "grab.cur");
            _cursorGrab = PInvoke.LoadCursorFromFile("grab.cur");
            File.Delete("grab.cur");

            stream = assembly.GetManifestResourceStream("WPF_GUI.Recources.Cursors.grabbing.cur");
            FlushCursorStreamToDisk(stream, "grabbing.cur");
            _cursorGrabbing = PInvoke.LoadCursorFromFile("grabbing.cur");
            File.Delete("grabbing.cur");
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            _hwndHost = IntPtr.Zero;

            _hwndHost = PInvoke.CreateWindowEx(0, "static", "",
                                PInvoke.WS_CHILD | PInvoke.WS_VISIBLE,
                                0, 0,
                                _hostWidth, _hostHeight, 
                                hwndParent.Handle,
                                (IntPtr)PInvoke.HOST_ID,
                                IntPtr.Zero,
                                0);

            PInvoke.SetClassLong(_hwndHost, PInvoke.GCLP_HCURSOR, _cursorGrab);

            return new HandleRef(this, _hwndHost);
        }
        
        public void Render()
        {
            this.Render(true);
        }

        public void Render(bool newImage)
        {
            Kernel.RenderPicture(this.Handle, this.FirstVisibleX, this.FirstVisibleY, this.Scale, newImage, false, 0);
        }

        protected override IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            switch (msg)
            {
                case PInvoke.WM_NCHITTEST: // This code activate mouse events
                    handled = true;
                    return new IntPtr(PInvoke.HTCLIENT);

                case PInvoke.WM_PAINT:
                    PInvoke.PAINTSTRUCT pStruct;

                    PInvoke.BeginPaint(hWnd, out pStruct);

                    this.Render();

                    PInvoke.EndPaint(hWnd, ref pStruct);

                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_MOUSEMOVE:
                    if (_isDragging)
                    {
                        PInvoke.SetCursor(_cursorGrabbing);
                    }
                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONDOWN:
                    PInvoke.SetCursor(_cursorGrabbing);
                    _isDragging = true;
                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONUP:
                    _isDragging = false;
                    handled = true;
                    return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            PInvoke.DestroyWindow(hwnd.Handle);
        }

        private static void FlushCursorStreamToDisk(Stream stream, string name)
        {
            if (stream == null) return;

            var fileStream = new FileStream(name, FileMode.CreateNew, FileAccess.Write, FileShare.None);

            var binaryReader = new BinaryReader(stream);

            fileStream.Write(binaryReader.ReadBytes((int)stream.Length), 0, (int)stream.Length);

            binaryReader.Close();
            binaryReader.Dispose();

            stream.Close();
            stream.Dispose();

            fileStream.Flush(true);
            fileStream.Close();
            fileStream.Dispose();

            File.SetAttributes(name, FileAttributes.Temporary | FileAttributes.Hidden);
        }
    }
}
