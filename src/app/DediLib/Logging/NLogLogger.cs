using System;

namespace DediLib.Logging
{
    public class NLogLogger : ILogger
    {
        private static readonly NLogLooseBinding NLogBinding = new NLogLooseBinding();

        private readonly object _logger;

        public ITimeSource TimeSource { get; set; }

        public NLogLogger(string name)
        {
            _logger = NLogBinding.GetCreateLoggerMethod().Invoke(name);
        }

        public void Debug(string logText, params object[] formatValues)
        {
            NLogBinding.GetDebugMethod().Invoke(_logger, FormatText(logText, formatValues));
        }

        private static string FormatText(string logText, object[] formatValues)
        {
            return formatValues == null || formatValues.Length == 0 ? logText : string.Format(logText, formatValues);
        }

        public void Info(string logText, params object[] formatValues)
        {
            NLogBinding.GetInfoMethod().Invoke(_logger, FormatText(logText, formatValues));
        }

        public void Warning(string logText, params object[] formatValues)
        {
            NLogBinding.GetWarnMethod().Invoke(_logger, FormatText(logText, formatValues));
        }

        public void Error(Exception exception)
        {
            NLogBinding.GetErrorWithExceptionMethod().Invoke(_logger, exception, null);
        }

        public void Error(Exception exception, string logText)
        {
            NLogBinding.GetErrorWithExceptionMethod().Invoke(_logger, exception, logText);
        }

        public void Error(string logText, params object[] formatValues)
        {
            NLogBinding.GetErrorMethod().Invoke(_logger, FormatText(logText, formatValues));
        }
    }
}
