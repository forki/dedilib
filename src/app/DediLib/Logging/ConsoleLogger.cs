using System;

namespace DediLib.Logging
{
    public class ConsoleLogger : ILogger
    {
        public bool ShowTimestamp { get; set; }

        public ConsoleLogger()
        {
            ShowTimestamp = true;
        }

        public ConsoleLogger(ITimeSource timeSource)
            : this()
        {
            TimeSource = timeSource ?? throw new ArgumentNullException(nameof(timeSource));
        }

        private void LogConsole(string logText, object[] formatValues)
        {
            if (ShowTimestamp) logText = TimeSource.UtcNow + " " + logText;

            if (formatValues != null && formatValues.Length != 0)
                Console.WriteLine(logText, formatValues);
            else
                Console.WriteLine(logText);
        }

        private readonly object _consoleLock = new object();

        public string Name { get; set; }

        public ITimeSource TimeSource { get; set; } = new DefaultTimeSource();

        private void LogConsole(ConsoleColor color, string logText, params object[] formatValues)
        {
            if (ShowTimestamp) logText = TimeSource.UtcNow + " " + logText;

            lock (_consoleLock)
            {
                var lastColor = Console.ForegroundColor;
                if (lastColor != color) Console.ForegroundColor = color;
                Console.WriteLine(logText, formatValues);
                if (lastColor != color) Console.ForegroundColor = lastColor;
            }
        }

        public void Trace(string logText, params object[] formatValues)
        {
            LogConsole(ConsoleColor.DarkGray, "TRACE " + logText, formatValues);
        }

        public void Debug(string logText, params object[] formatValues)
        {
            LogConsole("DEBUG " + logText, formatValues);
        }

        public void Info(string logText, params object[] formatValues)
        {
            LogConsole(ConsoleColor.DarkGreen, "INFO " + logText, formatValues);
        }

        public void Warning(string logText, params object[] formatValues)
        {
            LogConsole(ConsoleColor.DarkYellow, "WARN " + logText, formatValues);
        }

        public void Error(Exception exception)
        {
            LogConsole(ConsoleColor.Red, "ERROR {0}", exception);
        }

        public void Error(Exception exception, string logText)
        {
            LogConsole(ConsoleColor.Red, "ERROR {0}, Exception: {1}", logText, exception);
        }

        public void Error(string logText, params object[] formatValues)
        {
            LogConsole(ConsoleColor.Red, "ERROR " + logText, formatValues);
        }

        public void Fatal(Exception exception)
        {
            LogConsole(ConsoleColor.Magenta, "FATAL {0}", exception);
        }

        public void Fatal(Exception exception, string logText)
        {
            LogConsole(ConsoleColor.Magenta, "FATAL {0}, Exception: {1}", logText, exception);
        }

        public void Fatal(string logText, params object[] formatValues)
        {
            LogConsole(ConsoleColor.Magenta, "FATAL " + logText, formatValues);
        }
    }
}
