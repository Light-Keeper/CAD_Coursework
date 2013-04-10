﻿using System;
using System.Collections.Generic;
﻿using System.Drawing;
﻿using System.Drawing.Imaging;
﻿using System.IO;
﻿using System.Linq;
﻿using System.Text;
using System.Runtime.InteropServices;
﻿using System.Threading;
﻿using System.Windows;
﻿using System.Windows.Media.Imaging;
﻿using MediatorLib;
﻿using WPF_GUI.Helpers;

namespace WPF_GUI
{
    public struct Picture
    {
        public IntPtr UnmanagedStruct;
        public int Width;
        public int Height;
        public IntPtr Data;
    };

    public static class StaticLoader
    {
        public static App Application;
        public static Mediator Mediator;
        public static Picture Picture;

        // Call from native code
        public static int Exec(string msg)
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
            Picture = new Picture();
            Mediator = new Mediator();
            Application = new App();
            CoreMessage(msg);
            Application.Run();
            return 0;
        }

        // Call from native code
        public static int UpdatePictureEvent(string arg)
        {
            return 0;
        }

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool LoadFile(StringBuilder path);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern int GetModuleList(int bufferSize, StringBuilder charBuffer);

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
        private static extern IntPtr RenderPicture(Boolean forceDrawLayer, Int32 forceDrawLayerNumber);

        public static BitmapSource GetPicture(bool forceDrawLayer, int forceDrawLayerNumber)
        {
            var data = RenderPicture(forceDrawLayer, forceDrawLayerNumber);

            unsafe
            {
                Picture.UnmanagedStruct = data;
                Picture.Height = *((int*) (data.ToPointer()) + 1);
                Picture.Width = *((int*) (data.ToPointer()) + 2);
                Picture.Data = (IntPtr) (*((int*) (data.ToPointer()) + 3));
            }

            var imgLength = Picture.Height * Picture.Width * sizeof(Int32);
            var imgArray = new byte[imgLength];

            Marshal.Copy(Picture.Data, imgArray, 0, imgLength);

            var inStream = new MemoryStream();

            var bitmap = new Bitmap(Picture.Width, Picture.Height, PixelFormat.Format24bppRgb);

            for (int i = 0; i < Picture.Width; i++)
            {
                for (int j = 0; j < Picture.Height; j++)
                {
                    var color = Color.FromArgb(imgArray[i * j * 4],
                        imgArray[i * j * 4 + 1],
                        imgArray[i * j * 4 + 2],
                        imgArray[i * j * 4 + 3]);
                    bitmap.SetPixel(i, j, color);
                }
            }

            bitmap.Save(inStream, ImageFormat.Png);

            var image = new BitmapImage();

            image.BeginInit();
            image.StreamSource = inStream;
            image.EndInit();

            return image;
        }

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void FreePicture(IntPtr picture);

        public static void FreePicture(Picture picture)
        {
            FreePicture(picture.UnmanagedStruct);
        }

        public static int CoreMessage(string msg)
        {
            if (Mediator != null)
            {
                Mediator.NotifyColleagues(MediatorMessages.NewLog, msg);
            }

            return msg.Length;
        }
    }
}