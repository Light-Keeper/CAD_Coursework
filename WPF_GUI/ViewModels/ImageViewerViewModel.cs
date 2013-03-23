using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using MediatorLib;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            Mediator.Register(MediatorMessages.ZoomChanged, (Action<int>) this.ChangeImageZoom);
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
            this.ImageHeight = (SystemParameters.WorkArea.Height - 80.0) * zoom / 100;
        }
    }
}
