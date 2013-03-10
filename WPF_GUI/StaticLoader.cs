using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace WPF_GUI
{
    public unsafe struct Picture
    {
        public IntPtr UnmanagedStruct;
        public int width;
        public int height;
        public UIntPtr data;
    };

    public class StaticLoader
    {
        private static MainWindow window;

        // call forom native code
        public static int Exec(string arg)
        {
            // если убрать этот месседж бокс, то и окно не появится. наверно он что-то там инициализирует, надо разобраться.
            MessageBox.Show("hello, " + arg + "!");
            (new System.Windows.Application()).Run( window = new MainWindow() );      
            return 0;
        }

        // call forom native code
        public static int UpdatePictureEvent(string arg)
        {
            window.UpdatePictureEvent();
            return 0;
        }

        [DllImport("GUI_CLR_loader.dll")]
        private static extern int GetModuleList(int bufferSize, StringBuilder b);

        public static List<string> GetModuleList()
        {
            int BufferSize = 10000;
            var str = new StringBuilder(BufferSize);
            int res = GetModuleList(BufferSize, str);
            if (res < 0) return new List<string>();
            return str.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList() ;
        }

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartPlaceMoule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern IntPtr RenderPicture(float pos_x, float pos_y, float size_x, float size_y);

        public static Picture GetPicture(float pos_x, float pos_y, float size_x, float size_y)
        { 
            Picture p = new Picture();

            IntPtr data = RenderPicture(pos_x, pos_y, size_x, size_y);
            unsafe 
            {
                p.UnmanagedStruct = data;
                p.height = *((int*)(data.ToPointer()) + 1);
                p.width = *((int*)(data.ToPointer()) + 2);
                p.data = (UIntPtr)((int*)(data.ToPointer()) + 3);
            }

            return p;
        }

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void FreePicture(IntPtr picture);

        public static void FreePicture(Picture picture)
        {
            FreePicture(picture.UnmanagedStruct);
        }


    }



}
