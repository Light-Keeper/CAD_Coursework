using System.Collections.Generic;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class StatusBarViewModel : BaseViewModel
    {
        private class StateInfo
        {
            public string ImagePath;
            public string Description;
        }

        private const int MIN_IMAGE_ZOOM = 10;
        private const int MAX_IMAGE_ZOOM = 200;

        private readonly List<StateInfo> _stateInfo = new List<StateInfo>();

        #region Properties

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
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ZoomChanged, _imageZoom);
            }
        }

        #endregion

        #region CurrentState
        private readonly int _currentState;
        #endregion

        #region ImageStatePath
        public string ImageStatePath 
        {
            get { return _stateInfo[_currentState].ImagePath; } 
        }
        #endregion

        #region ZoomToolTip
        public string ZoomToolTip
        {
            get { return "Масштаб " + this.ImageZoom + "%"; }
        }
        #endregion

        #region ImageStateToolTip
        public string ImageStateToolTip
        {
            get { return _stateInfo[_currentState].Description; }
        }
        #endregion

        #endregion

        #region Public Methods
        public StatusBarViewModel()
        {
            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/ok.png",
                Description = "Программа готова к работе"
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/loading.png",
                Description = "Программа выполняет действия"
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/error.png",
                Description = "Произошла ошибка в ходе работы программы"
            });

            this.InfoMessage = "Что-то там случилось";
            this.ImageZoom = 100;
            _currentState = 1;
        }
        #endregion
    }
}
