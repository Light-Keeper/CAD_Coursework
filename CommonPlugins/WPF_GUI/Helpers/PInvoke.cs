using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace WPF_GUI.Helpers
{
    public static class PInvoke
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
                                                      string lpszClassName,
                                                      string lpszWindowName,
                                                      int style,
                                                      int x, int y,
                                                      int width, int height,
                                                      IntPtr hwndParent,
                                                      IntPtr hMenu,
                                                      IntPtr hInst,
                                                      [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint="SetClassLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetClassLong", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint SetClassLongPtr32(IntPtr hWnd, int nIndex, uint dwNewLong);

        public static IntPtr SetClassLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntPtr.Size > 4 ?
                SetClassLongPtr64(hWnd, nIndex, dwNewLong) :
                new IntPtr(SetClassLongPtr32(hWnd, nIndex, unchecked((uint)dwNewLong.ToInt32())));
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetCursor(IntPtr hCursor);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr LoadCursorFromFile(string lpFileName);

        // ----------------------------------------

        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            
            public RECT(RECT Rectangle)
                : this(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom)
            {
            }

            public RECT(int Left, int Top, int Right, int Bottom)
            {
                left = Left;
                top = Top;
                right = Right;
                bottom = Bottom;
            }
            
            public int X
            {
                get { return left; }
                set
                {
                    right -= (left - value);
                    left = value;
                }
            }

            public int Y
            {
                get { return top; }
                set
                {
                    bottom -= (top - value);
                    top = value;
                }
            }
            public int Height
            {
                get { return bottom - top; }
                set { bottom = value + top; }
            }
            public int Width
            {
                get { return right - left; }
                set { right = value + left; }
            }
            
            public int Left
            {
                get { return left; }
                set { left = value; }
            }

            public int Top
            {
                get { return top; }
                set { top = value; }
            }

            public int Right
            {
                get { return right; }
                set { right = value; }
            }

            public int Bottom
            {
                get { return bottom; }
                set { bottom = value; }
            }
            
            public Point Location
            {
                get
                {
                    return new Point(Left, Top);
                }
                set
                {
                    right -= (left - value.X);
                    bottom -= (top - value.Y);
                    left = value.X;
                    top = value.Y;
                }
            }

            public Size Size
            {
                get
                {
                    return new Size(Width, Height);
                }
                set
                {
                    right = value.Width + left;
                    bottom = value.Height + top;
                }
            }
            
            public static implicit operator Rectangle(RECT Rectangle)
            {
                return new Rectangle(Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height);
            }

            public static implicit operator RECT(Rectangle Rectangle)
            {
                return new RECT(Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom);
            }
            
            public static bool operator ==(RECT Rectangle1, RECT Rectangle2)
            {
                return Rectangle1.Equals(Rectangle2);
            }

            public static bool operator !=(RECT Rectangle1, RECT Rectangle2)
            {
                return !Rectangle1.Equals(Rectangle2);
            }
            
            public bool Equals(RECT Rectangle)
            {
                return Rectangle.Left == left && Rectangle.Top == top && Rectangle.Right == right && Rectangle.Bottom == bottom;
            }

            public override bool Equals(object Object)
            {
                if (Object is RECT)
                {
                    return Equals((RECT)Object);
                }
                else if (Object is Rectangle)
                {
                    return Equals(new RECT((Rectangle)Object));
                }

                return false;
            }
            
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override string ToString()
            {
                return "{Left: " + left + "; " + "Top: " + top + "; Right: " + right + "; Bottom: " + bottom + "}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }

        // -----------------------------------------

        internal const int WS_CHILD = 0x40000000;

        internal const int WS_VISIBLE = 0x10000000;

        internal const int HOST_ID = 0x00000002;

        internal const int WM_PAINT = 0x000F;

        internal const int WM_MOUSEMOVE = 0x0200;

        internal const int WM_LBUTTONDOWN = 0x0201;

        internal const int WM_LBUTTONUP = 0x0202;

        internal const int WM_MOUSEHWHEEL = 0x020E;

        internal const int WM_NCHITTEST = 0x0084;

        internal const int HTCLIENT = 1;        

        internal const int IMAGE_CURSOR = 2;

        internal const int IMAGE_ICON = 1;

        internal const int LR_LOADFROMFILE = 0x00000010;

        internal const int GCLP_HCURSOR = -12;

        internal const int MK_CONTROL   = 0x0008;
        internal const int MK_LBUTTON   = 0x0001;
        internal const int MK_MBUTTON   = 0x0010;
        internal const int MK_RBUTTON   = 0x0002;
        internal const int MK_SHIFT     = 0x0004;
        internal const int MK_XBUTTON1  = 0x0020;
        internal const int MK_XBUTTON2  = 0x0040;

        // ------------------------------------------

        internal static UInt16 LOWORD(IntPtr dwValue)
        {
            return (UInt16) (((UInt32)dwValue) & 0x0000FFFF);
        }

        internal static UInt16 HIWORD(IntPtr dwValue)
        {
            return (UInt16) ((((UInt32)dwValue) & 0xFFFF0000) >> 0x10);
        }

        internal static short GET_WHEEL_DELTA_WPARAM(IntPtr wParam)
        {
            return (short)HIWORD(wParam);
        }

        internal static short GET_KEYSTATE_WPARAM(IntPtr wParam)
        {
            return (short)LOWORD(wParam);
        }
    }
}
