using System.Windows;
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

            var mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel { IsAutoMode = true }
                };

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // Close log window and do something else
        }
    }
}
