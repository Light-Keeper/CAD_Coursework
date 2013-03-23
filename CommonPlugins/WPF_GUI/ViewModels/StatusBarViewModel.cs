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
        public StatusBarViewModel()
        {
            this.InfoMessage = "Что-то там случилось";
            this.ImageZoom = 100;
        }

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
                _imageZoom = value;
                RaisePropertyChanged(() => ImageZoom);
                Mediator.NotifyColleagues(MediatorMessages.ZoomChanged, _imageZoom);
            }
        }
        #endregion
    }
}
