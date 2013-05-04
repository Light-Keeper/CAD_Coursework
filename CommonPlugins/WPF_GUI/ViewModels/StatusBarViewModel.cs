using System;
using System.Collections.Generic;
using WPF_GUI.Helpers;
using WPF_GUI.Properties;

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

        #region Constants
        private const int MinZoomConst = 20;
        private const int MaxZoomConst = 400;
        #endregion

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
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewImageZoom, _zoom);
            }
        }
        #endregion

        #region MinZoom
        public int MinZoom
        {
            get { return MinZoomConst; }
        }
        #endregion

        #region MaxZoom
        public int MaxZoom
        {
            get { return MaxZoomConst; }
        }
        #endregion

        #region CurrentState
        private Program.State _currentState;
        public Program.State CurrentState {
            get { return _currentState; }
            set
            {
                if (value == _currentState) return;
                _currentState = value;
                StaticLoader.Application.State = value;
                RaisePropertyChanged(() => ImageStatePath);
                RaisePropertyChanged(() => ImageStateToolTip);
            }}
        #endregion

        #region ImageStatePath
        public string ImageStatePath 
        {
            get { return _stateInfo[(int)_currentState].ImagePath; } 
        }
        #endregion

        #region ZoomToolTip
        public string ZoomToolTip
        {
            get { return Resources.StatusBar_ZoomToolTipName + " " + this.Zoom + "%"; }
        }
        #endregion

        #region ImageStateToolTip
        public string ImageStateToolTip
        {
            get { return _stateInfo[(int)_currentState].Description; }
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
                Description = Resources.ProgramStateDescription_Good
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/loading.png",
                Description = Resources.ProgramStateDescription_Busy
            });

            _stateInfo.Add(new StateInfo
            {
                ImagePath = "../Recources/Images/error.png",
                Description = Resources.ProgramStateDescription_Error
            });

            _currentState = StaticLoader.Application.State;

            StaticLoader.Mediator.Register(MediatorMessages.NewInfoMsg, (Action<string>) this.SetInfoMessage);
            StaticLoader.Mediator.Register(MediatorMessages.SetProgramState, (Action<Program.State>) this.SetProgramState);
            StaticLoader.Mediator.Register(MediatorMessages.ChangeZoom, (Action<int>) this.ChangeImageZoom);
        }
        #endregion

        #region SetInfoMessage
        public void SetInfoMessage(string msg)
        {
            this.InfoMessage = msg;
        }
        #endregion

        #region SetProgramState
        public void SetProgramState(Program.State state)
        {
            this.CurrentState = state;
        }
        #endregion

        #region ChangeImageZoom
        public void ChangeImageZoom(int delta)
        {
            this.Zoom += delta;
        }
        #endregion

        #endregion
    }
}