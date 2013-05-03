using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class LogWindowViewModel : BaseViewModel
    {
        #region Properties

        #region LogMessages
        private readonly StringBuilder _logMessages = new StringBuilder();
        public StringBuilder LogMessages { get { return _logMessages; } }
        #endregion

        #region IsLogVisible
        private Visibility _isLogVisible;
        public Visibility IsLogVisible
        {
            get { return _isLogVisible; }
            set
            {
                if (_isLogVisible == value) return;
                _isLogVisible = value;
                RaisePropertyChanged(() => IsLogVisible);
                if (_isLogVisible == Visibility.Hidden) StaticLoader.Mediator.NotifyColleagues(MediatorMessages.LogWindowClosed);
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region Constructor
        public LogWindowViewModel()
        {
            StaticLoader.Mediator.Register(MediatorMessages.NewInfoMsg, (Action<string>) this.AddLog);
        }
        #endregion

        #region AddLog
        public void AddLog(string msg)
        {
            _logMessages.AppendLine(
                (new Log
                {
                    CreateTime = DateTime.Now,
                    Message = msg
                }).Formated);
            RaisePropertyChanged(() => LogMessages);
        }
        #endregion

        #endregion
    }
}