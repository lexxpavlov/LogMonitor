using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shell;
using Logging;
using LogMonitor.Configuration.Elements;
using LogMonitor.Filters;
using LogMonitor.Monitoring;
using LogMonitor.Properties;
using LogMonitor.Utils;

namespace LogMonitor.Configuration
{
    internal static class Config
    {
        #region Variables

        public static readonly List<ColumnMapping> ColumnMappings;
        public static readonly ColumnMapping DefaultColumnMapping;
        public static readonly Dictionary<LevelValues, Brush> LevelColors;
        public static readonly List<LogFile> SavedLogs;

        public static readonly int WindowWidth;
        public static readonly int WindowHeight;
        public static readonly int WindowMonitor;
        public static readonly WindowConfig.WindowPosition WindowPosition;

        public static readonly List<LevelValues> FilterLevels;
        public static readonly int FilterHighlight;
        public static readonly bool RunImmediately;
        public static readonly string RunLogFile;
        public static readonly string Language;
        public static readonly bool LogEnabled;

        private enum AppArgs { Run, Levels, Highlight, Mapping, Lang, Log }

        #endregion Variables

        #region Constructor

        static Config()
        {
            var section = MonitorSection.GetSection();
            
            ColumnMappings = ReadColumnMappings(section);
            LevelColors = ReadLevelColors(section);
            SavedLogs = ReadSavedLogs(section);

            var window = ReadWindowProperties(section);
            WindowWidth = window.Width;
            WindowHeight = window.Height;
            WindowMonitor = window.Monitor;
            WindowPosition = window.Position;

            var args = AppArguments.FromCommandLine();
            DefaultColumnMapping = GetMapping(args.GetOption(AppArgs.Mapping));
            FilterLevels = ParseFilterLevels(args.GetOption(AppArgs.Levels));
            FilterHighlight = args.GetOptionInteger(AppArgs.Highlight);
            RunImmediately = args.HasOption(AppArgs.Run);
            RunLogFile = args.GetCommand(1);
            Language = args.GetOption(AppArgs.Lang, CultureInfo.CurrentCulture.Name);
            LogEnabled = new[] { "1", "true", "on" }.Contains(args.GetOption(AppArgs.Log, "1").ToLower());

            if (FilterLevels == null)
            {
                FilterLevels = Enum.GetValues(typeof(LevelValues)).Cast<LevelValues>().ToList();
            }
            if (SavedLogs != null && !string.IsNullOrWhiteSpace(RunLogFile) && SavedLogs.All(log => log.FileName != RunLogFile))
            {
                SavedLogs.Insert(0, new LogFile(RunLogFile, DefaultColumnMapping, false));
            }

            Initialize();
        }

        #endregion Constructor

        #region Initialize

        private static void Initialize()
        {
            SelectCulture();
            UpdateJumpList();
        }

        private static void SelectCulture()
        {
            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Language);
            }
            catch (CultureNotFoundException)
            {
                Logger.Warn("Incorrect localization culture \"{0}\". The \"{1}\" culture used", Language,
                    CultureInfo.CurrentUICulture.Name);
            }
        }

        private static void UpdateJumpList()
        {
            var jumpList = JumpList.GetJumpList(Application.Current);
            if (jumpList == null)
            {
                return;
            }
            jumpList.JumpItems.Clear();
            if (SavedLogs != null)
            {
                var applicationPath = Environment.GetCommandLineArgs()[0];
                foreach (var log in SavedLogs.Select(log => log.FileName))
                {
                    jumpList.JumpItems.Add(new JumpTask
                    {
                        Title = log,
                        ApplicationPath = applicationPath,
                        Arguments = '"' + log + '"',
                        CustomCategory = Resources.SavedLogsTitle,
                    });
                }
            }
            jumpList.Apply();
        }

        #endregion Initialize

        #region Methods

        private static ColumnMapping GetMapping(string mappingName)
        {
            if (string.IsNullOrWhiteSpace(mappingName))
            {
                return ColumnMappings.FirstOrDefault(m => m.IsDefault)
                    ?? ColumnMappings.FirstOrDefault();
            }
            return ColumnMappings.FirstOrDefault(m => m.Name == mappingName);
        }

        private static List<LevelValues> ParseFilterLevels(string values)
        {
            if (string.IsNullOrWhiteSpace(values))
            {
                return null;
            }
            try
            {
                return values.Split(',')
                    .Select(l => (LevelValues)Enum.Parse(typeof(LevelValues), l, true))
                    .ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion Methods

        #region Read values

        private static List<LogFile> ReadSavedLogs(MonitorSection section)
        {
            return section != null && section.SavedLogs != null
                ? (section.SavedLogs.Cast<LogConfigElement>()
                    .Select(log => new LogFile(log.Log, GetMapping(log.Mapping), log.AutoStart, ParseFilterLevels(log.FilterLevels), log.FilterHighlight, log.FilterText)))
                    .ToList()
                : new List<LogFile>
                {
                    //new LogFile("test file", DefaultColumnMapping)
                };
        }

        private static WindowConfig ReadWindowProperties(MonitorSection section)
        {
            var width = 1440;
            var height = 350;
            var monitor = 0;
            var position = WindowConfig.WindowPosition.Center;
            if (section != null && section.Window != null)
            {
                width = section.Window.Width > 0 ? section.Window.Width : width;
                height = section.Window.Height > 0 ? section.Window.Height : height;
                monitor = section.Window.Monitor >= 0 ? section.Window.Monitor : 0;
                position = section.Window.Position;
            }
            return new WindowConfig(width, height, monitor, position);
        }
        
        private static List<ColumnMapping> ReadColumnMappings(MonitorSection section)
        {
            var result = new List<ColumnMapping>();
            if (section == null || section.ColumnMappings == null)
            {
                result.Add(new ColumnMapping("default", new List<ColumnConfig>
                {
                    new ColumnConfig("Time", ColumnTypes.Time),
                    new ColumnConfig("Level", ColumnTypes.Level),
                    new ColumnConfig("Cite", ColumnTypes.Text),
                    new ColumnConfig("Message", ColumnTypes.Message),
                }, "|", isDefault: true));
            }
            else
            {
                foreach (var mapping in section.ColumnMappings.Cast<ColumnMappingConfigElement>().ToList())
                {
                    var columns = mapping.Columns.Cast<ColumnConfigElement>().ToList()
                        .ConvertAll(element => new ColumnConfig(element.Header, element.Type, element.Path));
                    result.Add(new ColumnMapping(mapping.Name, columns, mapping.Separator, mapping.Pattern, mapping.IsDefault, mapping.IgnoreEmptyLines));
                }
            }
            return result;
        }

        private static Dictionary<LevelValues, Brush> ReadLevelColors(MonitorSection section)
        {
            var dictionary = new Dictionary<LevelValues, Brush>
            {
                { LevelValues.Debug, Brushes.Transparent },
                { LevelValues.Info, Brushes.GreenYellow },
                { LevelValues.Warn, Brushes.DarkOrange },
                { LevelValues.Error, Brushes.Red },
                { LevelValues.Fatal, Brushes.BlueViolet },
            };
            if (section == null || section.LevelColors == null)
            {
                return dictionary;
            }
            foreach (var element in section.LevelColors.Cast<LevelColorElement>())
            {
                var level = (LevelValues)Enum.Parse(typeof(LevelValues), element.Level, true);
                Brush brush;
                try
                {
                    brush = (SolidColorBrush)new BrushConverter().ConvertFromString(element.Color);
                }
                catch (FormatException)
                {
                    brush = Brushes.Transparent;
                }
                dictionary[level] = brush;
            }
            return dictionary;
        }

        #endregion Read values
    }
}
