using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace WPF_GUI.Helpers
{
    public class ImageHost : HwndHost
    {
        private IntPtr _hwndHost;
        
        private readonly int _hostWidth;
        private readonly int _hostHeight;

        public ImageHost() : this(0, 0)
        {
        }

        public ImageHost(int width, int height)
        {
            _hostWidth = width;
            _hostHeight = height;
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

            return new HandleRef(this, _hwndHost);
        }
        
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            PInvoke.DestroyWindow(hwnd.Handle);
        }

        private void ChangeWindowSize(Size size)
        {
            
        }
    }
}
