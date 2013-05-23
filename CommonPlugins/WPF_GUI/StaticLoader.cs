﻿using System;
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
                return msg.Length;
            }
            return 0;
        }
    }
}