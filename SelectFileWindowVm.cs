using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using LogMonitor.Configuration;
using LogMonitor.Monitoring;
using LogMonitor.Utils;

namespace LogMonitor
{
    public class SelectFileWindowVm : BaseViewModel
    {
        private readonly MainWindowVm _mainWindowVm;
        private readonly List<ColumnMapping> _mappings = Config.ColumnMappings;
        private readonly ObservableCollection<LogFile> _logFiles = new ObservableCollection<LogFile>(Config.SavedLogs);

        public List<ColumnMapping> Mappings { get { return _mappings; } }
        public ObservableCollection<LogFile> Files { get { return _logFiles; } }

        public ColumnMapping SelectedMapping { get; set; }

        public LogFile SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                _selectedFile = value;
                RaisePropertyChanged();
            }
        }

        #region ctor

        public SelectFileWindowVm()
        {
            SelectedMapping = Config.ColumnMappings.FirstOrDefault();
            SelectedFile = Files.FirstOrDefault();
        }

        public SelectFileWindowVm(MainWindowVm mainWindowVm)
            : this()
        {
            _mainWindowVm = mainWindowVm;
            mainWindowVm.SavedFiles.Except(_logFiles).ForEach(_logFiles.Add);
        }

        #endregion

        #region methods

        private void SelectCurrentFile()
        {
            _mainWindowVm.SavedFiles.Add(SelectedFile);
            _mainWindowVm.SetFileName(SelectedFile);
            RaiseCloseRequest();
        }

        private void BrowseFile()
        {
            var browser = new OpenFileDialog
            {
                DefaultExt = ".log",
                Filter = "Log files|*.log;*.txt|All files|*.*"
            };
            if (browser.ShowDialog() == DialogResult.OK)
            {
                AddFile(browser.FileName);
            }
        }

        private void AddFile(string file)
        {
            var logFile = new LogFile(file, SelectedMapping);
            Files.Add(logFile);
            SelectedFile = logFile;
        }

        #endregion

        #region commands

        private DelegateCommand _selectCommand;
        private DelegateCommand _browseCommand;
        private LogFile _selectedFile;

        public DelegateCommand SelectCommand
        {
            get { return _selectCommand ?? (_selectCommand = new DelegateCommand(SelectCurrentFile)); }
        }

        public DelegateCommand BrowseCommand
        {
            get { return _browseCommand ?? (_browseCommand = new DelegateCommand(BrowseFile)); }
        }

        #endregion

        #region events

        public event EventHandler WindowCloseRequest;

        private void RaiseCloseRequest()
        {
            if (WindowCloseRequest != null)
            {
                WindowCloseRequest(this, new EventArgs());
            }
        }

        #endregion

    }
}
