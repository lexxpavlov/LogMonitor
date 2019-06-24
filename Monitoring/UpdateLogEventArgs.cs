using System;
using System.Collections.Generic;

namespace LogMonitor.Monitoring
{
    public class UpdateLogEventArgs : EventArgs
    {
        public bool? FileExistanceChanged { get; private set; }
        public bool IsClear { get; private set; }
        public List<LogLine> AddedItems { get; private set; }

        public UpdateLogEventArgs(bool isClear, List<LogLine> addedItems = null)
        {
            IsClear = isClear;
            AddedItems = addedItems;
        }

        public UpdateLogEventArgs(bool isExist)
        {
            FileExistanceChanged = isExist;
        }
    }
}
