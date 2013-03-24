using System.Windows;

namespace WPF_GUI
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
            this.Visibility = Visibility.Hidden;
        }
    }
}
