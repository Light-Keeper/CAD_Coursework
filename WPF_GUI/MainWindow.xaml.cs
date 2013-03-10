using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void UpdatePictureEvent()
        {
            MessageBox.Show("UpdatePictureEvent called!");
            Picture p =  StaticLoader.GetPicture(0, 0, 1, 1);
            ///............
            StaticLoader.FreePicture(p);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var s = StaticLoader.GetModuleList();
            StaticLoader.StartPlaceMoule(s[0], false);
        }

    }
}
