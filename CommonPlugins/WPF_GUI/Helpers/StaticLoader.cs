﻿using System;
using System.Collections.Generic;
﻿using System.Linq;
﻿using System.Text;
using System.Runtime.InteropServices;
﻿using System.Threading;
﻿using System.Windows;
﻿using MediatorLib;
﻿using WPF_GUI.Helpers;

namespace WPF_GUI
{
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
        private static extern void RenderPicture(IntPtr hWnd, double x1, double y1, double x2, double y2);

        [DllImport("GUI_CLR_loader.dll")]
        private static extern void SetPictureSize(UInt32 width, UInt32 height);

        #endregion

        public static App Application;
        public static Mediator Mediator;
        public static ImageHost Image;

        // Call from native code
        public static int Exec(string msg)
        {
            try
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                Image = new ImageHost();
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
            RenderPicture(Image.Handle, 0, 0, 1, 1);
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
    }
}