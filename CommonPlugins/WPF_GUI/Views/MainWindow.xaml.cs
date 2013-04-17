using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WPF_GUI.Helpers;

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

            this.Title = Defines.ProgramName;

            _cursorGrab = ((TextBlock) this.Resources["CursorGrab"]).Cursor;
            _cursorGrabbing = ((TextBlock) this.Resources["CursorGrabbing"]).Cursor;

            DisplayedImage.Cursor = _cursorGrab;
            _isDragging = false;

            StaticLoader.Mediator.Register(MediatorMessages.AddFileNameToTitle, (Action<string>) this.AddFileNameToTitle);
            StaticLoader.Mediator.Register(MediatorMessages.RefreshImage, (Action<BitmapSource>) this.RefreshImage);
            StaticLoader.UpdatePictureEvent(null);
        }

        private void RefreshImage(BitmapSource bitmapSource)
        {
            DisplayedImage.Source = bitmapSource;
        }

        private void AddFileNameToTitle(string fileName)
        {
            this.Title = fileName + " - " + Defines.ProgramName;
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
            FullControlPanel.Visibility = Visibility.Collapsed;
            MinControlPanel.Visibility = Visibility.Visible;
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            MinControlPanel.Visibility = Visibility.Collapsed;
            FullControlPanel.Visibility = Visibility.Visible;
        }

        private void OptionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            OptionsButtonContextMenu.PlacementTarget = sender as Button;
            OptionsButtonContextMenu.IsOpen = true;
        }

        private void OptionsButton_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}