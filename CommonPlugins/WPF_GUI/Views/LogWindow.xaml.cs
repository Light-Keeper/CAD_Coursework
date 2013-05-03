using System.Windows;
using System.Windows.Input;
using WPF_GUI.ViewModels;

namespace WPF_GUI.Views
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
            DataContext = new LogWindowViewModel();
            this.Title =  "Log Viewer - " + Properties.Resources.ProgramName;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            this.Visibility = Visibility.Hidden;
        }

        private void LogViewer_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }
    }
}