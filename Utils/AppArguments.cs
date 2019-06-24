using System;
using System.Collections.Generic;
using System.Linq;

namespace LogMonitor.Utils
{
    /// <summary>
    /// Аргументы приложения
    /// </summary>
    public class AppArguments
    {
        #region inner classes

        private abstract class CommandBase
        {
            public string Argument { get; private set; }

            protected CommandBase(string argument)
            {
                Argument = argument;
            }
        }

        private class Command : CommandBase
        {
            public int Number { get; private set; }

            public Command(int number, string argument)
                : base(argument)
            {
                Number = number;
            }
        }

        private class Option : CommandBase
        {
            public string Key { get; private set; }

            public Option(string key, string argument)
                : base(argument)
            {
                Key = key;
            }
        }

        #endregion
        
        #region variables

        /// <summary>
        /// Команды
        /// </summary>
        private readonly List<Command> _commands = new List<Command>();

        /// <summary>
        /// Команды
        /// </summary>
        private readonly List<Option> _options = new List<Option>();

        #endregion

        #region static

        /// <summary>
        /// Ключи считать со строчного символа
        /// </summary>
        public static bool UseLowerCaseKey = true;

        /// <summary>
        /// Получение аргументов приложения
        /// </summary>
        /// <param name="commandLine">Строка аргументов. Если отсутствует, то используются аргументы приложения</param>
        /// <returns>Объект AppArguments</returns>
        public static AppArguments FromCommandLine(string commandLine = "")
        {
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                commandLine = Environment.CommandLine;
            }
            return new AppArguments(ParseArguments(commandLine));
        }

        /// <summary>
        /// Чтение строки аргументов
        /// </summary>
        /// <param name="line">Строка аргументов</param>
        /// <returns>Список аргументов</returns>
        private static List<CommandBase> ParseArguments(string line)
        {
            var commands = new List<CommandBase>();
            if (string.IsNullOrWhiteSpace(line))
            {
                return commands;
            }
            bool isStarted = false, quotes = false, isOption = false, isOptionKeySet = false;
            int start = 0, commandNumber = 0;
            string key = "";
            for (int i = 0; i < line.Length; i++)
            {
                var current = line[i];
                int lastChar = i == (line.Length - 1) ? 1 : 0; // is last char of string
                if (!isStarted)
                {
                    isStarted = true;
                    start = i;
                    if (current == '-' || current == '/')
                    {
                        start++;
                        isOption = true;
                        isOptionKeySet = false;
                        continue;
                    }
                }
                if (current == '"')
                {
                    if (i > 0 && line[i - 1] == '\\') continue;
                    quotes = !quotes;
                    if (start == i) start++; // skip leading quote
                    if (lastChar == 0) continue;
                }
                if (current == '=')
                {
                    key = line.Substring(start, i - start);
                    start = i + 1;
                    isOptionKeySet = true;
                    continue;
                }
                if (current == ' ' || lastChar > 0)
                {
                    if (!quotes)
                    {
                        var arg = "";
                        if (isOptionKeySet)
                        {
                            arg = line.Substring(start, i - start + lastChar);
                        }
                        else
                        {
                            key = line.Substring(start, i - start + lastChar);
                        }
                        if (key.EndsWith("\"")) key = key.Substring(0, key.Length - 1); // skip trailing quotes
                        if (arg.EndsWith("\"")) arg = arg.Substring(0, arg.Length - 1);
                        key = key.Replace("\\\"", "\"");
                        arg = arg.Replace("\\\"", "\"");
                        if (isOption)
                        {
                            commands.Add(new Option(key, arg));
                        }
                        else
                        {
                            commands.Add(new Command(commandNumber++, key));
                        }
                        isStarted = false;
                        isOption = false;
                    }
                }
            }
            return commands;
        }
        
        #endregion

        #region constructors

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public AppArguments()
        {
        }

        /// <summary>
        /// Конструктор устанавливает нулевой командой текущий путь приложения
        /// </summary>
        /// <param name="useCurrentAppPath">Установить нулевой командой текущий путь приложения</param>
        public AppArguments(bool useCurrentAppPath)
        {
            if (useCurrentAppPath)
            {
                _commands.Add(new Command(0, Environment.GetCommandLineArgs()[0]));
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        private AppArguments(List<CommandBase> commands)
        {
            _commands = commands.OfType<Command>().ToList();
            _options = commands.OfType<Option>().ToList();
        }

        #endregion

        #region public methods

        #region Получение элементов

        /// <summary>
        /// Проверка наличия команды по номеру
        /// </summary>
        /// <param name="index">Номер</param>
        /// <returns>Команда существует</returns>
        public bool HasCommand(int index)
        {
            return _commands.Count > index;
        }

        /// <summary>
        /// Получение команды по номеру
        /// </summary>
        /// <param name="index">Номер</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Команда</returns>
        public string GetCommand(int index, string defaultValue = "")
        {
            return HasCommand(index)
                ? _commands[index].Argument
                : defaultValue;
        }

        /// <summary>
        /// Проверка наличия опции по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Опция существует</returns>
        public bool HasOption(string key)
        {
            return _options.Any(opt => opt.Key == key);
        }

        /// <summary>
        /// Проверка наличия опции по перечислению
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Опция существует</returns>
        public bool HasOption(Enum key)
        {
            return HasOption(ToEnumString(key));
        }

        /// <summary>
        /// Получить опцию по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Значение</returns>
        public string GetOption(string key, string defaultValue = "")
        {
            var option = _options.FirstOrDefault(opt => opt.Key == key);
            return option != null ? option.Argument : defaultValue;
        }

        /// <summary>
        /// Получить опцию по перечислению
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Значение</returns>
        public string GetOption(Enum key, string defaultValue = "")
        {
            return GetOption(ToEnumString(key), defaultValue);
        }

        /// <summary>
        /// Получить опцию по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Значение</returns>
        public int GetOptionInteger(string key, int defaultValue = 0)
        {
            var option = _options.FirstOrDefault(opt => opt.Key == key);
            int value;
            return option != null && int.TryParse(option.Argument, out value)
                ? value
                : defaultValue;
        }

        /// <summary>
        /// Получить опцию по перечислению
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns>Значение</returns>
        public int GetOptionInteger(Enum key, int defaultValue = 0)
        {
            return GetOptionInteger(ToEnumString(key), defaultValue);
        }

        #endregion

        #region Добавление элементов

        /// <summary>
        /// Добавить команду
        /// </summary>
        /// <param name="value">Значение</param>
        public void AddCommand(string value)
        {
            _commands.Add(new Command(_commands.Count, value));
        }

        /// <summary>
        /// Добавить строчную опцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void AddOption(string key, string value)
        {
            _options.Add(new Option(key, value));
        }

        /// <summary>
        /// Добавить строчную опцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void AddOption(Enum key, string value)
        {
            AddOption(ToEnumString(key), value);
        }

        /// <summary>
        /// Добавить числовую опцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void AddOption(string key, int value)
        {
            AddOption(key, value.ToString());
        }

        /// <summary>
        /// Добавить числовую опцию
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="value">Значение</param>
        public void AddOption(Enum key, int value)
        {
            AddOption(ToEnumString(key), value.ToString());
        }

        #endregion

        /// <summary>
        /// Привести аргументы в строку
        /// </summary>
        /// <returns>Строка</returns>
        public override string ToString()
        {
            return ToString('-');
        }

        /// <summary>
        /// Привести аргументы в строку
        /// </summary>
        /// <param name="prefix">Префикс опции</param>
        /// <returns>Строка</returns>
        public string ToString(char prefix)
        {
            var commands = String.Join(" ", _commands);
            var options = String.Join(" ", _options.Select(a => prefix + a.Key + "=" + a.Argument));
            if (!string.IsNullOrWhiteSpace(options))
            {
                commands += " " + options;
            }
            return commands;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Преобразовать перечисление в строку. Первый символ преобразовывать в строчный символ, если настроено в классе
        /// </summary>
        /// <param name="key">Перечисление</param>
        /// <returns>Строка</returns>
        private string ToEnumString(Enum key)
        {
            var keyName = Enum.GetName(key.GetType(), key);
            if (string.IsNullOrEmpty(keyName))
            {
                return string.Empty;
            }
            return UseLowerCaseKey
                ? char.ToLower(keyName[0]) + keyName.Substring(1)
                : keyName;
        }

        #endregion
    }
}
