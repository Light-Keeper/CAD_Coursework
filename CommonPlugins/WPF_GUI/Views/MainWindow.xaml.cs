using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Navigation;
using WPF_GUI.Helpers;
using WPF_GUI.ViewModels;

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

            _isDragging = false;

            StaticLoader.Mediator.Register(MediatorMessages.AddFileNameToTitle, (Action<string>) this.AddFileNameToTitle);
            StaticLoader.UpdatePictureEvent(null);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            StaticLoader.Image.MessageHook += new HwndSourceHook(ControlMsgFilter);
            StaticLoader.Image.MouseDown += new MouseButtonEventHandler(Image_OnMouseDown);

            BorderForImage.Child = StaticLoader.Image;
        }

        private static IntPtr ControlMsgFilter(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StaticLoader.UpdatePictureEvent(null);
        }

        private void AddFileNameToTitle(string fileName)
        {
            this.Title = fileName + " - " + Defines.ProgramName;
        }

        private void Image_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseOffset = e.GetPosition((StaticLoader.Image.Parent as Border).Parent as ScrollViewer);
            _isDragging = true;
        }

        private void MainWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
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
            StaticLoader.UpdatePictureEvent(null);
        }

        private void MaximizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            MinControlPanel.Visibility = Visibility.Collapsed;
            FullControlPanel.Visibility = Visibility.Visible;
            StaticLoader.UpdatePictureEvent(null);
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