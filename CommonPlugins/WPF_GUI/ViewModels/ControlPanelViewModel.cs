using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_GUI.Helpers;
using WPF_GUI.Models;
using WPF_GUI.Views;

namespace WPF_GUI.ViewModels
{
    public class ControlPanelViewModel : BaseViewModel
    {
        public ControlPanelViewModel()
        {
            for (var i = 0; i < 5; i++) // Must be deleted in realese
            {
                RouteMethodCollection.Add("Метод трассировки " + (i + 1));
                MapMethodCollection.Add("Метод компановки " + (i + 1));
            }

            this.IsDemoMode = true;
            this.ConsoleButtonText = "Показать консоль";
            this.IsStartButtonEnabled = true;
            this.IsStopButtonEnabled = false;
            this.IsAllElementsEnabled = true;

            Mediator.Register(MediatorMessages.LogWindowClosed, (Action<bool>) this.ConsoleWasClosed);
        }

        #region Properties

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
        private ObservableCollection<string> _routeMethodCollection = new ObservableCollection<string>();
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
        private ObservableCollection<string> _mapMethodCollection = new ObservableCollection<string>();
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

        #region IsStartButtonEnabled
        private bool _isStartButtonEnabled;
        public bool IsStartButtonEnabled
        {
            get { return _isStartButtonEnabled; }
            set
            {
                if (_isStartButtonEnabled == value) return;
                _isStartButtonEnabled = value;
                RaisePropertyChanged(() => IsStartButtonEnabled);
            }
        }
        #endregion

        #region IsStopButtonEnabled
        private bool _isStopButtonEnabled;
        public bool IsStopButtonEnabled
        {
            get { return _isStopButtonEnabled; }
            set
            {
                if (_isStopButtonEnabled == value) return;
                _isStopButtonEnabled = value;
                RaisePropertyChanged(() => IsStopButtonEnabled);
            }
        }
        #endregion

        #region ConsoleButtonText
        private string _consoleButtonText;
        public string ConsoleButtonText
        {
            get { return _consoleButtonText; }
            set
            {
                if (_consoleButtonText == value) return;
                _consoleButtonText = value;
                RaisePropertyChanged(() => ConsoleButtonText);
            }
        }
        #endregion

        #region IsAllElementsEnabled
        private bool _isAllElementsEnabled;
        public bool IsAllElementsEnabled
        {
            get { return _isAllElementsEnabled; }
            set
            {
                if (_isAllElementsEnabled == value) return;
                _isAllElementsEnabled = value;
                RaisePropertyChanged(() => IsAllElementsEnabled);
            }
        }

        #endregion

        #endregion

        #region Commands

        public ICommand StartModeling { get { return new DelegateCommand(OnStartModeling); } }
        public ICommand StopModeling { get { return new DelegateCommand(OnStopModeling); } }
        public ICommand ShowInformation { get { return new DelegateCommand(OnShowInformation); } }
        public ICommand ShowConsole { get { return new DelegateCommand(OnShowConsole); } }
        public ICommand RefreshContent {get { return new DelegateCommand(OnRefreshContent); } }

        #endregion

        #region Private Methods

        private void OnStartModeling()
        {
            this.IsStartButtonEnabled = false;
            this.IsStopButtonEnabled = true;
            this.IsAllElementsEnabled = false;
        }

        private void OnStopModeling()
        {
            this.IsStartButtonEnabled = true;
            this.IsStopButtonEnabled = false;
            this.IsAllElementsEnabled = true;
        }

        private void OnShowInformation()
        {
            var infoWindow = new AboutWindow();
            infoWindow.ShowDialog();
        }

        private void OnShowConsole()
        {
            if (StaticLoader.Application.LogViewer.Visibility == Visibility.Visible)
            {
                StaticLoader.Application.LogViewer.Hide();
                this.ConsoleButtonText = "Показать консоль";
            }
            else
            {
                StaticLoader.Application.LogViewer.Show();
                StaticLoader.Application.LogViewer.WindowState = WindowState.Normal;
                this.ConsoleButtonText = "Скрыть консоль";
            }
        }

        private void OnRefreshContent()
        {
            RaisePropertyChanged(() => IsStartButtonEnabled);
        }

        #endregion

        public void ConsoleWasClosed(bool state)
        {
            this.ConsoleButtonText = "Показать консоль";
        }
    }
}
