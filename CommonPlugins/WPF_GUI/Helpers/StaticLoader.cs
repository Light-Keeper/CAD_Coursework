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
﻿using Point = System.Drawing.Point;

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
        #region CLR Loader Methods

        [DllImport("GUI_CLR_loader.dll")]
        public static extern UInt32 GetKernelState();

        [DllImport("GUI_CLR_loader.dll")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool LoadFile(StringBuilder path);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartPlaceModule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern bool StartTraceModule(string moduleName, bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        public static extern UInt32 NextStep(bool inDemoMode);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern int GetModuleList(int bufferSize, StringBuilder charBuffer);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern IntPtr RenderPicture(Boolean forceDrawLayer, Int32 forceDrawLayerNumber);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void FreePicture(IntPtr picture);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void SetPictureSize(UInt32 width, UInt32 height);

        #endregion

        public static App Application;
        public static Mediator Mediator;

        // Call from native code
        public static int Exec(string msg)
        {
            try
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                Mediator = new Mediator();
                Application = new App();
                CoreMessage(msg);
                SetPictureSize(// It's only recommendation
                    Convert.ToUInt32(SystemParameters.WorkArea.Width),
                    Convert.ToUInt32(SystemParameters.WorkArea.Height));
                Application.Run();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
            return 0;
        }

        // Call from native code
        public static int UpdatePictureEvent(string arg)
        {
            GetPicture(false, 0);
            return 0;
        }

        // Call from native code
        public static int CoreMessage(string msg)
        {
            if (Mediator != null)
            {
                Mediator.NotifyColleagues(MediatorMessages.NewLog, msg);
            }
            return msg.Length;
        }

        public static List<string> GetModuleList()
        {
            const int bufferSize = 10000;
            var str = new StringBuilder(bufferSize);
            var res = GetModuleList(bufferSize, str);
            return res < 0 ?
                new List<string>() :
                str.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static void GetPicture(bool forceDrawLayer, int forceDrawLayerNumber)
        {
//            var data = RenderPicture(forceDrawLayer, forceDrawLayerNumber);
//
//            var picture = new Picture();
//
//            unsafe
//            {
//                picture.UnmanagedStruct = data;
//                picture.Height = *((int*) (data.ToPointer()) + 1);
//                picture.Width = *((int*) (data.ToPointer()) + 2);
//                picture.Data = (IntPtr) (*((int*) (data.ToPointer()) + 3));
//            }
//
//            var imgLength = (uint) (picture.Height * picture.Width * sizeof(UInt32));
//
//            var bitmap = new Bitmap(picture.Width, picture.Height, PixelFormat.Format32bppRgb);
//
//            var dst = bitmap.LockBits(
//                new Rectangle(Point.Empty, bitmap.Size),
//                ImageLockMode.WriteOnly,
//                PixelFormat.Format32bppRgb);
//            
//            PInvoke.CopyMemory(dst.Scan0, picture.Data, imgLength);
//
//            bitmap.UnlockBits(dst);
//            
//            FreePicture(picture.UnmanagedStruct);
//
//            var inStream = new MemoryStream();
//
//            bitmap.Save(inStream, ImageFormat.Bmp);
//            bitmap.Dispose();
//
//            var image = new BitmapImage();
//
//            image.BeginInit();
//            image.StreamSource = inStream;
//            image.EndInit();
//
//            return image;
        }
    }
}