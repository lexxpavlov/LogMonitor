using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LogMonitor.Converters;
using LogMonitor.Monitoring;

namespace LogMonitor.Utils
{
    internal class GridColumnBuilder
    {
        private readonly ResourceDictionary _resources;

        public GridColumnBuilder(ResourceDictionary resources)
        {
            _resources = resources;
        }

        public DataGridColumn Add(ColumnConfig columnConfig)
        {
            return Add(columnConfig.Header, columnConfig.Type, columnConfig.Path);
        }

        public DataGridColumn Add(string header, ColumnTypes type, string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = header;
            }
            switch (type)
            {
                case ColumnTypes.Level:
                    return AddText(header, "Level", "LevelStyle");
                case ColumnTypes.Time:
                    return AddTime(header, "Time");
                case ColumnTypes.Message:
                    return AddText(header, "Message");
                default:
                    return AddText(header, path);
            }
        }

        public DataGridColumn AddText(string header, string path, string style = null)
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding() { Converter = new LogItemConverter(), ConverterParameter = path},
            };
            if (!string.IsNullOrWhiteSpace(style))
            {
                column.ElementStyle = (Style)_resources[style];
            }
            return column;
        }

        public DataGridColumn AddTime(string header, string path, string style = null)
        {
            var column = new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding { Converter = new LogItemConverter(), ConverterParameter = path, StringFormat = "HH:mm:ss" },
                HeaderTemplate = (DataTemplate)_resources["DateTimeColumnHeaderTemplate"],
                CellStyle = (Style)_resources["DateTimeColumnCellStyle"],
            };
            if (!string.IsNullOrWhiteSpace(style))
            {
                column.ElementStyle = (Style)_resources[style];
            }
            return column;
        }
    }
}
