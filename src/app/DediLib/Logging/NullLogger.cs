using System;

namespace DediLib.Logging
{
    public class NullLogger : ILogger
    {
        private ITimeSource _timeSource = new DefaultTimeSource();
        public ITimeSource TimeSource
        {
            get { return _timeSource; }
            set { _timeSource = value; }
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
