using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace WPF_GUI
{
    public partial class MainWindow : Window
    {
        private readonly Cursor _cursorGrab;
        private readonly Cursor _cursorGrabbing;

        private bool _isDragging;
        private Point _mouseOffset;

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
            _mouseOffset = e.GetPosition((sender as Image).Parent as ScrollViewer);
            _isDragging = true;
        }

        private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            DisplayedImage.Cursor = this._cursorGrab;
            _isDragging = false;
        }

        private void ScrollViewer_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || !_isDragging || sender == null)
            {
                return;
            }

            var mainImage = sender as ScrollViewer;
            var mouse = e.GetPosition(mainImage);

            mouse.Offset(-_mouseOffset.X, -_mouseOffset.Y);

            ImageViewer.ScrollToHorizontalOffset(ImageViewer.HorizontalOffset - mouse.X);
            ImageViewer.ScrollToVerticalOffset(ImageViewer.VerticalOffset - mouse.Y);

            _mouseOffset = e.GetPosition(mainImage);
        }

        private void ImageViewer_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Scroll Width
            if (Keyboard.Modifiers != ModifierKeys.Control &&
                Keyboard.Modifiers == ModifierKeys.Shift &&
                Keyboard.Modifiers != ModifierKeys.Alt)
            {
                e.Handled = true;

                if (e.Delta > 0)
                {
                    if (ImageViewer.HorizontalOffset > 1)
                    {
                        ImageViewer.LineLeft();
                        ImageViewer.LineLeft();
                        ImageViewer.LineLeft();
                        return;
                    }
                }

                if (e.Delta < 0)
                {
                    if (ImageViewer.HorizontalOffset < ImageViewer.ScrollableWidth)
                    {
                        ImageViewer.LineRight();
                        ImageViewer.LineRight();
                        ImageViewer.LineRight();
                        return;
                    }
                }
            }
            // Change Image Size
            if (Keyboard.Modifiers == ModifierKeys.Control &&
                Keyboard.Modifiers != ModifierKeys.Shift &&
                Keyboard.Modifiers != ModifierKeys.Alt)
            {
                e.Handled = true;

                if (e.Delta < 0)
                {
                    ImageZoom.Value -= 5;
                    return;
                }

                if (e.Delta > 0)
                {
                    ImageZoom.Value += 5;
                    return;
                }
            }
        }

        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control &&
                Keyboard.Modifiers != ModifierKeys.Shift &&
                Keyboard.Modifiers != ModifierKeys.Alt)
            {
                if (e.Key == Key.OemPlus)
                {
                    ImageZoom.Value += 5;
                    return;
                }
                if (e.Key == Key.OemMinus)
                {
                    ImageZoom.Value -= 5;
                    return;
                }
            }
        }

        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            MinControlPanel.Visibility = Visibility.Visible;

            var fadeOutStoryboard = this.Resources["FadeOut"] as Storyboard;

            if (fadeOutStoryboard == null) return;

            fadeOutStoryboard.Begin();
        }

        private void FadeOut_OnCompleted(object sender, EventArgs e)
        {
            FullControlPanel.Visibility = Visibility.Collapsed;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            FullControlPanel.Visibility = Visibility.Visible;

            var fadeInStoryboard = this.Resources["FadeIn"] as Storyboard;

            if (fadeInStoryboard == null) return;

            fadeInStoryboard.Begin();
        }

        private void FadeIn_OnCompleted(object sender, EventArgs e)
        {
            MinControlPanel.Visibility = Visibility.Collapsed;
        }
    }
}