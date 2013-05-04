﻿using System;
using System.Collections.Generic;
﻿using System.Linq;
﻿using System.Text;
﻿using System.Threading;
﻿using System.Windows;
﻿using MediatorLib;
﻿using WPF_GUI.Helpers;

namespace WPF_GUI
{
    public static class StaticLoader
    {
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
            Image.Render(true);
            return 0;
        }

        // Call from native code
        public static int CoreMessage(string msg)
        {
            if (Mediator != null)
            {
                Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, msg);
            }
            return msg.Length;
        }

        public static List<string> GetModuleList()
        {
            const int bufferSize = 10000;
            var str = new StringBuilder(bufferSize);
            var res = Kernel.GetModuleList(bufferSize, str);
            return res < 0 ?
                new List<string>() :
                str.ToString().Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}