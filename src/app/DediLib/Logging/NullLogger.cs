using System;

namespace DediLib.Logging
{
    public class NullLogger : ILogger
    {
        public string Name { get; }

        public ITimeSource TimeSource { get; set; } = new DefaultTimeSource();

        public NullLogger()
        {
        }

        public NullLogger(string name)
        {
            Name = name;
        }

        public void Debug(string logText, params object[] formatValues)
        {
        }

        public void Info(string logText, params object[] formatValues)
        {
        }

        public void Warning(string logText, params object[] formatValues)
        {
        }

        public void Error(Exception exception)
        {
        }

        public void Error(Exception exception, string logText)
        {
        }

        public void Error(string logText, params object[] formatValues)
        {
        }
    }
}
