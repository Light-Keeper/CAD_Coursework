using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_GUI.Helpers;
using WPF_GUI.Models;

namespace WPF_GUI.ViewModels
{
    public class ControlPanelViewModel : BaseViewModel
    {
        public ControlPanelViewModel()
        {
            this.PlaceMethodCollection = new ObservableCollection<Place>();
            this.RouteMethodCollection = new ObservableCollection<Route>();

            // Initialize Place and Route Collection
            foreach (var module in StaticLoader.GetModuleList())
            {
                var name = module.Remove(0, 1);
                switch (module[0])
                {
                    case 'P': // Place Method
                        PlaceMethodCollection.Add(
                            new Place
                                {
                                    Name = name,
                                    Command = SelectPlaceMethod
                                });
                        break;
                    case 'T': // Trace Method
                        RouteMethodCollection.Add(
                            new Route
                                {
                                    Name = name,
                                    Command = SelectRouteMethod
                                });
                        break;
                }
            }

            this.IsDemoMode = true;
            this.IsAutoExec = true;

            this.ConsoleButtonText = Defines.ConsoleButtonNameWhenClosed;

            this.IsStartButtonEnabled = true;
            this.IsStopButtonEnabled = false;

            this.IsPlaceMethodChecked = true;
            this.IsPlaceMethodEnabled = true;
            this.IsTraceMethodEnabled = true;

            StaticLoader.Mediator.Register(MediatorMessages.LogWindowClosed, (Action<bool>) this.ConsoleWasClosed);
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

        #region IsNormalMode
        private bool _isNormalMode;
        public bool IsNormalMode
        {
            get { return _isNormalMode; }
            set
            {
                if (_isNormalMode == value) return;
                _isNormalMode = value;
                RaisePropertyChanged(() => IsNormalMode);
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
                RaisePropertyChanged(() => IsDemoMode);
            }
        }
        #endregion

        #region IsStepExec
        private bool _isStepExec;
        public bool IsStepExec
        {
            get { return _isStepExec; }
            set
            {
                if (_isStepExec == value) return;
                _isStepExec = value;
                RaisePropertyChanged(() => IsStepExec);
            }
        }
        #endregion

        #region IsAutoExec
        private bool _isAutoExec;
        public bool IsAutoExec
        {
            get { return _isAutoExec; }
            set
            {
                if (_isAutoExec == value) return;
                _isAutoExec = value;
                RaisePropertyChanged(() => IsAutoExec);
            }
        }
        #endregion

        #region RouteMethodCollection
        public ObservableCollection<Route> RouteMethodCollection { get; private set; }
        #endregion

        #region PlaceMethodCollection
        public ObservableCollection<Place> PlaceMethodCollection { get; private set; }
        #endregion

        #region SelectedRouteMethod
        private Route _selectedRouteMethod;
        public Route SelectedRouteMethod
        {
            get { return _selectedRouteMethod; }
            set
            {
                if (_selectedRouteMethod == value) return;
                _selectedRouteMethod = value;
                RouteMethodCollection.First(x => x.Name == value.Name).IsChecked = true;
                RaisePropertyChanged(() => SelectedRouteMethod);
            }
        }
        #endregion

        #region SelectedPlaceMethod
        private Place _selectedPlaceMethod;
        public Place SelectedPlaceMethod
        {
            get { return _selectedPlaceMethod; }
            set
            {
                if (_selectedPlaceMethod == value) return;
                _selectedPlaceMethod = value;
                PlaceMethodCollection.First(x => x.Name == value.Name).IsChecked = true;
                RaisePropertyChanged(() => SelectedPlaceMethod);
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

        #region InputFilePath
        private string _inputFilePath;
        public string InputFilePath
        {
            get { return _inputFilePath; }
            set
            {
                if (_inputFilePath == value) return;
                _inputFilePath = value;
                RaisePropertyChanged(() => InputFilePath);
                RaisePropertyChanged(() => InputFileName);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.AddFileNameToTitle, InputFilePath);
            }
        }
        #endregion

        #region InputFileName
        public string InputFileName
        {
            get { return _inputFilePath.Split(new char[] {'\\'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault(); }
        }
        #endregion

        #region IsTraceMethodChecked
        private bool _isTraceMethodChecked;
        public bool IsTraceMethodChecked
        {
            get { return _isTraceMethodChecked; }
            set
            {
               if (_isTraceMethodChecked == value) return;
                _isTraceMethodChecked = value;
                RaisePropertyChanged(() => IsTraceMethodChecked);
            }
        }
        #endregion

        #region IsTraceMethodEnabled
        private bool _isTraceMethodEnabled;
        public bool IsTraceMethodEnabled
        {
            get { return _isTraceMethodEnabled; }
            set
            {
                if (_isTraceMethodEnabled == value) return;
                _isTraceMethodEnabled = value;
                RaisePropertyChanged(() => IsTraceMethodEnabled);
            }
        }
        #endregion

        #region IsPlaceMethodChecked
        private bool _isPlaceMethodChecked;
        public bool IsPlaceMethodChecked
        {
            get { return _isPlaceMethodChecked; }
            set
            {
                if (_isPlaceMethodChecked == value) return;
                _isPlaceMethodChecked = value;
                RaisePropertyChanged(() => IsPlaceMethodChecked);
            }
        }
        #endregion

        #region IsPlaceMethodEnabled
        private bool _isPlaceMethodEnabled;
        public bool IsPlaceMethodEnabled
        {
            get { return _isPlaceMethodEnabled; }
            set
            {
                if (_isPlaceMethodEnabled == value) return;
                _isPlaceMethodEnabled = value;
                RaisePropertyChanged(() => IsPlaceMethodEnabled);
            }
        }
        #endregion

        #endregion

        #region Commands

        public ICommand StartModeling { get { return new DelegateCommand(OnStartModeling); } }
        public ICommand StopModeling { get { return new DelegateCommand(OnStopModeling); } }
        public ICommand SelectPlaceMethod { get { return new DelegateCommand(OnSelectPlaceMethod); } }
        public ICommand SelectRouteMethod { get { return new DelegateCommand(OnSelectRouteMethod); } }
        public ICommand ShowInformation { get { return new DelegateCommand(OnShowInformation); } }
        public ICommand ShowConsole { get { return new DelegateCommand(OnShowConsole); } }
        public ICommand OpenSourceFile { get { return new DelegateCommand(OnOpenSourceFile); } }

        #endregion

        #region Private Methods

        private void OnStartModeling(object o)
        {
            this.IsStartButtonEnabled = false;
            this.IsStopButtonEnabled = true;

            
        }

        private void OnStopModeling(object o)
        {
            this.IsStartButtonEnabled = true;
            this.IsStopButtonEnabled = false;
        }

        private void OnShowInformation(object o)
        {
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ShowAboutPopup, true);
        }

        private void OnShowConsole(object o)
        {
            if (StaticLoader.Application.LogViewer.Visibility == Visibility.Visible)
            {
                StaticLoader.Application.LogViewer.Hide();
                this.ConsoleButtonText = Defines.ConsoleButtonNameWhenClosed;
            }
            else
            {
                StaticLoader.Application.LogViewer.Show();
                StaticLoader.Application.LogViewer.WindowState = WindowState.Normal;
                this.ConsoleButtonText = Defines.ConsoleButtonNameWhenOpened;
            }
        }

        private void OnOpenSourceFile(object o)
        {
            var dialog = new OpenFileDialog
                {
                    Title = "Открытие файла с входными данными",
                    Filter = "Файл данных (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    DefaultExt = "*.txt"
                };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.InputFilePath = dialog.FileName;

            if ( StaticLoader.LoadFile(new StringBuilder(this.InputFilePath)) )
            {
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetInfoMessage, InfoBarMessages.FileLoadSuccessful);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Defines.ProgramStateGood);
            }
            else
            {
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetInfoMessage, InfoBarMessages.FileLoadUnsuccessful);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Defines.ProgramStateError);
            }
        }

        private void OnSelectPlaceMethod(object o)
        {
            if (o == null) return;

            foreach (var place in PlaceMethodCollection)
            {
                if (place.Name == (o as String))
                {
                    this.SelectedPlaceMethod = place;
                }
                else
                {
                    place.IsChecked = false;
                }
            }
        }

        private void OnSelectRouteMethod(object o)
        {
            if (o == null) return;

            foreach (var route in RouteMethodCollection)
            {
                if (route.Name == (o as String))
                {
                    this.SelectedRouteMethod = route;
                }
                else
                {
                    route.IsChecked = false;
                }
            }
        }

        #endregion

        public void ConsoleWasClosed(bool state)
        {
            this.ConsoleButtonText = "Показать консоль";
        }
    }
}
