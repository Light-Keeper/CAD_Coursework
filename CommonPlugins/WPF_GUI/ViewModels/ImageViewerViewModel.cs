using System;
using System.Windows;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.ZoomChanged, (Action<int>) this.ChangeImageZoom);
        }

        #region ImageWidth
        private double _imageWidth;
        public double ImageWidth
        {
            get { return _imageWidth; }
            set
            {
                if (_imageWidth == value) return;
                _imageWidth = value;
                RaisePropertyChanged(() => ImageWidth);
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
            StaticLoader.Image.Scale = StaticLoader.Image.StartScale * zoom / 100;
            StaticLoader.Image.Render();
        }
    }
}
