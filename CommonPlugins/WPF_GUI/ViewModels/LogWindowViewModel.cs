using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using MediatorLib;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class LogWindowViewModel : BaseViewModel
    {
        public LogWindowViewModel()
        {
            LogCollection = new ObservableCollection<Log>();

            for (var i = 0; i < 20; i++)
            {
                this.AddLog(new Log
                    {
                        CreateTime = DateTime.Now,
                        Message = "Тестовое сообщение " + (i+1)
                    });
            }
        }

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
                if (_isLogVisible == Visibility.Hidden) Mediator.NotifyColleagues(MediatorMessages.LogWindowClosed, true);
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public void AddLog(Log log)
        {
            _logCollection.Add(log);
        }
        #endregion
    }
}

