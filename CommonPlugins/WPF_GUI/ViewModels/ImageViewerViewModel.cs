using System;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.ZoomChanged, (Action<int>)this.ChangeImageZoom);
        }

        #region ImageZoom
        private int _imageZoom = 100;
        public int ImageZoom
        {
            get { return _imageZoom; }
            set
            {
                if (_imageZoom == value) return;
                _imageZoom = value;
                RaisePropertyChanged(() => ImageZoom);
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
            this.ImageZoom = zoom;
            this.ImageHeight = 10*zoom;
        }
    }
}
