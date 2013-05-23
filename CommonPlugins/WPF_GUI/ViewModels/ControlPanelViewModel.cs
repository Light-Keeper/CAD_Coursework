using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_GUI.Helpers;
using WPF_GUI.Models;
using WPF_GUI.Properties;
using MessageBox = System.Windows.MessageBox;

namespace WPF_GUI.ViewModels
{
    public class ControlPanelViewModel : BaseViewModel
    {
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

        #region IsInputSelectable
        public bool IsInputSelectable
        {
            get { return !_isStopButtonEnabled; }
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
                RaisePropertyChanged(() => IsStartButtonEnabled);
            }
        }
        #endregion

        #region TraceMethodCollection
        public ObservableCollection<Route> TraceMethodCollection { get; private set; }
        #endregion

        #region PlaceMethodCollection
        public ObservableCollection<Place> PlaceMethodCollection { get; private set; }
        #endregion

        #region SelectedTraceMethod
        private Route _selectedTraceMethod;
        public Route SelectedTraceMethod
        {
            get { return _selectedTraceMethod; }
            set
            {
                if (_selectedTraceMethod == value) return;
                _selectedTraceMethod = value;
                TraceMethodCollection.First(x => x.Name == value.Name).IsChecked = true;
                RaisePropertyChanged(() => SelectedTraceMethod);
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
        public bool IsStartButtonEnabled
        {
            get { return !(this.IsAutoExec && StaticLoader.Application.State == Program.State.Busy); }
        }
        #endregion

        #region StartButtonTooltip
        private string _startButtonTooltip;
        public string StartButtonTooltip
        {
            get { return _startButtonTooltip; }
            set
            {
                if (_startButtonTooltip == value) return;
                _startButtonTooltip = value;
                RaisePropertyChanged(() => StartButtonTooltip);
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
                RaisePropertyChanged(() => IsInputSelectable);
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

        #region IsFullControlPanelVisible
        private Visibility _isFullControlPanelVisible;
        public Visibility IsFullControlPanelVisible
        {
            get { return _isFullControlPanelVisible; }
            set
            {
                if (_isFullControlPanelVisible == value) return;
                _isFullControlPanelVisible = value;
                RaisePropertyChanged(() => IsFullControlPanelVisible);
                RaisePropertyChanged(() => IsMinimizedControlPanelVisible);
            }
        }
        #endregion

        #region IsMinimizedControlPanelVisible
        public Visibility IsMinimizedControlPanelVisible
        {
            get
            {
                return
                    _isFullControlPanelVisible == Visibility.Visible ?
                    Visibility.Collapsed :
                    Visibility.Visible;
            }
        }
        #endregion

        #region IsTraceMethodSelected
        private bool _isTraceMethodSelected;
        public bool IsTraceMethodSelected
        {
            get { return _isTraceMethodSelected; }
            set
            {
               if (_isTraceMethodSelected == value) return;
                _isTraceMethodSelected = value;
                RaisePropertyChanged(() => IsTraceMethodSelected);
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

        #region IsPlaceMethodSelected
        private bool _isPlaceMethodSelected;
        public bool IsPlaceMethodSelected
        {
            get { return _isPlaceMethodSelected; }
            set
            {
                if (_isPlaceMethodSelected == value) return;
                _isPlaceMethodSelected = value;
                RaisePropertyChanged(() => IsPlaceMethodSelected);
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
        public ICommand HideOrShowControlPanel { get { return new DelegateCommand(OnHideOrShowControlPanel); } }

        #endregion

        #region Private Methods

        #region OnHideOrShowControlPanel
        private void OnHideOrShowControlPanel(object o)
        {
            this.IsFullControlPanelVisible = 
                this.IsFullControlPanelVisible == Visibility.Visible ?
                Visibility.Collapsed : Visibility.Visible;
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.RefreshImageWidth);
        }
        #endregion

        #region OnStartModeling
        private void OnStartModeling(object o)
        {
            var kernelState = Kernel.GetState();

            switch (kernelState)
            {
                case Kernel.StateTrace:
                case Kernel.StatePlace:
                    if (this.IsTraceMethodSelected)
                    {
                        if (this.TraceMethodCollection.Count == 0)
                        {
                            MessageBox.Show(
                                Resources.TraceMethodIsNotLoaded,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        if (this.SelectedTraceMethod == null)
                        {
                            MessageBox.Show(
                                Resources.TraceMethodIsNotSelected,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, Resources.StartedTracing);
                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Busy);

                        var isOk = Kernel.StartTraceModule(this.SelectedTraceMethod.Name, this.IsDemoMode);                        

                        if (!isOk)
                        {
                            StaticLoader.Mediator.NotifyColleagues(
                                MediatorMessages.NewInfoMsg,
                                Resources.CanNotStartTracing);

                            MessageBox.Show(
                                Resources.CanNotStartTracing,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    if (this.IsPlaceMethodSelected)
                    {
                        if (this.PlaceMethodCollection.Count == 0)
                        {
                            MessageBox.Show(
                                Resources.PlaceMethodNotLoaded,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (this.SelectedPlaceMethod == null)
                        {
                            MessageBox.Show(
                                Resources.PlaceMethodNotSelected,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, Resources.StartedPlacing);
                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Busy);

                        var isOk = Kernel.StartPlaceModule(this.SelectedPlaceMethod.Name, this.IsDemoMode);

                        if (!isOk)
                        {
                            StaticLoader.Mediator.NotifyColleagues(
                                MediatorMessages.NewInfoMsg,
                                Resources.CanNotStartPlacing);

                            MessageBox.Show(
                                Resources.CanNotStartPlacing,
                                Resources.DialogBox_WarningTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    this.IsStopButtonEnabled = true;
                    this.StartButtonName = Resources.StartButtonName_Step;
                    this.StartButtonTooltip = Resources.MinControlPanel_StartModelingToolTip_Step;
                    RaisePropertyChanged(() => IsStartButtonEnabled);
                    break;

                case Kernel.StatePlacing:
                case Kernel.StateTracing:
                    Kernel.NextStep(this.IsDemoMode);
                    switch (Kernel.GetState())
                    {
                        case Kernel.StateEmpty:
                        case Kernel.StatePlace:
                        case Kernel.StateTrace:
                            var msg = kernelState == Kernel.StatePlacing ?
                                Resources.PlacingSuccessfulyEnded :
                                Resources.TracingSuccessfulyEnded;

                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Good);

                            this.IsStopButtonEnabled = false;
                            this.StartButtonName = Resources.StartButtonName_Start;
                            this.StartButtonTooltip = Resources.MinControlPanel_StartModelingToolTip_Start;

                            RaisePropertyChanged(() => IsStartButtonEnabled);

                            MessageBox.Show(
                                msg,
                                Resources.DialogBox_InfoTitle,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            return;
                    }
                    break;

                case Kernel.StateEmpty:
                    MessageBox.Show(
                        StaticLoader.Application.State == Program.State.Good ?
                            Resources.OpenSourceFile_FileNotOpened :
                            Resources.OpenSourceFile_IncorrectFile,
                        Resources.DialogBox_WarningTitle,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
            }
        }
        #endregion

        #region OnStopModeling
        private void OnStopModeling(object o)
        {
            this.IsStopButtonEnabled = false;
            this.StartButtonName = Resources.StartButtonName_Start;
            this.StartButtonTooltip = Resources.MinControlPanel_StartModelingToolTip_Start;
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Good);
            RaisePropertyChanged(() => IsStartButtonEnabled);

            var kernelState = Kernel.GetState();

            if (kernelState == Kernel.StatePlacing)
            {
                MessageBox.Show(
                    Resources.PlaceWereBreaked,
                    Resources.DialogBox_WarningTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            else if (kernelState == Kernel.StateTracing)
            {
                MessageBox.Show(
                    Resources.TraceWereBreaked,
                    Resources.DialogBox_WarningTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        #endregion

        #region OnShowInformation
        private static void OnShowInformation(object o)
        {
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ShowAboutPopup);
        }
        #endregion

        #region OnShowConsole
        private void OnShowConsole(object o)
        {
            if (StaticLoader.Application.LogViewer.Visibility == Visibility.Visible)
            {
                StaticLoader.Application.LogViewer.Hide();
                this.ConsoleButtonText = Resources.ConsoleButtonName_Show;
            }
            else
            {
                StaticLoader.Application.LogViewer.Show();
                StaticLoader.Application.LogViewer.WindowState = WindowState.Normal;
                this.ConsoleButtonText = Resources.ConsoleButtonName_Hide;
            }
        }
        #endregion

        #region OnOpenSourceFile
        private void OnOpenSourceFile(object o)
        {
            var dialog = new OpenFileDialog
                {
                    Title = Resources.OpenSourceFile_DialogTitle,
                    Filter = Resources.OpenSourceFile_SupportedFormats,
                    DefaultExt = "*.txt"
                };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.InputFilePath = dialog.FileName;

            var result = Kernel.LoadFile(new StringBuilder(this.InputFilePath));

            if ( !result )
            {
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, Resources.SourceFileLoaded_Unsuccessful);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Error);
                return;
            }

            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewInfoMsg, Resources.SourceFileLoaded_Successful);
            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Program.State.Good);

            StaticLoader.Image.Render(true);

            StaticLoader.Image.RealWidth = Kernel.GetRealImageWidth();
            StaticLoader.Image.RealHeight = Kernel.GetRealImageHeight();

            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.ResizeImageScrollBar);

            var kernelState = Kernel.GetState();

            switch (kernelState)
            {
                case Kernel.StatePlace:
                    this.IsTraceMethodEnabled = false;
                    break;
                case Kernel.StateTrace:
                    this.IsTraceMethodEnabled = true;
                    break;
            }
        }
        #endregion

        #region OnSelectPlaceMethod
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
        #endregion

        #region OnSelectRouteMethod
        private void OnSelectRouteMethod(object o)
        {
            if (o == null) return;

            foreach (var route in TraceMethodCollection)
            {
                if (route.Name == (o as String))
                {
                    this.SelectedTraceMethod = route;
                }
                else
                {
                    route.IsChecked = false;
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods

        #region Constructor
        public ControlPanelViewModel()
        {
            this.PlaceMethodCollection = new ObservableCollection<Place>();
            this.TraceMethodCollection = new ObservableCollection<Route>();

            // Initialize Place and Route Collections
            foreach (var module in Kernel.GetModuleList())
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
                        TraceMethodCollection.Add(
                            new Route
                            {
                                Name = name,
                                Command = SelectRouteMethod
                            });
                        break;
                }
            }

            this.IsDemoMode = true;
            this.IsStepExec = true;

            this.IsStopButtonEnabled = false;

            this.IsTraceMethodSelected = true;

            this.IsPlaceMethodEnabled = PlaceMethodCollection.Count > 0;
            this.IsTraceMethodEnabled = TraceMethodCollection.Count > 0;

            this.IsFullControlPanelVisible = Visibility.Visible;

            this.StartButtonName = Resources.StartButtonName_Start;
            this.StartButtonTooltip = Resources.MinControlPanel_StartModelingToolTip_Start;

            this.ConsoleButtonText = Resources.ConsoleButtonName_Show;

            StaticLoader.Mediator.Register(MediatorMessages.LogWindowClosed, (Action) this.ConsoleWasClosed);
        }
        #endregion

        #region ConsoleWasClosed
        public void ConsoleWasClosed()
        {
            this.ConsoleButtonText = Resources.ConsoleButtonName_Show;
        }
        #endregion

        #endregion
    }
}
