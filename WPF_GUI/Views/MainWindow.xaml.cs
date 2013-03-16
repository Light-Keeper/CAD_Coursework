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
    public partial class MainWindow : Window
    {
        private readonly Cursor _cursorGrab;
        private readonly Cursor _cursorGrabbing;

        public MainWindow()
        {
            InitializeComponent();
            _cursorGrab = ((TextBlock) this.Resources["CursorGrab"]).Cursor;
            _cursorGrabbing = ((TextBlock) this.Resources["CursorGrabbing"]).Cursor;
            ImageViewer.Cursor = _cursorGrab;
        }

        private void ImageViewer_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ImageViewer.Cursor = this._cursorGrabbing;
        }

        private void ImageViewer_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ImageViewer.Cursor = this._cursorGrab;
        }
    }
}