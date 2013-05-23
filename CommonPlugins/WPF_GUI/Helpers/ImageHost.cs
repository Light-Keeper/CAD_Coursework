using System;
using System.IO;
using System.Reflection;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Point = System.Drawing.Point;

namespace WPF_GUI.Helpers
{
    public class ImageHost : HwndHost
    {
        private IntPtr _hwndHost;

        private static IntPtr _cursorGrab;
        private static IntPtr _cursorGrabbing;

        public bool IsDragging { get; private set; }

        private Point _dragStartPos;
        private Point _dragFirstVisiblePos;

        public int RealWidth;
        public int RealHeight;

        public double RealVisibleWidth
        {
            get { return this.Width / this.Scale; }
        }

        public double RealVisibleHeight
        {
            get { return this.Height / this.Scale; }
        }

        public Point FirstVisiblePos = new Point(0, 0);

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
                (uint)FirstVisiblePos.X,
                (uint)FirstVisiblePos.Y,
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
                    if ((PInvoke.GetKeyState(PInvoke.VK_LBUTTON) & 0x80) == 0)
                    {
                        IsDragging = false;
                    }
                    if ( IsDragging )
                    {
                        PInvoke.SetCursor(_cursorGrabbing);

                        var offsetX = (int)((_dragStartPos.X - PInvoke.LOWORD(lParam)) / this.Scale);
                        var offsetY = (int)((_dragStartPos.Y - PInvoke.HIWORD(lParam)) / this.Scale);

                        if (_dragFirstVisiblePos.X + offsetX < 0)
                        {
                            FirstVisiblePos.X = 0;
                        }
                        else if (_dragFirstVisiblePos.X + offsetX + RealVisibleWidth > RealWidth)
                        {
                            FirstVisiblePos.X = (int)(RealWidth - RealVisibleWidth);
                        }
                        else
                        {
                            FirstVisiblePos.X = _dragFirstVisiblePos.X + offsetX;
                        }

                        if (_dragFirstVisiblePos.Y + offsetY < 0)
                        {
                            FirstVisiblePos.Y = 0;
                        }
                        else if (_dragFirstVisiblePos.Y + offsetY + RealVisibleHeight > RealHeight)
                        {
                            FirstVisiblePos.Y = (int)(RealHeight - RealVisibleHeight);
                        }
                        else
                        {
                            FirstVisiblePos.Y = _dragFirstVisiblePos.Y + offsetY;
                        }

                        handled = true;

                        this.Render();

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);
                    }

                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONDOWN:
                    PInvoke.SetCursor(_cursorGrabbing);

                    _dragStartPos.X = PInvoke.LOWORD(lParam);
                    _dragStartPos.Y = PInvoke.HIWORD(lParam);

                    _dragFirstVisiblePos = FirstVisiblePos;

                    IsDragging = true;

                    handled = true;
                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONUP:
                    IsDragging = false;
                    handled = true;
                    return IntPtr.Zero;
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
