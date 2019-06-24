using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Logging;
using LogMonitor.Monitoring.ItemsProvider;

namespace LogMonitor.Monitoring
{
    public class Monitor
    {
        #region properties

        private readonly FileSystemWatcher _fileWatcher = new FileSystemWatcher();
        private readonly List<LogLine> _lines = new List<LogLine>();
        private IItemsProvider _itemsProvider;
        private bool _isRun;
        private bool _isCurrentFile;

        public event EventHandler<bool> RunEvent;
        public event EventHandler<UpdateLogEventArgs> UpdateLogEvent;

        public string FileName { get; private set; }
        public ColumnMapping Mapping { get; private set; }

        public bool IsRun
        {
            get { return _isRun; }
            private set
            {
                if (value.Equals(_isRun)) return;
                _isRun = value;
                _isCurrentFile = value;

                _fileWatcher.EnableRaisingEvents = value;

                if (RunEvent != null) RunEvent(this, value);
            }
        }

        public List<LogLine> Lines
        {
            get { return _lines; }
        }

        #endregion

        #region ctor

        public Monitor()
        {
            _fileWatcher.Created += OnLogCreated;
            _fileWatcher.Deleted += OnLogDeleted;
            _fileWatcher.Renamed += OnLogRenamed;
        }

        #endregion

        #region public methods

        public void Initialize(string filename, ColumnMapping mapping)
        {
            FileName = filename;
            Mapping = mapping;
            if (string.IsNullOrWhiteSpace(mapping.Pattern))
            {
                _itemsProvider = new SplitBySeparator(mapping.Separator, mapping.Columns.Count);
            }
            else
            {
                _itemsProvider = new SplitByPattern(Mapping.Pattern, mapping.Columns.Count);                
            }
            if (File.Exists(FileName))
            {
                Reload();
            }
            _fileWatcher.Path = Path.GetDirectoryName(FileName);
            _fileWatcher.Filter = Path.GetFileName(FileName);
        }

        public bool Run()
        {
            if (!File.Exists(FileName))
            {
                UpdateFileState(false);
                return false;
            }
            try
            {
                _fileWatcher.Changed += OnLogChanged;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }

            Reload();
            IsRun = true;
            return IsRun;
        }

        public void Stop()
        {
            IsRun = false;
            _fileWatcher.Changed -= OnLogChanged;
        }

        public void Clear()
        {
            try
            {
                File.WriteAllText(FileName, "");
                if (!IsRun)
                {
                    Lines.Clear();
                }
                RaiseUpdateLogEvent(true, null);
            }
            catch (IOException ex)
            {
                Logger.Error(ex.Message);
            }
        }

        public void Reload()
        {
            UpdateLog(GetLines());
        }

        #endregion

        #region private methods

        private void UpdateLog(List<LogLine> lines)
        {
            var isClear = false;
            var linesCount = Lines.Count;
            if (linesCount > lines.Count || linesCount > 0 && !Lines.Last().Equals(lines[linesCount - 1]))
            {
                Lines.Clear();
                linesCount = 0;
                isClear = true;
            }
            var updatedItems = lines.GetRange(linesCount, lines.Count - linesCount);
            Lines.AddRange(updatedItems);

            RaiseUpdateLogEvent(isClear, updatedItems);
        }

        private void RaiseUpdateLogEvent(bool isClear, List<LogLine> addedItems)
        {
            if (UpdateLogEvent != null)
            {
                UpdateLogEvent(this, new UpdateLogEventArgs(isClear, addedItems));
            }
        }

        private void RaiseUpdateLogEvent(bool isExist)
        {
            if (UpdateLogEvent != null)
            {
                UpdateLogEvent(this, new UpdateLogEventArgs(isExist));
            }
        }

        private List<LogLine> GetLines()
        {
            var result = new List<LogLine>();

            string[] lines;
            try
            {
                using (var fileStream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.GetEncoding(1251)))
                    {
                        var log = streamReader.ReadToEnd();
                        lines = log.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
                    }
                }
            }
            catch (SystemException ex)
            {
                Logger.Error(ex.Message);
                return result;
            }

            LogLine previousLine = null;
            int lastIndex = lines.Length - 1;
            var columnsCount = Mapping.Columns.Count;
            
            for (int i = 0; i <= lastIndex; i++)
            {
                var line = lines[i];
                if (i == lastIndex && string.IsNullOrEmpty(line))
                {
                    break;
                }
                var items = _itemsProvider.GetItems(line);

                if (items.Count == columnsCount)
                {
                    previousLine = new LogLine(i, Mapping, items, previousLine);
                    result.Add(previousLine);
                }
                else
                {
                    if (previousLine != null)
                    {
                        if (!Mapping.IgnoreEmptyLines || !string.IsNullOrEmpty(line))
                        {
                            previousLine.AppendMessage(line);
                        }
                    }
                    else
                    {
                        previousLine = new LogLine(i, line);
                        result.Add(previousLine);
                    }
                }
            }

            return result;
        }

        private void UpdateFileState(bool isExisted)
        {
            if (isExisted)
            {
                Reload();
                _fileWatcher.Filter = Path.GetFileName(FileName);
            }
            else
            {
                if (IsRun) Stop();
                _fileWatcher.Filter = "";
                _fileWatcher.EnableRaisingEvents = true;
            }
            RaiseUpdateLogEvent(isExisted);
        }

        #endregion

        #region event handlers

        private void OnLogChanged(object sender, FileSystemEventArgs args)
        {
            if (_isCurrentFile)
            {
                Reload();
            }
        }

        private void OnLogDeleted(object sender, FileSystemEventArgs args)
        {
            UpdateFileState(false);
        }

        private void OnLogCreated(object sender, FileSystemEventArgs args)
        {
            UpdateFileState(true);
        }

        private void OnLogRenamed(object sender, RenamedEventArgs args)
        {
            if (args.FullPath == FileName)
            {
                UpdateFileState(true);
            }
            if (args.OldFullPath == FileName)
            {
                UpdateFileState(false);
            }
        }

        #endregion
    }
}
