using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Logging;
using LogMonitor.Configuration;
using LogMonitor.Monitoring;
using LogMonitor.Utils;

namespace LogMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVm _mainWindowVm;
        private ScrollViewer _scrollViewer;

        public MainWindow()
        {
            UpdateWindowPosition(this);
            InitializeComponent();

            _mainWindowVm = (MainWindowVm)DataContext;
            _mainWindowVm.LogGrid = LogGrid;

            BuildColumns(Config.DefaultColumnMapping);

            _mainWindowVm.ColumnMappingChanged += MainWindowVmColumnMappingChanged;
            _mainWindowVm.Monitor.UpdateLogEvent += Monitor_OnUpdateLog;

            _mainWindowVm.Initialize();

            //new LoggerTesting().RunAll();
        }

        private void UpdateWindowPosition(MainWindow mainWindow)
        {
            var startupLocation = Config.WindowMonitor == 0 ? WindowStartupLocation.CenterScreen : WindowStartupLocation.Manual;
            mainWindow.WindowStartupLocation = startupLocation;

            if (Config.WindowMonitor > 0)
            {
                var screens = System.Windows.Forms.Screen.AllScreens;
                if (Config.WindowMonitor <= screens.Length)
                {
                    var screen = screens[Config.WindowMonitor - 1];
                    var workingArea = screen.WorkingArea;
                    var left = workingArea.Left + (screen.Bounds.Width - Config.WindowWidth) / 2;
                    var top = workingArea.Top;
                    switch (Config.WindowPosition)
                    {
                        case WindowConfig.WindowPosition.Center:
                            top += (screen.Bounds.Height - Config.WindowHeight) / 2;
                            break;
                        case WindowConfig.WindowPosition.Bottom:
                            top += screen.Bounds.Height - Config.WindowHeight;
                            break;
                    }
                    mainWindow.Left = left;
                    mainWindow.Top = top;
                }
            }
        }

        private void BuildColumns(ColumnMapping columnMapping)
        {
            var builder = new GridColumnBuilder(Resources);
            LogGrid.Columns.Clear();
            foreach (var column in columnMapping.Columns.Where(col => col.Type != ColumnTypes.Skip))
            {
                LogGrid.Columns.Add(builder.Add(column));
            }
        }

        private void ScrollToEnd(bool scroll)
        {
            if (scroll)
            {
                DispatcherHelper.Invoke(_scrollViewer.ScrollToEnd);
                _mainWindowVm.IsScrolledToEnd = true;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = (ScrollViewer)((Decorator)VisualTreeHelper.GetChild(LogGrid, 0)).Child;
            _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs scrollChangedEventArgs)
        {
            var bottomScrollOffset = _scrollViewer.ScrollableHeight - _scrollViewer.VerticalOffset;
            _mainWindowVm.IsScrolledToEnd = bottomScrollOffset <= 1;
        }

        private void MainWindowVmColumnMappingChanged(object sender, ColumnMapping columnMapping)
        {
            BuildColumns(columnMapping);
        }

        private void Monitor_OnUpdateLog(object sender, UpdateLogEventArgs e)
        {
            var isScrolledToEnd = _mainWindowVm.IsScrolledToEnd;
            DispatcherHelper.BeginInvoke(() => ScrollToEnd(isScrolledToEnd));
        }

        private void ScrollToEndButton_OnClick(object sender, RoutedEventArgs e)
        {
            ScrollToEnd(true);
        }

        private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_mainWindowVm != null && !_mainWindowVm.IsRun)
            {
                if (e.AddedItems.Count > 0)
                {
                    var item = (LogFile)e.AddedItems[0];
                    _mainWindowVm.SetFileName(item);
                }
            }
        }
    }
}
