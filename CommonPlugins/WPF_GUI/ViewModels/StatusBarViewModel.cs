using System;
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

        #region Zoom
        private int _zoom = 100;
        public int Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom == value) return;

                _zoom = value;

                RaisePropertyChanged(() => Zoom);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ZoomChanged, _zoom);
            }
        }
        #endregion

        #region MinZoom
        private int _minZoom = 100;
        public int MinZoom
        {
            get { return _minZoom; }
        }
        #endregion

        #region MaxZoom
        private int _maxZoom = 1000;
        public int MaxZoom
        {
            get { return _maxZoom; }
        }
        #endregion

        #region CurrentState
        private int _currentState;
        public int CurrentState {
            get { return _currentState; }
            set
            {
                if (value == _currentState) return;
                _currentState = value;
                StaticLoader.Application.ProgramState = value;
                RaisePropertyChanged(() => ImageStatePath);
                RaisePropertyChanged(() => ImageStateToolTip);
            }}
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
            get { return "Масштаб " + this.Zoom + "%"; }
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

        #region Constructor
        public StatusBarViewModel()
        {
            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/ok.png",
                Description = "Всё в порядке, программа готова к работе"
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/loading.png",
                Description = "Программа выполняет указанные действия"
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/error.png",
                Description = "Произошла ошибка в ходе работы программы"
            });

            this.InfoMessage = InfoBarMessages.ModulesLoadSuccessful;
            _currentState = Defines.ProgramStateGood;

            StaticLoader.Mediator.Register(MediatorMessages.SetInfoMessage, (Action<string>) this.SetInfoMessage);
            StaticLoader.Mediator.Register(MediatorMessages.SetProgramState, (Action<int>) this.SetProgramState);
        }
        #endregion

        #region SetInfoMessage
        public void SetInfoMessage(string msg)
        {
            this.InfoMessage = msg;
        }
        #endregion

        #region SetProgramState
        public void SetProgramState(int state)
        {
            this.CurrentState = state;
        }
        #endregion

        #endregion
    }
}