using System;
using System.Windows;
using System.Windows.Forms;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.ZoomChanged, (Action<int>)this.ChangeImageZoom);
        }

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
    }
}
