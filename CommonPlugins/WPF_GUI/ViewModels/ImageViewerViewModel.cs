using System;
using System.Windows;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class ImageViewerViewModel : BaseViewModel
    {
        public ImageViewerViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.NewImageZoom, (Action<int>) this.SetNewImageZoom);
            StaticLoader.Mediator.Register(MediatorMessages.ResizeImageScrollBar, (Action) this.ResizeImageScrollBar);
        }

        #region VerticalScrollBarMinimum
        private double _verticalScrollBarMinimum;
        public double VerticalScrollBarMinimum
        {
            get { return _verticalScrollBarMinimum; }
            set
            {
                if (_verticalScrollBarMinimum == value) return;
                _verticalScrollBarMinimum = value;
                RaisePropertyChanged(() => VerticalScrollBarMinimum);
            }
        }
        #endregion

        #region VerticalScrollBarMaximum
        private double _verticalScrollBarMaximum;
        public double VerticalScrollBarMaximum
        {
            get { return _verticalScrollBarMaximum; }
            set
            {
                if (_verticalScrollBarMaximum == value) return;
                _verticalScrollBarMaximum = value;
                RaisePropertyChanged(() => VerticalScrollBarMaximum);
            }
        }
        #endregion

        #region VerticalScrollBarValue
        private double _verticalScrollBarValue;
        public double VerticalScrollBarValue
        {
            get { return _verticalScrollBarValue; }
            set
            {
                if (_verticalScrollBarValue == value) return;
                _verticalScrollBarValue = value;
                RaisePropertyChanged(() => VerticalScrollBarValue);
            }
        }
        #endregion

        #region VerticalScrollBarViewportSize
        private double _verticalScrollBarViewportSize;
        public double VerticalScrollBarViewportSize
        {
            get { return _verticalScrollBarViewportSize; }
            set
            {
                if (_verticalScrollBarViewportSize == value) return;
                _verticalScrollBarViewportSize = value;
                RaisePropertyChanged(() => VerticalScrollBarViewportSize);
            }
        }
        #endregion

        #region HorizontalScrollBarMinimum
        private double _horizontalScrollBarMinimum;
        public double HorizontalScrollBarMinimum
        {
            get { return _horizontalScrollBarMinimum; }
            set
            {
                if (_horizontalScrollBarMinimum == value) return;
                _horizontalScrollBarMinimum = value;
                RaisePropertyChanged(() => HorizontalScrollBarMinimum);
            }
        }
        #endregion

        #region HorizontalScrollBarMaximum
        private double _horizontalScrollBarMaximum;
        public double HorizontalScrollBarMaximum
        {
            get { return _horizontalScrollBarMaximum; }
            set
            {
                if (_horizontalScrollBarMaximum == value) return;
                _horizontalScrollBarMaximum = value;
                RaisePropertyChanged(() => HorizontalScrollBarMaximum);
            }
        }
        #endregion

        #region HorizontalScrollBarValue
        private double _horizontalScrollBarValue;
        public double HorizontalScrollBarValue
        {
            get { return _horizontalScrollBarValue; }
            set
            {
                if (_horizontalScrollBarValue == value) return;
                _horizontalScrollBarValue = value;
                RaisePropertyChanged(() => HorizontalScrollBarValue);
            }
        }
        #endregion

        #region HorizontalScrollBarViewportSize
        private double _horizontalScrollBarViewportSize;
        public double HorizontalScrollBarViewportSize
        {
            get { return _horizontalScrollBarViewportSize; }
            set
            {
                if (_horizontalScrollBarViewportSize == value) return;
                _horizontalScrollBarViewportSize = value;
                RaisePropertyChanged(() => HorizontalScrollBarViewportSize);
            }
        }
        #endregion

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

        private void SetNewImageZoom(int zoom)
        {
            StaticLoader.Image.Scale = ((double)(zoom)) / 100;
            StaticLoader.Image.Render();

            this.ResizeImageScrollBar();
        }

        private void ResizeImageScrollBar()
        {
            this.HorizontalScrollBarValue = StaticLoader.Image.RealVisibleWidth + StaticLoader.Image.FirstVisiblePos.X;
            this.VerticalScrollBarValue = StaticLoader.Image.RealVisibleHeight + StaticLoader.Image.FirstVisiblePos.Y;

            this.HorizontalScrollBarViewportSize = StaticLoader.Image.RealVisibleWidth;
            this.VerticalScrollBarViewportSize = StaticLoader.Image.RealVisibleHeight;

            this.HorizontalScrollBarMinimum = StaticLoader.Image.RealVisibleWidth;
            this.VerticalScrollBarMinimum = StaticLoader.Image.RealVisibleHeight;

            this.HorizontalScrollBarMaximum = StaticLoader.Image.RealWidth;
            this.VerticalScrollBarMaximum = StaticLoader.Image.RealHeight;
        }
    }
}
