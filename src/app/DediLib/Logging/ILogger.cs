using System;

namespace DediLib.Logging
{
    public interface ILogger
    {
        string Name { get; }
        ITimeSource TimeSource { get; set; }

        void Trace(string logText, params object[] formatValues);
        void Debug(string logText, params object[] formatValues);
        void Info(string logText, params object[] formatValues);
        void Warning(string logText, params object[] formatValues);
        void Error(Exception exception);
        void Error(Exception exception, string logText);
        void Error(string logText, params object[] formatValues);
        void Fatal(Exception exception);
        void Fatal(Exception exception, string logText);
        void Fatal(string logText, params object[] formatValues);
    }
}
