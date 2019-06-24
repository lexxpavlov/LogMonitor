using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Logging;
using LogMonitor.Configuration;
using LogMonitor.Converters;
using LogMonitor.Filters;
using LogMonitor.Monitoring;
using LogMonitor.Properties;
using LogMonitor.Utils;

namespace LogMonitor
{
    public class MainWindowVm : BaseViewModel
    {
        #region properties

        private readonly Monitor _monitor;
        private readonly ObservableCollection<LogFile> _savedFiles;
        private readonly CollectionViewSource _sourceView = new CollectionViewSource();
        private readonly SelectFileWindowVm _selectFileWindowVm;
        private LogFile _logFile;
        private string _fileName;
        private bool _fileExisted;
        private bool? _showYear = false;
        private bool _isScrolledToEnd;

        public Monitor Monitor
        {
            get { return _monitor; }
        }

        public ObservableCollection<LogFile> SavedFiles
        {
            get { return _savedFiles; }
        }
        
        public string FileName
        {
            get { return _fileName; }
            private set
            {
                if (value == _fileName) return;
                _fileName = value;
                RaisePropertyChanged();
                RunCommand.RaiseCanExecuteChanged();
            }
        }

        public ColumnMapping CurrentColumnMapping
        {
            get; private set;
        }

        public List<LogLine> Lines
        {
            get; private set;
        }

        public CollectionViewSource SourceView
        {
            get { return _sourceView; }
        }

        public bool IsRun
        {
            get { return _monitor.IsRun; }
        }

        public double WindowWidth { get; private set; }

        public double WindowHeight { get; private set; }

        public bool? ShowYear
        {
            get { return _showYear; }
            set
            {
                _showYear = value;
                var timeHeader = CurrentColumnMapping.Columns.FirstOrDefault(col => col.Type == ColumnTypes.Time);
                var column = timeHeader != null 
                    ? (DataGridTextColumn)LogGrid.Columns.First(col => timeHeader.Header.Equals(col.Header))
                    : (DataGridTextColumn)LogGrid.Columns[0];
                column.Binding = new Binding
                {
                    Converter = new LogItemConverter(),
                    ConverterParameter = "Time",
                    StringFormat = value == true ? "yyyy-MM-dd HH:mm:ss" : "HH:mm:ss",
                };
                column.Width = 0;
                column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Auto);
            }
        }

        public DataGrid LogGrid { get; set; }

        public string Title
        {
            get
            {
                if (!_fileExisted) return string.Format(Resources.MainWindow_Title_FileNotExisted, FileName);
                var count = Lines.Count;
                var shownCount = _sourceView.View.Cast<object>().Count();
                var format = count == shownCount ? Resources.MainWindow_Title : Resources.MainWindow_Title_Filtered;
                return string.Format(format, FileName, count, shownCount);
            }
        }

        public bool IsScrolledToEnd
        {
            get { return _isScrolledToEnd; }
            set
            {
                if (value.Equals(_isScrolledToEnd)) return;
                _isScrolledToEnd = value;
                RaisePropertyChanged();
            }
        }

        public double ScrolledToEndButtonFontSize
        {
            get { return SystemParameters.HorizontalScrollBarHeight - 2; }
        }

        #region filters

        private FilterValue<FilterTypes, LevelValues, bool> _filterLevel;
        private FilterValue<FilterTypes, int, bool> _filterHighlighted;
        private readonly Dictionary<LevelValues, bool> _filterLevels = new Dictionary<LevelValues, bool>();
        private List<string> _filterLevelsCurrent = new List<string>();
        private int _filterHighlight = -1;
        private string _filterText;
        private bool _caseSensitive;

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (value == _filterText) return;
                _filterText = value;
                RaisePropertyChanged();
                Refresh();
            }
        }

        public bool CaseSensitive
        {
            get { return _caseSensitive; }
            set
            {
                if (value.Equals(_caseSensitive)) return;
                _caseSensitive = value;
                RaisePropertyChanged();
                Refresh();
            }
        }

        public FilterValue<FilterTypes, LevelValues, bool> FilterLevel
        {
            get { return _filterLevel; }
            set
            {
                if (value.Equals(_filterLevel)) return;
                _filterLevel = value;
                Filter(_filterLevel.Type, _filterLevel.Filter, _filterLevel.Value);
                RaisePropertyChanged();
            }
        }

        public FilterValue<FilterTypes, int, bool> FilterHighlighted
        {
            get { return _filterHighlighted; }
            set
            {
                if (value.Equals(_filterHighlighted)) return;
                _filterHighlighted = value;
                Filter(_filterHighlighted.Type, _filterHighlighted.Filter, _filterHighlighted.Value);
                RaisePropertyChanged();
            }
        }

        #endregion

        #endregion

        #region constructor

        public MainWindowVm()
        {
            _savedFiles = new ObservableCollection<LogFile>(Config.SavedLogs);
            Lines = new List<LogLine>();
            _monitor = new Monitor();
            _selectFileWindowVm = new SelectFileWindowVm(this);

            _sourceView.Source = Lines;
            _sourceView.Filter += SourceView_OnFilter;

            // ReSharper disable once ExplicitCallerInfoArgument
            _monitor.RunEvent += (sender, isRun) => RaisePropertyChanged("IsRun");
            _monitor.UpdateLogEvent += Monitor_UpdateLogEvent;

            WindowWidth = Config.WindowWidth;
            WindowHeight = Config.WindowHeight;
            Logger.Settings.Enabled = Config.LogEnabled;
        }

        #endregion

        #region events and handlers

        public event EventHandler<ColumnMapping> ColumnMappingChanged; 
        
        private void Monitor_UpdateLogEvent(object sender, UpdateLogEventArgs args)
        {
            var isScrolledToEnd = IsScrolledToEnd;
            if (args.IsClear || args.FileExistanceChanged == false)
            {
                Lines.Clear();
            }
            if (args.AddedItems != null)
            {
                Lines.AddRange(args.AddedItems);
            }
            if (args.FileExistanceChanged != null && _fileExisted != args.FileExistanceChanged)
            {
                _fileExisted = args.FileExistanceChanged.Value;
                if (!IsRun && _logFile.AutoStart) Run();
            }
            Refresh();
            IsScrolledToEnd = isScrolledToEnd;
        }

        #endregion

        #region public methods

        public void Initialize()
        {
            InitializeFilters(Config.FilterLevels, Config.FilterHighlight);
            if (_savedFiles != null && _savedFiles.Count > 0)
            {
                var logFile = _savedFiles.FirstOrDefault(log => log.FileName == Config.RunLogFile) ?? _savedFiles.First();
                SetFileName(logFile);
            }

            if (Config.RunImmediately)
            {
                Run();
            }
        }

        public void Run()
        {
            if (CanRun())
            {
                _monitor.Run();
            }
        }

        public void Stop()
        {
            _monitor.Stop();
        }

        public void Clear()
        {
            _monitor.Clear();
        }

        public void ReloadLog()
        {
            _monitor.Reload();
        }

        public void SetFileName(LogFile logFile)
        {
            CurrentColumnMapping = logFile.Mapping ?? Config.DefaultColumnMapping;
            if (ColumnMappingChanged != null)
            {
                ColumnMappingChanged(this, CurrentColumnMapping);
            }
            _logFile = logFile;
            FileName = logFile.FileName;
            _fileExisted = true;
            _monitor.Initialize(FileName, CurrentColumnMapping);

            InitializeFilters(logFile.FilterLevels, logFile.FilterHighlight);

            if (logFile.AutoStart)
            {
                Run();
            }
        }

        #endregion

        #region private methods

        private void InitializeFilters(List<LevelValues> levels, int highlight, string filterText = null)
        {
            var allTrue = levels.Count == 0;
            foreach (LevelValues level in Enum.GetValues(typeof(LevelValues)))
            {
                SetFilter(FilterTypes.Level, level, allTrue || levels.Contains(level));
            }
            SetFilter(FilterTypes.Highlight, highlight, true);
            FilterText = filterText;
        }

        private bool CanRun()
        {
            return !string.IsNullOrWhiteSpace(FileName);
        }

        private void Refresh()
        {
            DispatcherHelper.Invoke(() => _sourceView.View.Refresh());
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged("Title");
        }

        private void BrowseLog()
        {
            var wnd = new SelectFileWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = _selectFileWindowVm,
            };
            wnd.ShowDialog();
        }

        private void ShowFileNameInExternalEditor()
        {
            System.Diagnostics.Process.Start(FileName);
        }

        private bool CanShowFileNameInExternalEditor()
        {
            return !string.IsNullOrWhiteSpace(FileName);
        }

        #endregion

        #region filters

        private void SourceView_OnFilter(object sender, FilterEventArgs args)
        {
            var line = (LogLine)args.Item;
            var comparison = CaseSensitive
                ? StringComparison.CurrentCulture
                : StringComparison.CurrentCultureIgnoreCase;

            var filterLevel = _filterLevelsCurrent.Any(level => String.Equals(level, line.Level, StringComparison.InvariantCultureIgnoreCase));
            var filterHighlight = _filterHighlight <= 0 || line.HighlightLevel >= _filterHighlight;
            var filterText = string.IsNullOrWhiteSpace(FilterText) || line.Message.IndexOf(FilterText, comparison) >= 0;
            
            args.Accepted = filterLevel && filterHighlight && filterText;
        }

        private void Filter(FilterTypes type, object filter, bool value)
        {
            switch (type)
            {
                case FilterTypes.Level:
                    var level = (LevelValues)filter;
                    _filterLevels[level] = value;
                    _filterLevelsCurrent = _filterLevels
                        .Where(lvl => lvl.Value)
                        .Select(lvl => lvl.Key.ToString())
                        .ToList();
                    break;
                case FilterTypes.Highlight:
                    if (value)
                    {
                        _filterHighlight = Convert.ToInt32(filter);
                    }
                    break;
            }
            Refresh();
        }

        private void SetFilter(FilterTypes type, object filter, bool value)
        {
            switch (type)
            {
                case FilterTypes.Level:
                    FilterLevel = new FilterValue<FilterTypes, LevelValues, bool>(type, (LevelValues)filter, value);
                    break;
                case FilterTypes.Highlight:
                    FilterHighlighted = new FilterValue<FilterTypes, int, bool>(type, Convert.ToInt32(filter), value);
                    break;
            }
        }

        #endregion
        
        #region commands

        private DelegateCommand _runCommand;
        private DelegateCommand _stopCommand;
        private DelegateCommand _clearCommand;
        private DelegateCommand _refreshLogCommand;
        private DelegateCommand _browseLogCommand;
        private DelegateCommand _showInExternalEditorCommand;

        public DelegateCommand RunCommand
        {
            get { return _runCommand ?? (_runCommand = new DelegateCommand(Run, CanRun)); }
        }

        public DelegateCommand StopCommand
        {
            get { return _stopCommand ?? (_stopCommand = new DelegateCommand(Stop)); }
        }

        public DelegateCommand ClearCommand
        {
            get { return _clearCommand ?? (_clearCommand = new DelegateCommand(Clear)); }
        }

        public DelegateCommand RefreshLogCommand
        {
            get { return _refreshLogCommand ?? (_refreshLogCommand = new DelegateCommand(ReloadLog)); }
        }

        public DelegateCommand BrowseLogCommand
        {
            get { return _browseLogCommand ?? (_browseLogCommand = new DelegateCommand(BrowseLog)); }
        }

        public DelegateCommand ShowInExternalEditorCommand
        {
            get { return _showInExternalEditorCommand ?? (_showInExternalEditorCommand = new DelegateCommand(ShowFileNameInExternalEditor, CanShowFileNameInExternalEditor)); }
        }

        #endregion
    }
}
