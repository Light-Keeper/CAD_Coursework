using System.Windows;
using WPF_GUI.Helpers;
using WPF_GUI.ViewModels;

namespace WPF_GUI
{
    public partial class App : Application
    {
        public int ProgramState = Defines.ProgramStateGood;

        public LogWindow LogViewer = new LogWindow();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.MainWindow = new MainWindow();
            this.MainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.LogViewer.Close();
            base.OnExit(e);
        }
    }
}