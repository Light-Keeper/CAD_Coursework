using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF_GUI
{
    public partial class MainWindow : Window
    {
        private readonly Cursor _cursorGrab;
        private readonly Cursor _cursorGrabbing;
        private Point _mouseOffset;
        private bool _isDragging;

        public MainWindow()
        {
            InitializeComponent();
            _cursorGrab = ((TextBlock) this.Resources["CursorGrab"]).Cursor;
            _cursorGrabbing = ((TextBlock) this.Resources["CursorGrabbing"]).Cursor;
            DisplayedImage.Cursor = _cursorGrab;
            _isDragging = false;
        }

        private void DisplayedImage_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DisplayedImage.Cursor = this._cursorGrabbing;
            _mouseOffset = e.GetPosition(sender as Image);
            _isDragging = true;
        }

        private void DisplayedImage_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            DisplayedImage.Cursor = this._cursorGrab;
            _isDragging = false;
        }

        private void DisplayedImage_OnMouseMove(object sender, MouseEventArgs e)
        {
//            if (e.LeftButton != MouseButtonState.Pressed || !_isDragging || sender == null)
//            {
//                return;
//            }

//            var canvas = (sender as Image).Parent as Canvas;
//            var mouse = e.GetPosition(canvas);
//            mouse.Offset(-_mouseOffset.X, -_mouseOffset.Y);
//            DisplayedImage.SetValue(Canvas.LeftProperty, mouse.X);
//            DisplayedImage.SetValue(Canvas.TopProperty, mouse.Y);
        }
    }
}