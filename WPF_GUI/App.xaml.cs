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

//        private static readonly Mediator _mediator = new Mediator();
//        public Mediator Mediator
//        {
//            get { return _mediator; }
//        }

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
