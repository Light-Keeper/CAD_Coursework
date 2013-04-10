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
        private int _currentState;
        public int CurrentState {
            get { return _currentState; }
            set
            {
                if (value == _currentState) return;
                _currentState = value;
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

            this.InfoMessage = "Загрузка модулей программы прошла успешно";
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