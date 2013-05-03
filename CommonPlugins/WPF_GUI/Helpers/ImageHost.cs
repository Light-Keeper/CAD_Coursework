using System;
using System.IO;
using System.Reflection;
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

        private Point _dragFistVisiblePos;

        public Size RealSize;

        private Point _firstVisiblePos = new Point(0, 0);

        public double Scale = 1.0;

        public ImageHost()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var stream = assembly.GetManifestResourceStream("WPF_GUI.Recources.Cursors.grab.cur");
            FlushCursorStreamToDisk(stream, "grab.cur");
            _cursorGrab = LoadCursorFromFile("grab.cur");
            File.Delete("grab.cur");

            stream = assembly.GetManifestResourceStream("WPF_GUI.Recources.Cursors.grabbing.cur");
            FlushCursorStreamToDisk(stream, "grabbing.cur");
            _cursorGrabbing = LoadCursorFromFile("grabbing.cur");
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
                    if ( _isDragging )
                    {
                        PInvoke.SetCursor(_cursorGrabbing);

                        var offsetX = (int)((_dragStartPos.X - PInvoke.LOWORD(lParam)) / this.Scale);
                        var offsetY = (int)((_dragStartPos.Y - PInvoke.HIWORD(lParam)) / this.Scale);

                        if (_dragFistVisiblePos.X + offsetX < 0)
                        {
                            _firstVisiblePos.X = 0;
                        }
                        else if (_dragFistVisiblePos.X + offsetX + (this.Width / this.Scale) > RealSize.Width)
                        {
                            _firstVisiblePos.X = (int)(RealSize.Width - (this.Width / this.Scale));
                        }
                        else
                        {
                            _firstVisiblePos.X = _dragFistVisiblePos.X + offsetX;
                        }

                        if (_dragFistVisiblePos.Y + offsetY < 0)
                        {
                            _firstVisiblePos.Y = 0;
                        }
                        else if (_dragFistVisiblePos.Y + offsetY + (this.Height / this.Scale) > RealSize.Height)
                        {
                            _firstVisiblePos.Y = (int)(RealSize.Height - (this.Height / this.Scale));
                        }
                        else
                        {
                            _firstVisiblePos.Y = _dragFistVisiblePos.Y + offsetY;
                        }

                        handled = true;

                        this.Render();
                    }

                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONDOWN:
                    PInvoke.SetCursor(_cursorGrabbing);

                    _dragStartPos.X = PInvoke.LOWORD(lParam);
                    _dragStartPos.Y = PInvoke.HIWORD(lParam);

                    _dragFistVisiblePos = _firstVisiblePos;

                    StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg,
                        "StartX: " + (_firstVisiblePos.X + (_dragStartPos.X / this.Scale))
                        + " StartY: " + (_firstVisiblePos.Y + (_dragStartPos.Y / this.Scale))
                        + "  RealWidth: " + RealSize.Width + " RealHeight: " + RealSize.Height);

                    _isDragging = true;

                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONUP:
                    _isDragging = false;
                    handled = true;
                    return IntPtr.Zero;

//                case PInvoke.WM_MOUSEHWHEEL:
//
//                    var delta = PInvoke.GET_WHEEL_DELTA_WPARAM(wParam);
//                    var keyState = PInvoke.GET_KEYSTATE_WPARAM(wParam);
//
////                    MessageBox.Show("Delta: " + delta.ToString("X") + "\nkeyState: " + keyState.ToString("X"));
//
//                    // Change zoom
//                    if ((keyState & PInvoke.MK_CONTROL) == PInvoke.MK_CONTROL &&
//                        (keyState & PInvoke.MK_SHIFT) != PInvoke.MK_SHIFT)
//                    {
//                        if (delta < 0)
//                        {
//                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ChangeZoom, -5);
//                        }
//                        else
//                        {
//                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ChangeZoom, 5);
//                        }
//                    }
//                    // Change width scroll
//                    else if ((keyState & PInvoke.MK_CONTROL) != PInvoke.MK_CONTROL &&
//                             (keyState & PInvoke.MK_SHIFT) == PInvoke.MK_SHIFT)
//                    {
//                        if (delta < 0)
//                        {
//                            
//                        }
//                        else
//                        {
//                            
//                        }
//                    }
//                    // Change height scroll
//                    else if ((keyState & PInvoke.MK_CONTROL) != PInvoke.MK_CONTROL &&
//                             (keyState & PInvoke.MK_SHIFT) != PInvoke.MK_SHIFT)
//                    {
//                        if (delta < 0)
//                        {
//
//                        }
//                        else
//                        {
//
//                        }
//                    }
//
//
//                    handled = true;
//                    return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            PInvoke.DestroyWindow(hwnd.Handle);
        }

        private static IntPtr LoadCursorFromFile(string lpFileName)
        {
            const int cursorSize = 16;

            return PInvoke.LoadImage(
                IntPtr.Zero,
                lpFileName,
                PInvoke.IMAGE_CURSOR,
                cursorSize, // X size
                cursorSize, // Y size
                PInvoke.LR_LOADFROMFILE);
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
