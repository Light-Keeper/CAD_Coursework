using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_GUI
{
    public partial class LogWindow : Window
    {
        public struct LogInfo
        {
            public DateTime Time;
            public String Msg;
        }

        private static readonly int MAX_MSG_IN_BUFFER = 200;

        private static readonly List<LogInfo> Buffer = new List<LogInfo>();

        public LogWindow()
        {
            InitializeComponent();
            foreach (var log in Buffer)
            {
                LogViewer.Text += FormLogMsg(log);
            }
        }

        public static string FormLogMsg(LogInfo logInfo)
        {
            return "[" + logInfo.Time.ToLongTimeString() + "]: " + logInfo.Msg + "\n";
        }

        public void Print(string str)
        {
            var newLog = new LogInfo
                {
                    Msg = str,
                    Time = DateTime.Now
                };

            Buffer.Add(newLog);

            // Pushing new log to console
            LogViewer.Text += FormLogMsg(newLog);

            // Delete first output log
            if (Buffer.Count > MAX_MSG_IN_BUFFER)
            {
                Buffer.RemoveAt(0);
            }
        }

        private void LogWindow_OnClosing(object sender, CancelEventArgs e)
        {
//            StaticLoader.MainWindow.ShowLogViewer.IsChecked = false;
        }
    }
}
