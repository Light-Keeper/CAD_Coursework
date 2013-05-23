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

        private bool _isDragging;
        private Point _dragStartPos;
        private Point _dragFirstVisiblePos;

        public double RealWidth { get; set; }
        public double RealHeight { get; set; }

        public new double MaxWidth { get; set; }
        public new double MaxHeight { get; set; }

        public double RealVisibleWidth
        {
            get { return this.Width / this.Scale; }
        }

        public double RealVisibleHeight
        {
            get { return this.Height / this.Scale; }
        }

        public Point FirstVisiblePos = new Point(0, 0);

        private double _scale = 1.0;
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (_scale == value) return;
                _scale = value;

                if (this.RealVisibleWidth > this.RealWidth)
                {
                    this.Width -= (this.RealVisibleWidth - this.RealWidth) * _scale;
                }
                
                if (this.RealVisibleHeight > this.RealHeight)
                {
                    this.Height -= (this.RealVisibleHeight - this.RealHeight) * _scale;
                }

                if (this.RealVisibleWidth < this.RealWidth && this.Width < this.MaxWidth)
                {
                    var newWidth = this.RealWidth * _scale;
                    this.Width = newWidth > this.MaxWidth ? this.MaxWidth : newWidth;
                }

                if (this.RealVisibleHeight < this.RealHeight && this.Height < this.MaxHeight)
                {
                    var newHeight = this.RealHeight * _scale;
                    this.Height = newHeight > this.MaxHeight ? this.MaxHeight : newHeight;
                }

                this.CheckBorders();
            }
        }

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

        public void CheckBorders()
        {
            if (FirstVisiblePos.X + RealVisibleWidth > RealWidth)
            {
                FirstVisiblePos.X = 0;
            }
            if (FirstVisiblePos.Y + RealVisibleHeight > RealHeight)
            {
                FirstVisiblePos.Y = 0;
            }
            this.Render();
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
                        _isDragging = false;
                    }
                    if ( _isDragging )
                    {
                        PInvoke.SetCursor(_cursorGrabbing);

                        var offsetX = (int)((_dragStartPos.X - PInvoke.LOWORD(lParam)) / _scale);
                        var offsetY = (int)((_dragStartPos.Y - PInvoke.HIWORD(lParam)) / _scale);

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

                        this.Render();

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);

                        handled = true;
                    }

                    return IntPtr.Zero;

                case PInvoke.WM_LBUTTONDOWN:
                    PInvoke.SetCursor(_cursorGrabbing);

                    _dragStartPos.X = PInvoke.LOWORD(lParam);
                    _dragStartPos.Y = PInvoke.HIWORD(lParam);

                    _dragFirstVisiblePos = FirstVisiblePos;

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
