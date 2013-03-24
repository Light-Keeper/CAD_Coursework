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

            Mediator.Register(MediatorMessages.ShowLogWindow, (Action<bool>) this.ChangeVisibility);
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

        #region WindowVisibility
        private bool _windowVisibility;
        public bool WindowVisibility
        {
            get { return _windowVisibility; }
            set
            {
                if (_windowVisibility == value) return;
                _windowVisibility = value;
                RaisePropertyChanged(() => WindowVisibility);
            }
        }
        #endregion

        #endregion

//        [MediatorMessageSink(MediatorMessages.NewLog, ParameterType = typeof(Log))]
        public void AddLog(Log log)
        {
            _logCollection.Add(log);
        }

        public void ChangeVisibility(bool visibility)
        {
            this.WindowVisibility = visibility;
//            MessageBox.Show("xa-xa-xa" + visibility);
        }
    }
}

