using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class LogWindowViewModel : BaseViewModel
    {
        #region Properties

        #region LogCollection
        private ObservableCollection<Log> _logCollection;
        public ObservableCollection<Log> LogCollection
        {
            get { return _logCollection; }
            set
            {
                if (_logCollection == value) return;
                _logCollection = value;
                RaisePropertyChanged(() => LogCollection);
            }
        }
        #endregion

        #region AllLogMessages
        public string AllLogMessages
        {
            get
            {
                return LogCollection.Aggregate("", (current, log) => current + (log.Formated + "\n"));
            }
        }

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
                if (_isLogVisible == Visibility.Hidden) StaticLoader.Mediator.NotifyColleagues(MediatorMessages.LogWindowClosed, true);
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region Constructor
        public LogWindowViewModel()
        {
            LogCollection = new ObservableCollection<Log>();
            StaticLoader.Mediator.Register(MediatorMessages.NewLog, (Action<string>)this.AddLog);
        }
        #endregion

        #region AddLog
        public void AddLog(string msg)
        {
            _logCollection.Add(
                new Log
                    {
                        CreateTime = DateTime.Now,
                        Message = msg
                    });
            RaisePropertyChanged(() => LogCollection);
        }
        #endregion

        #endregion
    }
}

