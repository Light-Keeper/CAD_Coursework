using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class ControlPanelViewModel : BaseViewModel
    {
        public ControlPanelViewModel() // Must be deleted in realese
        {
            RouteMethodCollection = new ObservableCollection<string>();
            MapMethodCollection = new ObservableCollection<string>();

            for (var i = 0; i < 5; i++)
            {
                RouteMethodCollection.Add("Метод трассировки " + (i + 1));
                MapMethodCollection.Add("Метод компановки " + (i + 1));
            }

            this.IsDemoMode = true;
        }

        #region StartButtonName
        private string _startButtonName;
        public string StartButtonName
        {
            get { return _startButtonName; }
            set
            {
                if (_startButtonName == value) return;
                _startButtonName = value;
                RaisePropertyChanged(() => StartButtonName);
            }
        }
        #endregion

        #region IsDemoMode
        private bool _isDemoMode;
        public bool IsDemoMode
        {
            get { return _isDemoMode; }
            set
            {
                if (_isDemoMode == value) return;
                _isDemoMode = value;
                if (_isDemoMode) this.StartButtonName = "Показать";
                RaisePropertyChanged(() => IsDemoMode);
                RaisePropertyChanged(() => StartButtonName);
            }
        }
        #endregion

        #region IsAutoMode
        private bool _isAutoMode;
        public bool IsAutoMode
        {
            get { return _isAutoMode; }
            set
            {
                if (_isAutoMode == value) return;
                _isAutoMode = value;
                if (_isAutoMode) this.StartButtonName = "Запустить";
                RaisePropertyChanged(() => IsAutoMode);
            }
        }
        #endregion

        #region IsStepMode
        private bool _isStepMode;
        public bool IsStepMode
        {
            get { return _isStepMode; }
            set
            {
                if (_isStepMode == value) return;
                _isStepMode = value;
                if (_isStepMode) this.StartButtonName = "Шаг";
                RaisePropertyChanged(() => IsStepMode);
            }
        }
        #endregion

        #region RouteMethodCollection
        private ObservableCollection<string> _routeMethodCollection;
        public ObservableCollection<string> RouteMethodCollection
        {
            get { return _routeMethodCollection; }
            set
            {
                if (_routeMethodCollection == value) return;
                _routeMethodCollection = value;
                RaisePropertyChanged(() => RouteMethodCollection);
            }
        }
        #endregion

        #region MapMethodCollection
        private ObservableCollection<string> _mapMethodCollection;
        public ObservableCollection<string> MapMethodCollection
        {
            get { return _mapMethodCollection; }
            set
            {
                if (_mapMethodCollection == value) return;
                _mapMethodCollection = value;
                RaisePropertyChanged(() => MapMethodCollection);
            }
        }
        #endregion

        #region SelectedRouteMethod
        private string _selectedRouteMethod;
        public string SelectedRouteMethod
        {
            get { return _selectedRouteMethod; }
            set
            {
                if (_selectedRouteMethod == value) return;

                _selectedRouteMethod = value;
                RaisePropertyChanged(() => SelectedRouteMethod);
            }
        }
        #endregion

        #region SelectedMapMethod
        private string _selectedMapMethod;
        public string SelectedMapMethod
        {
            get { return _selectedMapMethod; }
            set
            {
                if (_selectedMapMethod == value) return;

                _selectedMapMethod = value;
                RaisePropertyChanged(() => SelectedMapMethod);
            }
        }
        #endregion

        #region ShowLog
        private bool _showLog;
        public bool ShowLog
        {
            get { return _showLog; }
            set
            {
                if (_showLog == value) return;
                _showLog = value;
                RaisePropertyChanged(() => ShowLog);
                Mediator.NotifyColleagues(MediatorMessages.ShowLogWindow, _showLog);
            }
        }
        #endregion
    }
}
