﻿using System;
using System.Collections.Generic;
﻿using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MediatorLib;
﻿using MessageBox = System.Windows.Forms.MessageBox;

namespace WPF_GUI
{
    public unsafe struct Picture
    {
        public IntPtr UnmanagedStruct;
        public int Width;
        public int Height;
        public UIntPtr Data;
    };

    public class StaticLoader
    {
        public static App Application;
        public static Mediator Mediator;

        // call forom native code
        public static int Exec(string arg)
        {
            // если убрать этот месседж бокс, то и окно не появится
            // наверно он что-то там инициализирует, надо разобраться.
//            MessageBox.Show("hello, " + arg + "!");
            Application = new App();
            Application.Run();

            Mediator = new Mediator();
            return 0;
        }

        // call forom native code
        public static int UpdatePictureEvent(string arg)
        {
//            MainWindow.UpdatePictureEvent();
            return 0;
        }

        [DllImport("GUI_CLR_loader.dll")]
        private static extern int GetModuleList(int bufferSize, StringBuilder b);

        public static List<string> GetModuleList()
        {
            const int bufferSize = 10000;
            var str = new StringBuilder(bufferSize);
            var res = GetModuleList(bufferSize, str);
            return res < 0 ?
                new List<string>() :
                str.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartPlaceMoule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern IntPtr RenderPicture(float posX, float posY, float sizeX, float sizeY);

        public static Picture GetPicture(float posX, float posY, float sizeX, float sizeY)
        { 
            var p = new Picture();

            var data = RenderPicture(posX, posY, sizeX, sizeY);
            unsafe 
            {
                p.UnmanagedStruct = data;
                p.Height = *((int*)(data.ToPointer()) + 1);
                p.Width = *((int*)(data.ToPointer()) + 2);
                p.Data = (UIntPtr)((int*)(data.ToPointer()) + 3);
            }

            return p;
        }

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void FreePicture(IntPtr picture);

        public static void FreePicture(Picture picture)
        {
            FreePicture(picture.UnmanagedStruct);
        }

        public static int CoreMessage(string msg)
        {
            MessageBox.Show(msg);
            return 0;
        }
    }
}