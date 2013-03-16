﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace WPF_GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LogWindow _logViewer;

        private Cursor handCursor;
        private Cursor dragHandCursor;

        public MainWindow()
        {
            InitializeComponent();
            InitializeCursors();

            //            GridViewer.Cursor = handCursor;
        }

        private void InitializeCursors()
        {
            //            var uri = new Uri("cursor_hand.ico", UriKind.Relative);

            //            handCursor = new Cursor(GetCURFromICO(uri, 1, 1));

            //            uri = new Uri("pack://application:,,,/Content/cursor_drag_hand.ico");
            //            dragHandCursor = new Cursor(GetCURFromICO(uri, 1, 1));
        }

        public void UpdatePictureEvent()
        {
            //            MessageBox.Show("UpdatePictureEvent called!");
            //            Picture p =  StaticLoader.GetPicture(0, 0, 1, 1);
            //            ///............
            //            StaticLoader.FreePicture(p);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //            var s = StaticLoader.GetModuleList();
            //            StaticLoader.StartPlaceMoule(s[0], false);
        }

        private void ShowLogViewer_OnChecked(object sender, RoutedEventArgs e)
        {
            _logViewer = new LogWindow();
            _logViewer.Show();
            _logViewer.Print("Тестовая строка");
        }

        private void ShowLogViewer_OnUnchecked(object sender, RoutedEventArgs e)
        {
            _logViewer.Close();
        }

        public Stream GetCURFromICO(Uri uri, byte hotspotx, byte hotspoty)
        {
            var sri = Application.GetResourceStream(uri);

            if (sri == null)
            {
                MessageBox.Show("Something wrong with reciurce ico file");
                return null;
            }

            var stream = sri.Stream;

            var buffer = new byte[stream.Length];

            stream.Read(buffer, 0, (int)stream.Length);

            var ms = new MemoryStream();

            buffer[2] = 2; // change to CUR file type
            buffer[10] = hotspotx;
            buffer[12] = hotspoty;

            ms.Write(buffer, 0, (int)stream.Length);

            ms.Position = 0;

            return ms;
        }

        private void GridViewer_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //            GridViewer.Cursor = Cursors.UpArrow;
        }

        private void GridViewer_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //            GridViewer.Cursor = Cursors.Arrow;
        }
    }
}