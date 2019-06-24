/*
 * Logger - the log writing tool
 * 
 * Version: 1.0.0
 * Author:  Alexey Pavlov <lexx.pavlov@gmail.com>
 * License: MIT
 * 
 * Usage:
 *   Logger.Debug("Some log message");
 *   Logger.Info("Log {0} message {1}", i, message);
 *   Logger.Debug("* Some log message"); // "*" for use highlighted filter 
 *   
 *   var logs = new List<string> { "first log", "second log" };
 *   Logger.GetLogger().Multiple(logs, Logger.Levels.Debug);
 *   
 *   Logger.SetLogger(new RotationLogger());
 *   var settings = new RotationLogger.SettingsHolder
 *   {
 *       RotationBy = RotationLogger.SettingsHolder.RotationByValues.Length,
 *   };
 *   Logger.SetLogger(new RotationLogger(settings));
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Logging
{
    /// <summary>
    /// Logger class
    /// </summary>
    public static class Logger
    {
        #region singleton

        private static AbstractLogger _instance;

        private static AbstractLogger Instance
        {
            get { return _instance ?? (_instance = new DefaultLogger()); }
        }

        /// <summary>
        /// Settings of logger
        /// </summary>
        public static AbstractLogger.SettingsHolder Settings { get { return Instance.Settings; } }

        /// <summary>
        /// Get logger
        /// </summary>
        public static AbstractLogger GetLogger()
        {
            return Instance;
        }

        /// <summary>
        /// Get logger
        /// </summary>
        public static T GetLogger<T>() where T : AbstractLogger
        {
            return (T)Instance;
        }
        
        /// <summary>
        /// Set logger
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <exception cref="ArgumentNullException">Throws if specified logger is null</exception>
        public static void SetLogger(AbstractLogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            _instance = logger;
        }

        #endregion

        #region public

        /// <summary>
        /// Enumeration of log levels
        /// </summary>
        public enum Levels { Debug, Info, Warn, Error, Fatal }

        /// <summary>
        /// Log message with Debug level. Writes in DEBUG mode only
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public static void Debug(string message, params object[] args)
        {
#if DEBUG
            Instance.LogSkipFrames(Levels.Debug, 1, message, args);
#endif
        }

        /// <summary>
        /// Log message with Info level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public static void Info(string message, params object[] args)
        {
            Instance.Log(Levels.Info, message, args);
        }

        /// <summary>
        /// Log message with Warn level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public static void Warn(string message, params object[] args)
        {
            Instance.Log(Levels.Warn, message, args);
        }

        /// <summary>
        /// Log message with Error level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public static void Error(string message, params object[] args)
        {
            Instance.Log(Levels.Error, message, args);
        }

        /// <summary>
        /// Log message with Fatal level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public static void Fatal(string message, params object[] args)
        {
            Instance.Log(Levels.Fatal, message, args);
        }
        
        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="level">Level of log</param>
        public static void LogException(Exception exception, Levels level = Levels.Error)
        {
            var method = exception.TargetSite;
            var methodName = method.ReflectedType.FullName + "." + method.Name;
            Instance.Log(level, Settings.ExceptionLogMessage, new object[]
            {
                exception.GetType().Name, 
                exception.Message, 
                methodName, 
                exception.StackTrace
            });
        }

        /// <summary>
        /// Get caller method of stack frame
        /// </summary>
        /// <param name="skipFrames">Skip frames of stack</param>
        public static string GetCaller(int skipFrames = 1)
        {
            var method = new StackFrame(skipFrames + 1).GetMethod();
            return method.ReflectedType.FullName + "." + method.Name;
        }

        /// <summary>
        /// Get list of caller methods of stack trace
        /// </summary>
        /// <param name="frames">Frames of stack</param>
        public static string GetCallerStack(int frames = 5)
        {
            var trace = new StackTrace(1);
            var methods = new string[frames];
            for (int i = 0; i < frames; i++)
            {
                var method = trace.GetFrame(i).GetMethod();
                methods[i] = string.Format("\n  at {0}.{1}", method.ReflectedType.FullName, method.Name);
            }
            return string.Join("", methods);
        }

        #endregion
    }

    /// <summary>
    /// Logger class
    /// </summary>
    public abstract class AbstractLogger
    {
        #region settings

        /// <summary>
        /// Settings of logger
        /// </summary>
        public SettingsHolder Settings { get; protected set; }

        /// <summary>
        /// Base settings class
        /// </summary>
        public class SettingsHolder
        {
            public enum LevelCaseModes { UpperCase, LowerCase, UpperCaseFirstLetter }

            /// <summary>
            /// Get additional skip frames
            /// </summary>
            public readonly int AdditionalSkipFrames = 0;

            /// <summary>
            /// Get or set the state of logging
            /// </summary>
            public bool Enabled = true;

            /// <summary>
            /// Get or set case of level string
            /// </summary>
            public LevelCaseModes LevelCase = LevelCaseModes.UpperCase;

            /// <summary>
            /// Get or set async logging
            /// </summary>
            public bool Async = false;

            /// <summary>
            /// Get or set separator of log items
            /// </summary>
            public string Separator = "|";

            /// <summary>
            /// Get or set format of time
            /// </summary>
            public string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

            /// <summary>
            /// Get or set message of exception log. 
            /// Available arguments: 0 = Name, 1 = Message,  2 = full name of target site function, 3 = StackTrace
            /// </summary>
            public string ExceptionLogMessage = "An exception \"{0}\" occured! Message: \"{1}\" Target site: {2}\n{3}";

            /// <summary>
            /// Base SettingsHolder constructor
            /// </summary>
            public SettingsHolder()
            {
            }

            /// <summary>
            ///  Base SettingsHolder constructor
            /// </summary>
            /// <param name="additionalSkipFrames">Additional skip frames</param>
            public SettingsHolder(int additionalSkipFrames)
            {
                if (additionalSkipFrames < 0)
                {
                    const string message = "additionalSkipFrames must be not negative";
                    throw new ArgumentOutOfRangeException("additionalSkipFrames", message);
                }
                AdditionalSkipFrames = additionalSkipFrames;
            }
        }

        #endregion

        #region variables

        protected readonly int DefaultSkipFrames = 2;
        private readonly object _lockingObject = new object();

        #endregion

        #region ctor

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        protected AbstractLogger(SettingsHolder settings)
        {
            Settings = settings;
            DefaultSkipFrames += settings.AdditionalSkipFrames;
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="async">Run logging async</param>
        protected AbstractLogger(bool async = false)
            : this(new SettingsHolder { Async = async })
        {
        }
        
        #endregion

        #region public

        /// <summary>
        /// Log message with Debug level. Writes in DEBUG mode only
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Debug(string message, params object[] args)
        {
#if DEBUG
            LogSkipFrames(Logger.Levels.Debug, 1, message, args);
#endif
        }

        /// <summary>
        /// Log message with Debug level with previous method from stack trace. Writes in DEBUG mode only
        /// </summary>
        /// <param name="skipFrames">Skip stack frames to get the function name</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void DebugSkipFrames(int skipFrames, string message, params object[] args)
        {
#if DEBUG
            LogSkipFrames(Logger.Levels.Debug, 1 + skipFrames, message, args);
#endif
        }

        /// <summary>
        /// Log message with Info level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Info(string message, params object[] args)
        {
            LogSkipFrames(Logger.Levels.Info, 1, message, args);
        }

        /// <summary>
        /// Log message with Warn level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Warn(string message, params object[] args)
        {
            LogSkipFrames(Logger.Levels.Warn, 1, message, args);
        }

        /// <summary>
        /// Log message with Error level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Error(string message, params object[] args)
        {
            LogSkipFrames(Logger.Levels.Error, 1, message, args);
        }

        /// <summary>
        /// Log message with Fatal level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Fatal(string message, params object[] args)
        {
            LogSkipFrames(Logger.Levels.Fatal, 1, message, args);
        }

        /// <summary>
        /// Log many messages
        /// </summary>
        /// <param name="messages">Messages (list or dictionary)</param>
        public virtual void Multiple(IEnumerable<KeyValuePair<string, Logger.Levels>> messages)
        {
            if (!Settings.Enabled) return;
            var logs = messages.Select(m => BuildLogString(m.Value, 3 + DefaultSkipFrames, m.Key, null));
            WriteLog(string.Join(Environment.NewLine, logs));
        }

        /// <summary>
        /// Log many messages with selected level
        /// </summary>
        /// <param name="messages">List of messages</param>
        /// <param name="level">Level of log</param>
        public virtual void Multiple(IEnumerable<string> messages, Logger.Levels level = Logger.Levels.Debug)
        {
            if (!Settings.Enabled) return;
            var logs = messages.Select(log => BuildLogString(level, 3 + DefaultSkipFrames, log, null));
            WriteLog(string.Join(Environment.NewLine, logs));
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="level">Level of log</param>
        public virtual void LogException(Exception exception, Logger.Levels level = Logger.Levels.Error)
        {
            Log(level, Settings.ExceptionLogMessage, new object[]
            {
                exception.GetType().Name, 
                exception.Message, 
                GetMethodName(exception.TargetSite), 
                exception.StackTrace
            });
        }

        /// <summary>
        /// Log message with selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void Log(Logger.Levels level, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            WriteLog(BuildLogString(level, DefaultSkipFrames, message, args));
        }

        /// <summary>
        /// Log message with skip frames and selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="skipFrames">Additional skip frames of stack</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public virtual void LogSkipFrames(Logger.Levels level, int skipFrames, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            WriteLog(BuildLogString(level, skipFrames + DefaultSkipFrames, message, args));
        }

        public virtual void Clear()
        {
            DoClearLog();
        }

        #endregion

        #region protected

        protected string GetAppName()
        {
            var appName = Process.GetCurrentProcess().ProcessName;
            var vshostIndex = appName.LastIndexOf(".vshost", StringComparison.CurrentCulture);
            if (vshostIndex > 0)
            {
                appName = appName.Substring(0, vshostIndex);
            }
            return appName;
        }

        protected string BuildLogString(Logger.Levels level, int skipFrames, string message, object[] args)
        {
            var time = DateTime.Now.ToString(Settings.DateTimeFormat);
            var lvl = LevelToString(level);
            var site = GetMethodName(new StackFrame(skipFrames).GetMethod());
            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            return string.Join(Settings.Separator, time, lvl, site, message);
        }

        protected string LevelToString(Logger.Levels level)
        {
            switch (Settings.LevelCase)
            {
                case SettingsHolder.LevelCaseModes.UpperCase:
                    return level.ToString().ToUpper();
                case SettingsHolder.LevelCaseModes.LowerCase:
                    return level.ToString().ToLower();
                default:
                    return level.ToString();
            }
        }

        protected string GetMethodName(MethodBase method)
        {
            return method.ReflectedType.FullName + "." + method.Name;
        }

        #endregion

        #region private

        private void WriteLog(string log, int tries = 1, int delay = 1000)
        {
            if (Settings.Async)
            {
                WriteLogAsync(log, tries, delay);
            }
            else
            {
                WriteLogSync(log, tries, delay);
            }
        }

        private void WriteLogSync(string log, int tries, int delay)
        {
            lock (_lockingObject)
            {
                try
                {
                    DoWriteLog(log);
                }
                catch (Exception ex)
                {
                    if (tries > 0)
                    {
                        var newLog = BuildLogString(Logger.Levels.Error, 2, "Cannot write log. Exception: {0}", new object[] { ex.Message });
                        Task.Delay(delay).ContinueWith(t => WriteLog(newLog + Environment.NewLine + log, tries - 1));
                    }
                }
            }
        }

        private void WriteLogAsync(string log, int tries, int delay)
        {
            Task.Factory.StartNew(() => WriteLogSync(log, tries, delay));
        }

        #endregion

        #region virtual

        protected virtual void DoWriteLog(string log)
        {
        }

        protected virtual void DoClearLog()
        {
        }

        #endregion
    }

    /// <summary>
    /// Logger class
    /// </summary>
    public class DefaultLogger : AbstractLogger
    {
        #region settings

        /// <summary>
        /// Settings of logger
        /// </summary>
        public new SettingsHolder Settings { get; protected set; }

        /// <summary>
        /// Settings class of DefaultLogger
        /// </summary>
        public new class SettingsHolder : AbstractLogger.SettingsHolder
        {
            /// <summary>
            /// Get or set current full log file name
            /// </summary>
            public string FileName;

            /// <summary>
            /// Set log file (in directory where application exists)
            /// </summary>
            /// <param name="filename">Name of log file</param>
            public void SetLogFile(string filename)
            {
                var directory = AppDomain.CurrentDomain.BaseDirectory;
                FileName = Path.Combine(directory, filename);
            }

            /// <summary>
            /// Default SettingsHolder constructor
            /// </summary>
            public SettingsHolder()
            {
            }

            /// <summary>
            /// SettingsHolder constructor
            /// </summary>
            /// <param name="additionalSkipFrames">Additional skip frames</param>
            public SettingsHolder(int additionalSkipFrames)
                : base(additionalSkipFrames)
            {
            }
        }

        #endregion

        #region ctor

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        public DefaultLogger(SettingsHolder settings)
            : base(settings)
        {
            Settings = settings;
            if (settings.FileName == null)
            {
                Settings.SetLogFile(GetAppName() + ".log");
            }
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="async">Run logging async</param>
        public DefaultLogger(bool async = false)
            : this(new SettingsHolder { Async = async })
        {
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="logFlePath">Full path of log file</param>
        /// <param name="async">Run logging async</param>
        public DefaultLogger(string logFlePath, bool async = false)
            : this(new SettingsHolder { FileName = logFlePath, Async = async })
        {
        }

        #endregion

        #region override

        protected override void DoWriteLog(string log)
        {
            using (var fileStream = new FileStream(Settings.FileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                using (var logStream = new StreamWriter(fileStream))
                {
                    logStream.WriteLine(log);
                }
            }
        }

        protected override void DoClearLog()
        {
            if (File.Exists(Settings.FileName))
            {
                using (new FileStream(Settings.FileName, FileMode.Truncate, FileAccess.Write, FileShare.Read))
                {
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Logger class with log rotation
    /// </summary>
    public class RotationLogger : DefaultLogger
    {
        #region properties

        /// <summary>
        /// Name of file that rotated last time
        /// </summary>
        public string LastRotatedFile { get; private set; }
        
        #endregion

        #region settings

        /// <summary>
        /// Default maximum log file length (10 MiB)
        /// </summary>
        private const int DefaultLogFileLength = 10485760;

        /// <summary>
        /// Settings of logger
        /// </summary>
        public new SettingsHolder Settings { get; protected set; }

        /// <summary>
        /// Settings class of RotationLogger
        /// </summary>
        public new class SettingsHolder : DefaultLogger.SettingsHolder
        {
            /// <summary>
            /// Rotation modes
            /// </summary>
            public enum RotationByValues
            {
                /// <summary>
                /// Rotate by file length
                /// </summary>
                Length = 1, 

                /// <summary>
                /// Rotate by file date
                /// </summary>
                Date = 2, 

                /// <summary>
                /// Rotate by length and date of file
                /// </summary>
                LengthAndDate = 3
            }

            /// <summary>
            /// Modes of date rotation
            /// </summary>
            public enum RotationModeValues
            {
                /// <summary>
                /// Daily rotation
                /// </summary>
                Daily, 

                /// <summary>
                /// Weekly rotation
                /// </summary>
                Weekly, 

                /// <summary>
                /// Monthly rotation
                /// </summary>
                Monthly
            }

            /// <summary>
            /// Get or set maximum log file length
            /// </summary>
            public int LogFileLength = DefaultLogFileLength;

            /// <summary>
            /// Get or set mode rotation
            /// </summary>
            public RotationByValues RotationBy = RotationByValues.Date;

            /// <summary>
            /// Get or set mode rotation by date
            /// </summary>
            public RotationModeValues DateRotationMode = RotationModeValues.Monthly;
        }

        #endregion

        #region ctor

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        public RotationLogger(SettingsHolder settings)
            : base(settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="async">Run logging async</param>
        public RotationLogger(bool async = false)
            : this(new SettingsHolder { Async = async })
        {
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="logFlePath">Full path of log file</param>
        /// <param name="async">Run logging async</param>
        public RotationLogger(string logFlePath, bool async = false)
            : this(new SettingsHolder { FileName = logFlePath, Async = async })
        {
        }

        #endregion

        #region override

        protected override void DoWriteLog(string log)
        {
            var fileMode = FileMode.Append;
            var fileInfo = new FileInfo(Settings.FileName);
            var state = IsNeedRotate(fileInfo, log);
            if (fileInfo.Exists && state.HasValue)
            {
                RotateFile(state.Value, fileInfo);
                fileMode = FileMode.Truncate;
            }
            using (var fileStream = new FileStream(Settings.FileName, fileMode, FileAccess.Write, FileShare.Read))
            {
                using (var logStream = new StreamWriter(fileStream))
                {
                    logStream.WriteLine(log);
                }
            }
        }

        #endregion

        #region private

        private SettingsHolder.RotationByValues? IsNeedRotate(FileInfo fileInfo, string log)
        {
            var expiredByLength = ExpiredByLength(fileInfo, log);
            var expiredByDate = ExpiredByDate(fileInfo);
            var result = (expiredByLength ? 1 : 0) + (expiredByDate ? 2 : 0);
            return result > 0 ? (SettingsHolder.RotationByValues?)result : null;
        }

        private void RotateFile(SettingsHolder.RotationByValues state, FileInfo fileInfo)
        {
            var baseFile = fileInfo.FullName;
            var ext = fileInfo.Extension;
            if (ext.Length > 0)
            {
                baseFile = baseFile.Substring(0, baseFile.LastIndexOf(ext, StringComparison.Ordinal));
            }
            var suffix = BuildLogSuffix(state, fileInfo);
            string newFile = string.Format("{0}.{1}{2}", baseFile, suffix, ext);
            var i = 1;
            while (File.Exists(newFile))
            {
                newFile = string.Format("{0}.{1}.{2}{3}", baseFile, suffix, i++, ext);
            }
            fileInfo.CreationTimeUtc = DateTime.UtcNow;
            fileInfo.CopyTo(newFile);
            LastRotatedFile = newFile;
        }

        private string BuildLogSuffix(SettingsHolder.RotationByValues state, FileInfo fileInfo)
        {
            DateTime lastWriteTime;
            try
            {
                lastWriteTime = fileInfo.LastWriteTimeUtc;
            }
            catch
            {
                lastWriteTime = DateTime.UtcNow;
            }
            switch (state)
            {
                case SettingsHolder.RotationByValues.Length:
                    return lastWriteTime.ToString("yyyy-MM-dd");
                case SettingsHolder.RotationByValues.Date:
                    switch (Settings.DateRotationMode)
                    {
                        case SettingsHolder.RotationModeValues.Weekly:
                            var now = DateTime.UtcNow;
                            var weeks = Math.Abs(GetWeek(now) - GetWeek(fileInfo.LastWriteTimeUtc));
                            if (weeks > 2)
                            {
                                return lastWriteTime.ToString("yyyy-MM-dd");
                            }
                            return lastWriteTime.ToString("yyyy-MM") + "." + GetWeek(lastWriteTime);
                        case SettingsHolder.RotationModeValues.Monthly:
                            return fileInfo.LastWriteTimeUtc.ToString("yyyy-MM");
                        default:
                            return lastWriteTime.ToString("yyyy-MM-dd");
                    }
                default:
                    return lastWriteTime.ToString("yyyy-MM-dd");
            }
        }

        private bool ExpiredByLength(FileInfo fileInfo, string log)
        {
            if (!fileInfo.Exists) return false;
            return (fileInfo.Length + log.Length) > Settings.LogFileLength;
        }

        private bool ExpiredByDate(FileInfo fileInfo)
        {
            var fileTime = fileInfo.LastWriteTimeUtc;
            var currentTime = DateTime.UtcNow;
            switch (Settings.DateRotationMode)
            {
                case SettingsHolder.RotationModeValues.Daily:
                    return (currentTime - fileTime).TotalDays >= 1;
                case SettingsHolder.RotationModeValues.Weekly:
                    return Math.Abs(GetWeek(currentTime) - GetWeek(fileTime)) >= 1;
                default:
                    return Math.Abs(currentTime.Month - fileTime.Month) >= 1;
            }
        }

        private static int GetWeek(DateTime time)
        {
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var day = calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }
            return calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
        }

        #endregion
    }

    /// <summary>
    /// Logger class with callback function
    /// </summary>
    public class CallbackLogger : AbstractLogger
    {
        #region variables

        public delegate void CallbackLogDelegate(string log);
        public delegate void CallbackPartsDelegate(string time, string level, string site, string message);

        private readonly CallbackLogDelegate _callbackLog;
        private readonly CallbackPartsDelegate _callbackParts;
        private readonly Action _callbackClear;

        #endregion

        #region ctor

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="callback">Callback function</param>
        /// <param name="callbackClear">Callback for clear all logs</param>
        public CallbackLogger(SettingsHolder settings, CallbackLogDelegate callback, Action callbackClear = null)
            : base(settings)
        {
            _callbackLog = callback;
            _callbackClear = callbackClear;
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="callback">Callback function</param>
        /// <param name="callbackClear">Callback for clear all logs</param>
        /// <param name="async">Run logging async</param>
        public CallbackLogger(CallbackLogDelegate callback, Action callbackClear = null, bool async = false)
            : this(new SettingsHolder { Async = async }, callback, callbackClear)
        {
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="callback">Callback function</param>
        /// <param name="callbackClear">Callback for clear all logs</param>
        public CallbackLogger(SettingsHolder settings, CallbackPartsDelegate callback, Action callbackClear = null)
            : base(settings)
        {
            _callbackParts = callback;
            _callbackClear = callbackClear;
        }

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="callback">Callback function</param>
        /// <param name="callbackClear">Callback for clear all logs</param>
        /// <param name="async">Run logging async</param>
        public CallbackLogger(CallbackPartsDelegate callback, Action callbackClear = null, bool async = false)
            : this(new SettingsHolder { Async = async }, callback, callbackClear)
        {
        }

        #endregion

        #region override
        
        /// <summary>
        /// Log many messages
        /// </summary>
        /// <param name="messages">Messages (list or dictionary)</param>
        public override void Multiple(IEnumerable<KeyValuePair<string, Logger.Levels>> messages)
        {
            if (!Settings.Enabled) return;
            foreach (var pair in messages)
            {
                Callback(pair.Value, DefaultSkipFrames, pair.Key);
            }
        }

        /// <summary>
        /// Log many messages with selected level
        /// </summary>
        /// <param name="messages">List of messages</param>
        /// <param name="level">Level of log</param>
        public override void Multiple(IEnumerable<string> messages, Logger.Levels level = Logger.Levels.Debug)
        {
            if (!Settings.Enabled) return;
            foreach (var message in messages)
            {
                Callback(level, DefaultSkipFrames, message);
            }
        }

        /// <summary>
        /// Log message with selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Log(Logger.Levels level, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            Callback(level, DefaultSkipFrames, message, args);
        }

        /// <summary>
        /// Log message with skip frames and selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="skipFrames">Additional skip frames of stack</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void LogSkipFrames(Logger.Levels level, int skipFrames, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            Callback(level, skipFrames + DefaultSkipFrames, message, args);
        }

        public override void Clear()
        {
            if (_callbackClear != null)
            {
                _callbackClear();
            }
        }

        #endregion

        #region private

        private void Callback(Logger.Levels level, int skipFrames, string message, params object[] args)
        {
            var time = DateTime.Now.ToString(Settings.DateTimeFormat);
            var lvl = LevelToString(level);
            var site = GetMethodName(new StackFrame(skipFrames).GetMethod());
            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            if (_callbackLog != null)
            {
                _callbackLog(string.Join(Settings.Separator, time, lvl, site, message));
            }
            else
            {
                _callbackParts(time, lvl, site, message);
            }
        }

        #endregion
    }

    /// <summary>
    /// Logger class with chained loggers
    /// </summary>
    public class ChainLogger : AbstractLogger
    {
        #region variables

        private readonly DefaultLogger[] _loggers;

        #endregion

        #region ctor

        /// <summary>
        /// Logger class constructor
        /// </summary>
        /// <param name="loggers">Loggers</param>
        public ChainLogger(params DefaultLogger[] loggers)
        {
            _loggers = loggers;
        }

        #endregion
        
        #region public

        /// <summary>
        /// Log message with Debug level. Writes in DEBUG mode only
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Debug(string message, params object[] args)
        {
#if DEBUG
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Debug, 1, message, args);
            }
#endif
        }

        /// <summary>
        /// Log message with Debug level with previous method from stack trace. Writes in DEBUG mode only
        /// </summary>
        /// <param name="skipFrames">Skip stack frames to get the function name</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void DebugSkipFrames(int skipFrames, string message, params object[] args)
        {
#if DEBUG
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Debug, 1 + skipFrames, message, args);
            }
#endif
        }

        /// <summary>
        /// Log message with Info level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Info(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Info, 1, message, args);
            }
        }

        /// <summary>
        /// Log message with Warn level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Warn(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Warn, 1, message, args);
            }
        }

        /// <summary>
        /// Log message with Error level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Error(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Error, 1, message, args);
            }
        }

        /// <summary>
        /// Log message with Fatal level
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Fatal(string message, params object[] args)
        {
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(Logger.Levels.Fatal, 1, message, args);
            }
        }

        /// <summary>
        /// Log many messages
        /// </summary>
        /// <param name="messages">Messages (list or dictionary)</param>
        public override void Multiple(IEnumerable<KeyValuePair<string, Logger.Levels>> messages)
        {
            if (!Settings.Enabled) return;

            var msgs = messages.ToList();
            foreach (var logger in _loggers)
            {
                foreach (var message in msgs)
                {
                    logger.LogSkipFrames(message.Value, 1, message.Key);
                }
            }
        }

        /// <summary>
        /// Log many messages with selected level
        /// </summary>
        /// <param name="messages">List of messages</param>
        /// <param name="level">Level of log</param>
        public override void Multiple(IEnumerable<string> messages, Logger.Levels level = Logger.Levels.Debug)
        {
            if (!Settings.Enabled) return;

            var msgs = messages.ToList();
            foreach (var logger in _loggers)
            {
                foreach (var message in msgs)
                {
                    logger.LogSkipFrames(level, 1, message);
                }
            }
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="level">Level of log</param>
        public override void LogException(Exception exception, Logger.Levels level = Logger.Levels.Error)
        {
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(level, 1, Settings.ExceptionLogMessage, new object[]
                {
                    exception.GetType().Name, 
                    exception.Message, 
                    GetMethodName(exception.TargetSite), 
                    exception.StackTrace
                });
            }
        }

        /// <summary>
        /// Log message with selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void Log(Logger.Levels level, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(level, 1, message, args);
            }
        }

        /// <summary>
        /// Log message with skip frames and selected level
        /// </summary>
        /// <param name="level">Level</param>
        /// <param name="skipFrames">Additional skip frames of stack</param>
        /// <param name="message">Message</param>
        /// <param name="args">Arguments of message</param>
        public override void LogSkipFrames(Logger.Levels level, int skipFrames, string message, params object[] args)
        {
            if (!Settings.Enabled) return;
            foreach (var logger in _loggers)
            {
                logger.LogSkipFrames(level, 1 + skipFrames, message, args);
            }
        }

        public override void Clear()
        {
            foreach (var logger in _loggers)
            {
                logger.Clear();
            }
        }

        #endregion
    }

    public class LoggerTesting
    {
        private const string TestingLoggerMessage = "Testing {0}";
        private const string TestMessage = "Test message";

        private readonly Random _random = new Random();

        #region public

        public double Benchmark(AbstractLogger logger = null, int count = 10000, bool verbose = true)
        {
            string logFlePath = null;
            if (logger == null)
            {
                logFlePath = GetLogFlePath("benchmark", _random.Next());
                logger = new DefaultLogger(logFlePath);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < count; i++)
            {
                logger.Debug(TestMessage + " " + i);
            }
            stopwatch.Stop();

            var overall = stopwatch.ElapsedMilliseconds / 1000.0;
            var timePerLog = overall / count;
            if (verbose)
            {
                Console.WriteLine(@"Benchmark of {0}:", logger.GetType().Name);
                Console.WriteLine(@"  Count of logs: {0}", count);
                Console.WriteLine(@"  Overall time: {0} sec", overall);
                Console.WriteLine(@"  Time of one log: {0} msec", timePerLog*1000);
            }
            if (logFlePath != null) File.Delete(logFlePath);
            return timePerLog;
        }

        public Dictionary<string, bool> RunAll(bool verbose = true, bool deleteFiles = true)
        {
            var watch = new Stopwatch();
            watch.Start();
            var random = _random.Next();
            var results = new Dictionary<string, bool>
            {
                { "Static", RunStatic(verbose, deleteFiles, random) },
                { "Default", RunDefault(verbose, deleteFiles, random) },
                { "Rotation", RunRotation(verbose, deleteFiles, random) },
                { "Callback", RunCallback(verbose, deleteFiles, random) },
                { "Chain", RunChain(verbose, deleteFiles, random) },
                { "Settings", RunSettings(verbose, deleteFiles, random) },
            };
            if (verbose) PrintExecutionTime(watch, 0);
            return results;
        }

        public bool RunStatic(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (verbose) Console.WriteLine(TestingLoggerMessage, @"static logger");

            var prevLogger = Logger.GetLogger();
            var logFlePath = GetLogFlePath("static", random);
            var logger = new DefaultLogger(logFlePath);
            logger.Clear();
            Logger.SetLogger(logger);
            var results = new List<bool>();

            // common methods
            Logger.Debug(TestMessage);
            Logger.Error(TestMessage);
            var logs = ParseAll(GetAllLogs(logFlePath), logger.Settings.Separator);

            var result = DateTime.Now - logs[0].Time < TimeSpan.FromSeconds(1);
            if (verbose) PrintTest("log time", result);
            results.Add(result);

            result = logs[1].Level == Logger.Levels.Error;
            if (verbose) PrintTest("log level", result);
            results.Add(result);

            result = logs[0].Site == Logger.GetCaller(0);
            if (verbose) PrintTest("log site", result);
            results.Add(result);

            result = logs[0].Message == TestMessage;
            if (verbose) PrintTest("log message", result);
            results.Add(result);

            // clear
            logger.Clear();
            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            logger.Clear();
            result = new FileInfo(logFlePath).Length == 0;
            if (verbose) PrintTest("Clear", result);
            results.Add(result);

            Logger.SetLogger(prevLogger);
            if (deleteFile) File.Delete(logFlePath);
            if (verbose) PrintExecutionTime(watch);
            return results.TrueForAll(r => r);
        }

        public bool RunDefault(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            var logFlePath = GetLogFlePath("default", random);
            var logger = new DefaultLogger(logFlePath);
            logger.Clear();

            var results = TestLogger(logger, logFlePath, verbose);

            File.Delete(logFlePath);
            if (verbose) PrintExecutionTime(watch);
            return results.TrueForAll(r => r);
        }

        public bool RunRotation(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            var logFlePath = GetLogFlePath("rotation", random);
            var logger = new RotationLogger(logFlePath);
            logger.Clear();

            var results = TestLogger(logger, logFlePath, verbose);

            // rotation by length
            logger.Clear();
            logger.Settings.RotationBy = RotationLogger.SettingsHolder.RotationByValues.Length;
            logger.Debug(TestMessage);
            var logLength = new FileInfo(logFlePath).Length;
            logger.Settings.LogFileLength = (int)(2.5 * logLength);
            logger.Debug(TestMessage);
            logger.Debug(TestMessage);

            var result = CheckRotationFiles(logger, logFlePath, deleteFile);
            if (verbose) PrintTest("rotation by length", result);
            results.Add(result);

            // rotation by date
            var settings = new RotationLogger.SettingsHolder { RotationBy = RotationLogger.SettingsHolder.RotationByValues.Date };
            var fileInfo = new FileInfo(logFlePath);

            // rotation by date daily
            settings.DateRotationMode = RotationLogger.SettingsHolder.RotationModeValues.Daily;
            logger = new RotationLogger(settings);
            settings.SetLogFile(logFlePath);
            logger.Clear();

            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            fileInfo.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc - TimeSpan.FromDays(1);
            logger.Debug(TestMessage);

            result = CheckRotationFiles(logger, logFlePath, deleteFile);
            if (verbose) PrintTest("rotation by date daily", result);
            results.Add(result);

            // rotation by date weekly
            settings.DateRotationMode = RotationLogger.SettingsHolder.RotationModeValues.Weekly;
            logger = new RotationLogger(settings);
            settings.SetLogFile(logFlePath);
            logger.Clear();
            fileInfo.LastWriteTimeUtc = DateTime.UtcNow;

            logger.Debug(TestMessage);
            fileInfo.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc - TimeSpan.FromDays(1);
            logger.Debug(TestMessage);
            result = logger.LastRotatedFile == null && GetAllLogs(logFlePath).Length == 2;
            fileInfo.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc - TimeSpan.FromDays(7);
            logger.Debug(TestMessage);

            result &= CheckRotationFiles(logger, logFlePath, deleteFile);
            if (verbose) PrintTest("rotation by date weekly ", result);
            results.Add(result);

            // rotation by date monthly
            settings.DateRotationMode = RotationLogger.SettingsHolder.RotationModeValues.Monthly;
            logger = new RotationLogger(settings);
            settings.SetLogFile(logFlePath);
            logger.Clear();
            fileInfo.LastWriteTimeUtc = DateTime.UtcNow;

            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            result = logger.LastRotatedFile == null && GetAllLogs(logFlePath).Length == 2;
            fileInfo.LastWriteTimeUtc = fileInfo.LastWriteTimeUtc - TimeSpan.FromDays(30);
            logger.Debug(TestMessage);

            result &= CheckRotationFiles(logger, logFlePath, deleteFile);
            if (verbose) PrintTest("rotation by date monthly", result);
            results.Add(result);

            if (deleteFile) File.Delete(logFlePath);
            if (verbose) PrintExecutionTime(watch);
            return results.TrueForAll(r => r);
        }

        public bool RunCallback(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            var logFlePath = GetLogFlePath("callback", random);
            var logger = new CallbackLogger(log => File.AppendAllText(logFlePath, log + Environment.NewLine), () => File.WriteAllText(logFlePath, ""));
            logger.Clear();

            var results = TestLogger(logger, logFlePath, verbose);

            if (deleteFile) File.Delete(logFlePath);
            if (verbose) PrintExecutionTime(watch);
            return results.TrueForAll(r => r);
        }

        public bool RunChain(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            random = random >= 0 ? random : _random.Next();
            var logFlePath = GetLogFlePath("chain", random);
            var logger1 = new DefaultLogger(logFlePath);
            var logger = new ChainLogger(logger1);
            logger.Clear();

            var results = TestLogger(logger, logFlePath, verbose);

            // clear several loggers
            var logFlePath2 = GetLogFlePath("chain.several", random);
            var logger2 = new DefaultLogger(logFlePath2);
            logger = new ChainLogger(logger1, logger2);
            File.WriteAllText(logFlePath, TestMessage);
            File.WriteAllText(logFlePath2, TestMessage);
            logger.Clear();
            var result = new FileInfo(logFlePath).Length == 0 && new FileInfo(logFlePath2).Length == 0;
            if (verbose) PrintTest("clear several loggers", result);
            results.Add(result);

            if (deleteFile)
            {
                File.Delete(logFlePath);
                File.Delete(logFlePath2);
            }
            if (verbose) PrintExecutionTime(watch);

            return results.TrueForAll(r => r);
        }

        public bool RunSettings(bool verbose = true, bool deleteFile = true, int random = -1)
        {
            var watch = new Stopwatch();
            watch.Start();
            if (verbose) Console.WriteLine(TestingLoggerMessage, @"Settings");
            random = random >= 0 ? random : _random.Next();
            var logFlePath = GetLogFlePath("settings", random);
            var logger = new DefaultLogger(logFlePath);
            var results = new List<bool>();

            // async
            var logFlePathAsync = GetLogFlePath("settings.async", random);
            var loggerAsync = new DefaultLogger(logFlePathAsync, true);
            logger.Clear();
            loggerAsync.Clear();
            var firstMessage = TestMessage + 1;
            var secondMessage = TestMessage + 2;
            loggerAsync.Debug(firstMessage);
            logger.Debug(secondMessage);
            Thread.Sleep(250);
            var logs = ParseAll(GetAllLogs(logFlePath), logger.Settings.Separator);
            var logsAsync = ParseAll(GetAllLogs(logFlePathAsync), loggerAsync.Settings.Separator);
            var result = logs.Count == 1 && logsAsync.Count == 1 && logs[0].Message == secondMessage && logsAsync[0].Message == firstMessage;
            result &= new FileInfo(logFlePath).CreationTimeUtc < new FileInfo(logFlePathAsync).CreationTimeUtc;
            if (verbose) PrintTest("async", result);
            results.Add(result);

            // TODO test AdditionalSkipFrames
            // TODO test LevelCase
            // TODO test Separator
            // TODO test DateTimeFormat

            if (deleteFile)
            {
                File.Delete(logFlePath);
                File.Delete(logFlePathAsync);
            }
            if (verbose) PrintExecutionTime(watch);

            return results.TrueForAll(r => r); 
        }

        #endregion

        #region inner class

        private class Log
        {
            public readonly DateTime Time;
            public readonly Logger.Levels Level;
            public readonly string Site;
            public readonly string Message;

            public Log(DateTime time, Logger.Levels level, string site, string message)
            {
                Time = time;
                Level = level;
                Site = site;
                Message = message;
            }
        }

        #endregion

        #region private

        private string GetLogFlePath(string title, int random)
        {
            var path = Environment.GetCommandLineArgs()[0];
            random = random >= 0 ? random : _random.Next();
            return string.Format("{0}.test.{1}.{2}.log", path, title, random);
        }

        private List<bool> TestLogger(AbstractLogger logger, string logFlePath, bool verbose)
        {
            if (verbose) Console.WriteLine(TestingLoggerMessage, logger.GetType().Name);

            var results = new List<bool>();

            // common methods
            logger.Debug(TestMessage);
            logger.Error(TestMessage);
            var logs = ParseAll(GetAllLogs(logFlePath), logger.Settings.Separator);

            var result = DateTime.Now - logs[0].Time < TimeSpan.FromSeconds(1);
            if (verbose) PrintTest("log time", result);
            results.Add(result);

            result = logs[1].Level == Logger.Levels.Error;
            if (verbose) PrintTest("log level", result);
            results.Add(result);

            result = logs[0].Message == TestMessage;
            if (verbose) PrintTest("log message", result);
            results.Add(result);

            result = logs[0].Site == Logger.GetCaller(0);
            if (verbose) PrintTest("log site", result);
            results.Add(result);

            // skip frames
            logger.Clear();
            WriteLog(logger, TestMessage, 0);
            WriteLog(logger, TestMessage, 1);
            logs = ParseAll(GetAllLogs(logFlePath), logger.Settings.Separator);
            result = logs[0].Site == (new StackFrame().GetMethod().ReflectedType.FullName + ".WriteLog");
            result &= logs[1].Site == Logger.GetCaller(0);
            if (verbose) PrintTest("log site skip frames", result);
            results.Add(result);

            // multiple
            logger.Clear();
            logger.Multiple(new[] { TestMessage + 1, TestMessage + 2 });
            logs = ParseAll(GetAllLogs(logFlePath), logger.Settings.Separator);

            result = logs[0].Message == TestMessage + 1;
            result &= logs[1].Message == TestMessage + 2;
            if (verbose) PrintTest("Multiple", result);
            results.Add(result);

            // clear
            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            logger.Debug(TestMessage);
            result = new FileInfo(logFlePath).Length > 0;
            logger.Clear();
            result &= new FileInfo(logFlePath).Length == 0;
            if (verbose) PrintTest("Clear", result);
            results.Add(result);

            return results;
        }

        private bool CheckRotationFiles(RotationLogger logger, string logFlePath, bool deleteFile)
        {
            var result = logger.LastRotatedFile != null
                      && logFlePath != logger.LastRotatedFile
                      && GetAllLogs(logger.LastRotatedFile).Length == 2
                      && GetAllLogs(logFlePath).Length == 1;
            if (deleteFile && logger.LastRotatedFile != null && logFlePath != logger.LastRotatedFile && File.Exists(logger.LastRotatedFile))
            {
                File.Delete(logger.LastRotatedFile);
            }
            return result;
        }

        private void PrintTest(string title, bool result)
        {
            Console.Write(@"  - Test {0}: ", title);
            var color = Console.ForegroundColor;
            Console.ForegroundColor = result ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(result ? "OK" : "Fail");
            Console.ForegroundColor = color;
            Console.WriteLine();
        }

        private void PrintExecutionTime(Stopwatch watch, int indent = 2)
        {
            watch.Stop();
            Console.WriteLine(@"{0}Execution time: {1} sec", string.Empty.PadLeft(indent), watch.ElapsedMilliseconds / 1000.0);
        }

        private Log Parse(string log, string separator)
        {
            var parts = log.Split(new[] {separator}, StringSplitOptions.None);
            try
            {
                var dateTime = DateTime.Parse(parts[0]);
                var level = (Logger.Levels)Enum.Parse(typeof(Logger.Levels), parts[1], true);
                return new Log(dateTime, level, parts[2], parts[3]);
            }
            catch (Exception e)
            {
                return new Log(DateTime.Now, Logger.Levels.Error, Logger.GetCaller(0), e.Message);
            }
        }

        private List<Log> ParseAll(IEnumerable<string> logs, string separator)
        {
            return logs.Select(log => Parse(log, separator)).ToList();
        }

        private string[] GetAllLogs(string filename)
        {
            return File.ReadAllLines(filename);
        }

        private void WriteLog(AbstractLogger logger, string message, int skipFrames, Logger.Levels level = Logger.Levels.Debug)
        {
            logger.LogSkipFrames(level, skipFrames, message);
        }

        #endregion
    }
}
