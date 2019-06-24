using System;
using System.Collections.Generic;
using System.Linq;
using LogMonitor.Filters;

namespace LogMonitor.Monitoring
{
    public class LogLine
    {
        private readonly Dictionary<string, object> _columns = new Dictionary<string, object>();
        private readonly string _timePath = "Time";
        private readonly string _levelPath = "Level";
        private readonly string _messagePath = "Message";
        private readonly LogLine _previousLine;

        public int Number { get; private set; }

        public int HighlightLevel { get; private set; }

        public bool IsHighlighted { get { return HighlightLevel > 0; } }

        public bool IsNextSecond { get { return _previousLine != null && (_previousLine.Time.Second != Time.Second || _previousLine.Time.Minute != Time.Minute); } }

        public DateTime Time
        {
            get
            {
                return _columns.ContainsKey(_timePath) && _columns[_timePath] is DateTime
                    ? (DateTime)_columns[_timePath]
                    : DateTime.MinValue;
            }
        }

        public string TimeString
        {
            get
            {
                return _columns.ContainsKey(_timePath)
                    ? _columns[_timePath].ToString()
                    : string.Empty;
            }
        }

        public string Level
        {
            get
            {
                return _columns.ContainsKey(_levelPath)
                    ? _columns[_levelPath] as string
                    : LevelValues.Info.ToString();
            }
        }

        public string Message
        {
            get
            {
                return _columns.ContainsKey(_messagePath)
                    ? _columns[_messagePath] as string
                    : null;
            }
        }

        public object this[string column]
        {
            get
            {
                return _columns.ContainsKey(column)
                    ? _columns[column]
                    : null;
            }
        }

        public LogLine(int number, ColumnMapping mapping, List<string> columns, LogLine previousLine = null)
        {
            Number = number;
            _previousLine = previousLine;
            int i = 0;
            if (columns.Count < mapping.Columns.Count)
            {
                columns.AddRange(Enumerable.Repeat<string>(null, mapping.Columns.Count - columns.Count));
            }
            foreach (var column in mapping.Columns)
            {
                switch (column.Type)
                {
                    case ColumnTypes.Time:
                        DateTime time;
                        if (DateTime.TryParse(columns[i], out time))
                        {
                            _columns[column.Path] = time;
                        }
                        else
                        {
                            _columns[column.Path] = columns[i];                            
                        }
                        _timePath = column.Path;
                        break;
                    case ColumnTypes.Skip:
                        break;
                    case ColumnTypes.Level:
                        _levelPath = column.Path;
                        goto default;
                    case ColumnTypes.Message:
                        _messagePath = column.Path;
                        goto default;
                    default:
                        _columns[column.Path] = columns[i];
                        break;
                }
                i++;
            }
            HighlightLevel = CheckHighlight(Message);
        }

        public LogLine(int number, string message, LogLine previousLine = null)
        {
            Number = number;
            _previousLine = previousLine;
            _columns[_messagePath] = message;
            HighlightLevel = CheckHighlight(message);
        }

        public bool ContainsKey(string key)
        {
            return _columns.ContainsKey(key);
        }

        public void AppendMessage(string line)
        {
            _columns[_messagePath] += Environment.NewLine + line;
        }

        private int CheckHighlight(string message)
        {
            return message.TakeWhile(ch => ch == '*' || ch == '#').Count();
        }

        public override int GetHashCode()
        {
            return Number ^ TimeString.GetHashCode() ^ Message.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var that = obj as LogLine;
            return that != null && Number == that.Number && TimeString == that.TimeString && Message == that.Message;
        }
    }
}
