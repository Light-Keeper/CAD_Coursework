using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WPF_GUI.Helpers;

namespace WPF_GUI.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel() // Must be deleted in realese
        {
            RouteMethodCollection = new ObservableCollection<string>();
            MapMethodCollection = new ObservableCollection<string>();

            for (var i = 0; i < 5; i++)
            {
                RouteMethodCollection.Add("Метод трассировки " + (i + 1));
                MapMethodCollection.Add("Метод компановки " + (i + 1));
            }
        }

        #region Properties

        #region StartButtonName
        private string _startButtonName;
        public string StartButtonName
        {
            get { return _startButtonName; }
            set {
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
                
                if (_showLog)
                {
                    StaticLoader.Application.LogViewer.Show();
                }
                else
                {
                    StaticLoader.Application.LogViewer.Hide();
                }
                RaisePropertyChanged(() => ShowLog);
            }
        }
        #endregion

        #region ImageZomm
        private int _imageZoom = 100;
        /// <summary>
        /// Image Zoom
        /// Can be 25, 50, 75, 100 (default), 125, 150, 175 or 200
        /// </summary>
        public int ImageZoom
        {
            get { return _imageZoom; }
            set
            {
                if (_imageZoom == value) return;
                _imageZoom = value;
                RaisePropertyChanged(() => ImageZoom);
                StatusBarInfo = "Изменён масштаб изображения на " + ImageZoom;
            }
        }
        #endregion

        #region ImageViewerWidth
        private bool _imageViewerWidth;
        public bool ImageViewerWidth
        {
            get { return _imageViewerWidth; }
            set
            {
                if (_imageViewerWidth == value) return;
                _imageViewerWidth = value;
                RaisePropertyChanged(() => ImageViewerWidth);
            }
        }
        #endregion

        #region ImageViewerHeight
        private bool _imageViewerHeight;
        public bool ImageViewerHeight
        {
            get { return _imageViewerHeight; }
            set
            {
                if (_imageViewerHeight == value) return;
                _imageViewerHeight = value;
                RaisePropertyChanged(() => ImageViewerHeight);
            }
        }
        #endregion

        #region StatuBarInfo
        private string _statusBarInfo = "Что-то там случилось";
        public string StatusBarInfo
        {
            get { return _statusBarInfo; }
            set
            {
                if (_statusBarInfo == value) return;
                _statusBarInfo = value;
                RaisePropertyChanged(() => StatusBarInfo);
            }
        }
        #endregion

        #endregion

        #region Commands

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
