using System.Collections.Generic;
using LogMonitor.Filters;

namespace LogMonitor.Monitoring
{
    public class LogFile
    {
        public string FileName { get; private set; }
        
        public ColumnMapping Mapping { get; private set; }
        
        public bool AutoStart { get; private set; }

        public List<LevelValues> FilterLevels { get; private set; }

        public int FilterHighlight { get; private set; }

        public string FilterText { get; private set; }

        public LogFile(string fileName, ColumnMapping mapping, bool autoStart = false, List<LevelValues> filterLevels = null, int filterHighlight = 0, string filterText = null)
        {
            FileName = fileName;
            Mapping = mapping;
            AutoStart = autoStart;
            FilterLevels = filterLevels ?? new List<LevelValues>();
            FilterHighlight = filterHighlight;
            FilterText = filterText;
        }
    }
}
