using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace WPF_GUI.Helpers
{
    public class ImageHost : HwndHost
    {
        private IntPtr _hwndHost;

        private static IntPtr _cursorGrab;
        private static IntPtr _cursorGrabbing;

        private bool _isDragging;
        public bool IsDragging { get { return _isDragging; } }

        private Point _dragStartPos;

        public Size RealSize;

        private Point _firstVisiblePos = new Point(0, 0);

        public double StartScale = 0.4;

        public double Scale;

        public ImageHost()
        {
            Scale = StartScale;

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
                                0, 0, // Width and height
                                hwndParent.Handle,
                                (IntPtr)PInvoke.HOST_ID,
                                IntPtr.Zero,
                                0);

            PInvoke.SetClassLong(_hwndHost, PInvoke.GCLP_HCURSOR, _cursorGrab);

            return new HandleRef(this, _hwndHost);
        }
        
        public void Render()
        {
            this.Render(false);
        }

        public void Render(bool redrawImage)
        {
            Kernel.RenderPicture(
                this.Handle,
                (uint)_firstVisiblePos.X,
                (uint)_firstVisiblePos.Y,
                this.Scale,
                redrawImage,
                false, 0);
        }

        protected override IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            switch (msg)
            {
                case PInvoke.WM_NCHITTEST: // Activate mouse events
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

                        var dragNewPos = new Point(PInvoke.LOWORD(lParam), PInvoke.HIWORD(lParam));

                        var offsetX = dragNewPos.X - _dragStartPos.X;
                        var offsetY = dragNewPos.Y - _dragStartPos.Y;

                        if (_firstVisiblePos.X - offsetX < 0)
                        {
                            _firstVisiblePos.X = 0;
                        }
//                        else if () // > MaxWidth
//                        {    
//                        }
                        else
                        {
                            _firstVisiblePos.X -= offsetX;
                        }

                        if (_firstVisiblePos.Y - offsetY < 0)
                        {
                            _firstVisiblePos.Y = 0;
                        }
//                        else if () // > MaxHeight
//                        {
//                        }
                        else
                        {
                            _firstVisiblePos.Y -= offsetY;
                        }
                        _dragStartPos.X = dragNewPos.X;
                        _dragStartPos.Y = dragNewPos.Y;

                        handled = true;

                        this.Render();
                    }

                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONDOWN:
                    PInvoke.SetCursor(_cursorGrabbing);

                    _dragStartPos.X = PInvoke.LOWORD(lParam);
                    _dragStartPos.Y = PInvoke.HIWORD(lParam);

                    _isDragging = true;

                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONUP:
                    _isDragging = false;
                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_MOUSEHWHEEL:

                    var delta = PInvoke.GET_WHEEL_DELTA_WPARAM(wParam);
                    var keyState = PInvoke.GET_KEYSTATE_WPARAM(wParam);

                    MessageBox.Show("Delta: " + delta.ToString("X") + "\nkeyState: " + keyState.ToString("X"));

                    // Change zoom
                    if ((keyState & PInvoke.MK_CONTROL) == PInvoke.MK_CONTROL &&
                        (keyState & PInvoke.MK_SHIFT) != PInvoke.MK_SHIFT)
                    {
                        if (delta < 0)
                        {
                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ChangeZoom, -5);
                        }
                        else
                        {
                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ChangeZoom, 5);
                        }
                    }
                    // Change width scroll
                    else if ((keyState & PInvoke.MK_CONTROL) != PInvoke.MK_CONTROL &&
                             (keyState & PInvoke.MK_SHIFT) == PInvoke.MK_SHIFT)
                    {
                        if (delta < 0)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                    // Change height scroll
                    else if ((keyState & PInvoke.MK_CONTROL) != PInvoke.MK_CONTROL &&
                             (keyState & PInvoke.MK_SHIFT) != PInvoke.MK_SHIFT)
                    {
                        if (delta < 0)
                        {

                        }
                        else
                        {

                        }
                    }


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
