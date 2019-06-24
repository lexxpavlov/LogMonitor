namespace LogMonitor.Monitoring
{
    public class ColumnConfig
    {
        private static int _unnamedColumnIndex;

        public string Header { get; private set; }
        public ColumnTypes Type { get; private set; }
        public string Path { get; private set; }

        public ColumnConfig(string header, ColumnTypes type, string path = null)
        {
            if (string.IsNullOrWhiteSpace(header))
            {
                header = type.ToString() + _unnamedColumnIndex++;
            }
            Header = header;
            Type = type;
            if (string.IsNullOrWhiteSpace(path))
            {
                switch (type)
                {
                    case ColumnTypes.Level:
                    case ColumnTypes.Message:
                    case ColumnTypes.Time:
                        Path = type.ToString();
                        break;
                    default:
                        Path = header;
                        break;
                }
            }
            else
            {
                Path = path;
            }
        }

        public override string ToString()
        {
            return Header;
        }
    }
}
