using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using WPF_GUI.Helpers;
using WPF_GUI.Models;
using MessageBox = System.Windows.MessageBox;

namespace WPF_GUI.ViewModels
{
    public class ControlPanelViewModel : BaseViewModel
    {
        public ControlPanelViewModel()
        {
            this.PlaceMethodCollection = new ObservableCollection<Place>();
            this.TraceMethodCollection = new ObservableCollection<Route>();

            // Initialize Place and Route Collections
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

            this.IsStartButtonEnabled = true;
            this.IsStopButtonEnabled = false;

            this.IsTraceMethodChecked = true;

            this.IsPlaceMethodEnabled = true;
            this.IsTraceMethodEnabled = true;

            this.StartButtonName = Defines.StartButtonNameBegin;

            this.ConsoleButtonText = Defines.ConsoleButtonNameWhenClosed;

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
//            this.IsStartButtonEnabled = false;
//            this.IsStopButtonEnabled = true;

            var kernelState = StaticLoader.GetKernelState();

            switch (kernelState)
            {
                case Defines.KernelStatePlace:
                    if (this.PlaceMethodCollection.Count == 0)
                    {
                        MessageBox.Show("Методы компановки не загружены!\n" +
                            "Чтобы выполнить компановку добавьте dll файл(ы) " +
                            "с методами компановки в папку plugins.",
                            "Предупреждение",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    if (string.IsNullOrEmpty(this.SelectedPlaceMethod.Name))
                    {
                        MessageBox.Show("Метод компановки не выбран!\nВыберите метод и попробуйте снова.",
                            "Предупреждение",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    this.StartButtonName = Defines.StartButtonNameStep;

                    StaticLoader.StartPlaceModule(this.SelectedPlaceMethod.Name, this.IsDemoMode);

                    this.IsStopButtonEnabled = true;

                    break;

                case Defines.KernelStateTrace:
                    if (this.IsTraceMethodChecked)
                    {
                        if (this.TraceMethodCollection.Count == 0)
                        {
                            MessageBox.Show("Методы трассировки не загружены!\n" +
                                "Чтобы выполнить трассировку добавьте dll файл(ы) " +
                                "с методами трассировки в папку plugins и перезапустите приложение.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (this.SelectedTraceMethod == null)
                        {
                            MessageBox.Show("Метод трассировки не выбран!\nВыберите метод и попробуйте снова.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        var isOk = StaticLoader.StartTraceModule(this.SelectedTraceMethod.Name, this.IsDemoMode);

                        if (!isOk)
                        {
                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewLog,
                                "Не удалось запустить трассировку.");
                            MessageBox.Show("Не удалось запустить трассировку.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }

                        this.IsStopButtonEnabled = true;

                        this.StartButtonName = Defines.StartButtonNameStep;

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewLog,
                                "Начался процесс моделирования трассировки.");
                        return;
                    }

                    if (this.IsPlaceMethodChecked)
                    {
                        if (this.PlaceMethodCollection.Count == 0)
                        {
                            MessageBox.Show("Методы компановки не загружены!\n" +
                                "Чтобы выполнить компановку добавьте dll файл(ы) " +
                                "с методами компановки в папку plugins и перезапустите приложение.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (this.SelectedPlaceMethod == null)
                        {
                            MessageBox.Show("Метод компановки не выбран!\nВыберите метод и попробуйте снова.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        var isOk = StaticLoader.StartPlaceModule(this.SelectedPlaceMethod.Name, this.IsDemoMode);
                        
                        if (!isOk)
                        {
                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewLog,
                                "Не удалось запустить компановку.");
                            MessageBox.Show("Не удалось запустить компановку.",
                                "Предупреждение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }

                        this.IsStopButtonEnabled = true;

                        this.StartButtonName = Defines.StartButtonNameStep;

                        StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewLog,
                                "Начался процесс моделирования компановки.");
                        return;
                    }
                    break;

                case Defines.KernelStatePlacing:
                case Defines.KernelStateTracing:
                    StaticLoader.NextStep(this.IsDemoMode);
                    switch (StaticLoader.GetKernelState())
                    {
                        case Defines.KernelStateEmpty:
                        case Defines.KernelStatePlace:
                        case Defines.KernelStateTrace:
                            var msg = kernelState == Defines.KernelStatePlacing ?
                                "Компановка успешно завершена" :
                                "Трассировка успешно завершена";

                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.NewLog, msg);
                            StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetInfoMessage, msg);

                            MessageBox.Show(msg, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                            this.IsStopButtonEnabled = false;

                            this.StartButtonName = Defines.StartButtonNameBegin;

                            break;
                    }
                    break;

                case Defines.KernelStateEmpty:
                    MessageBox.Show(
                        StaticLoader.Application.ProgramState == Defines.ProgramStateGood
                            ? "Входной файл не выбран!\nОткройте файл с данными и повторите попытку."
                            : "Выбранный файл не содержит данных для моделирования!\nОткройте файл " +
                              "с корректными данными и повторите попытку.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
            }
        }

        private void OnStopModeling(object o)
        {
            this.IsStopButtonEnabled = false;
            this.StartButtonName = Defines.StartButtonNameBegin;
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

            var result = StaticLoader.LoadFile(new StringBuilder(this.InputFilePath));

            if ( result )
            {
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetInfoMessage, InfoBarMessages.FileLoadSuccessful);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Defines.ProgramStateGood);
            }
            else
            {
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetInfoMessage, InfoBarMessages.FileLoadUnsuccessful);
                StaticLoader.Mediator.NotifyColleagues(MediatorMessages.SetProgramState, Defines.ProgramStateError);
            }

            var kernelState = StaticLoader.GetKernelState();

            switch (kernelState)
            {
                case Defines.KernelStatePlace:
                    this.IsTraceMethodEnabled = false;
                    break;
                case Defines.KernelStateTrace:
                    this.IsTraceMethodEnabled = true;
                    break;
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

        public void ConsoleWasClosed(bool state)
        {
            this.ConsoleButtonText = Defines.ConsoleButtonNameWhenClosed;
        }
    }
}
