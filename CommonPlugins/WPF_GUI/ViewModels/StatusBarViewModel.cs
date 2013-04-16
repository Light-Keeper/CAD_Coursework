﻿using System;
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

        #region ImageZoom
        private int _imageZoom;
        public int ImageZoom
        {
            get { return _imageZoom; }
            set
            {
                if (_imageZoom == value) return;

                if (value < this.ImageMinZoom)
                {
                    _imageZoom = this.ImageMinZoom;
                }
                else if (value > this.ImageMaxZoom)
                {
                    _imageZoom = this.ImageMaxZoom;
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

        #region ImageMinZoom
        private int _imageMinZoom;
        public int ImageMinZoom
        {
            get { return _imageMinZoom; }
            private set
            {
                if (_imageMinZoom == value) return;
                _imageMinZoom = value;
                RaisePropertyChanged(() => ImageMinZoom);
            }
        }
        #endregion

        #region ImageMaxZoom
        private int _imageMaxZoom;
        public int ImageMaxZoom
        {
            get { return _imageMaxZoom; }
            private set
            {
                if (_imageMaxZoom == value) return;
                _imageMaxZoom = value;
                RaisePropertyChanged(() => ImageMaxZoom);
            }
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
            this.ImageMinZoom = 10;
            this.ImageMaxZoom = 1000;
            this.ImageZoom = 100;
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