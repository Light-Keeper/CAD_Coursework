using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using WPF_GUI.Helpers;

namespace WPF_GUI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            StaticLoader.Mediator.Register(MediatorMessages.AddFileNameToTitle, (Action<string>) this.AddFileNameToTitle);
            StaticLoader.Mediator.Register(MediatorMessages.RefreshImageWidth, (Action) this.RefreshImageWidth);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, Properties.Resources.ModulesLoadSuccessful);

            ImageViewer.Child = StaticLoader.Image;

            StaticLoader.Image.SetBinding(
                WidthProperty,
                new Binding
                {
                    Path = new PropertyPath("ImageWidth"),
                    Mode = BindingMode.TwoWay
                });

            StaticLoader.Image.SetBinding(
                HeightProperty,
                new Binding
                {
                    Path = new PropertyPath("ImageHeight"),
                    Mode = BindingMode.TwoWay
                });

            StaticLoader.Image.Width = ImageViewer.ActualWidth - ImageViewer.Padding.Left - ImageViewer.Padding.Right;
            StaticLoader.Image.Height = ImageViewer.ActualHeight - ImageViewer.Padding.Top - ImageViewer.Padding.Bottom;

            StaticLoader.Image.MaxWidth = StaticLoader.Image.Width;
            StaticLoader.Image.MaxHeight = StaticLoader.Image.Height;

            StaticLoader.Image.Render(true);

            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);
        }

        public void RefreshImageWidth()
        {
            ImageViewer.UpdateLayout();
            var newWidth = ImageViewer.ActualWidth - ImageViewer.Padding.Left - ImageViewer.Padding.Right;
            if (StaticLoader.Image.Width == StaticLoader.Image.MaxWidth)
            {
                StaticLoader.Image.Width = StaticLoader.Image.MaxWidth = newWidth;
            }
            else
            {
                StaticLoader.Image.MaxWidth = newWidth;
            }

            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StaticLoader.Image.Width -= (e.PreviousSize.Width - e.NewSize.Width);
            StaticLoader.Image.Height -= (e.PreviousSize.Height - e.NewSize.Height);

            StaticLoader.Image.MaxWidth -= (e.PreviousSize.Width - e.NewSize.Width);
            StaticLoader.Image.MaxHeight -= (e.PreviousSize.Height - e.NewSize.Height);

            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);
        }

        private void AddFileNameToTitle(string fileName)
        {
            this.Title = fileName + " - " + Properties.Resources.ProgramName;
        }

        // Change Image zoom with hotkeys
        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control &&
                Keyboard.Modifiers != ModifierKeys.Shift &&
                Keyboard.Modifiers != ModifierKeys.Alt)
            {
                var delta = StaticLoader.Image.RealHeight;
                delta -= StaticLoader.Image.RealVisibleHeight;
                delta += StaticLoader.Image.RealWidth;
                delta -= StaticLoader.Image.RealVisibleWidth;
                delta /= 200;

                delta = Math.Ceiling(delta);
                delta = Math.Max(1.0, delta);
                
                if (e.Key == Key.OemPlus)
                {
                    ImageZoom.Value += delta;
                    return;
                }
                if (e.Key == Key.OemMinus)
                {
                    ImageZoom.Value -= delta;
                    return;
                }
            }
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

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var scroll = sender as ScrollBar;

            if (scroll.Orientation == Orientation.Horizontal)
            {
                StaticLoader.Image.FirstVisiblePos.X = (int)(scroll.Value - scroll.Minimum);
            }
            else
            {
                StaticLoader.Image.FirstVisiblePos.Y = (int)(scroll.Value - scroll.Minimum);
            }
            StaticLoader.Image.Render();
        }
    }
}