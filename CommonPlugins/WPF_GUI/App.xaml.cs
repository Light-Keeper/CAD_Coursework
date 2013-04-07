using System.Threading;
using System.Windows;
using MediatorLib;
using WPF_GUI.ViewModels;

namespace WPF_GUI
{
    public partial class App : Application
    {
        public LogWindow LogViewer =
            new LogWindow
            {
                DataContext = new LogWindowViewModel()
            };

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            this.LogViewer.Close();
        }
    }
}
