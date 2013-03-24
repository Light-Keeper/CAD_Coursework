using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class StatusBarViewModel : BaseViewModel
    {
        private const int MIN_IMAGE_ZOOM = 10;
        private const int MAX_IMAGE_ZOOM = 200;

        public StatusBarViewModel()
        {
            this.InfoMessage = "Что-то там случилось";
            this.ImageZoom = 100;
        }

        #region Properties

        #region ZoomInfo
        public string ZoomInfo
        {
            get { return "Масштаб " + this.ImageZoom + "%"; }
        }
        #endregion

        #region InfoMessage
        private string _infoMessage;
        public string InfoMessage
        {
            get { return _infoMessage; }
            set
            {
                if (_infoMessage == value) return;
                _infoMessage = value;
                RaisePropertyChanged(() => InfoMessage);
            }
        }
        #endregion

        #region ImageZoom
        private int _imageZoom;
        public int ImageZoom
        {
            get { return _imageZoom; }
            set
            {
                if (_imageZoom == value) return;

                if (value < MIN_IMAGE_ZOOM)
                {
                    _imageZoom = MIN_IMAGE_ZOOM;
                }
                else if (value > MAX_IMAGE_ZOOM)
                {
                    _imageZoom = MAX_IMAGE_ZOOM;
                }
                else
                {
                    _imageZoom = value;
                }

                RaisePropertyChanged(() => ImageZoom);
                Mediator.NotifyColleagues(MediatorMessages.ZoomChanged, _imageZoom);
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #endregion
    }
}
