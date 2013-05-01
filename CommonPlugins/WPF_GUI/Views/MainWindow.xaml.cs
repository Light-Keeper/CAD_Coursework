using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using WPF_GUI.Helpers;

namespace WPF_GUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Title = Defines.ProgramName;

            StaticLoader.Mediator.Register(MediatorMessages.AddFileNameToTitle, (Action<string>) this.AddFileNameToTitle);
            StaticLoader.Mediator.Register(MediatorMessages.RefreshImageWidth, (Action) this.RefreshImageWidth);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            BorderForImage.Child = StaticLoader.Image;

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

            StaticLoader.Image.Width = BorderForImage.ActualWidth;
            StaticLoader.Image.Height = BorderForImage.ActualHeight;

            StaticLoader.Image.Render(true);
        }

        public void RefreshImageWidth()
        {
            StaticLoader.Image.Width = ImageViewer.Width;
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            StaticLoader.Image.Width -= (e.PreviousSize.Width - e.NewSize.Width);
            StaticLoader.Image.Height -= (e.PreviousSize.Height - e.NewSize.Height);
        }

        private void AddFileNameToTitle(string fileName)
        {
            this.Title = fileName + " - " + Defines.ProgramName;
        }

        // Change Image zoom with hotkeys
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