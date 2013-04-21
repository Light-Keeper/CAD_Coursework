using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_GUI.Helpers;
using MessageBox = System.Windows.MessageBox;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.ZoomChanged, (Action<int>)this.ChangeImageZoom);
//            StaticLoader.Mediator.Register(MediatorMessages.RefreshImage, (Action<bool>) this.RefreshImage);
        }

        #region ImageSource
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (Equals(_imageSource, value)) return;
                _imageSource = value;
                RaisePropertyChanged(() => ImageSource);
            }
        }
        #endregion

        #region ImageHeight
        private double _imageHeight;
        public double ImageHeight
        {
            get { return _imageHeight; }
            set
            {
                if (_imageHeight == value) return;
                _imageHeight = value;
                RaisePropertyChanged(() => ImageHeight);
            }
        }
        #endregion

        public void ChangeImageZoom(int zoom)
        {
            this.ImageHeight = SystemParameters.WorkArea.Height * zoom / 100;
        }

        public void RefreshImage(bool bitmap)
        {
            var path = Environment.CurrentDirectory + @"\plugins\tmp.png";

            MessageBox.Show(path);

            this.ImageSource = new BitmapImage(new Uri(path));
            RaisePropertyChanged(() => ImageSource);
        }
    }
}
