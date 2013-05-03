using System.Windows;
using WPF_GUI.Helpers;

namespace WPF_GUI
{
    public partial class App : Application
    {
        public Program.State State { get; internal set; }

        public LogWindow LogViewer = new LogWindow();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
            LogViewer.Owner = this.MainWindow;
            State = Program.State.Good;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.LogViewer.Close();
            base.OnExit(e);
        }
    }
}