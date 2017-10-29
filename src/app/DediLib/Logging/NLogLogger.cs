using System;

namespace DediLib.Logging
{
    public class NLogLogger : ILogger
    {
        private static readonly NLogLooseBinding NLogBinding = new NLogLooseBinding();

        private readonly object _logger;

        public string Name { get; }

        public ITimeSource TimeSource { get; set; }

        static NLogLogger()
        {
            if (NLogBinding.GetTraceMethod() == null)
                throw new InvalidOperationException("Could not find Trace method of NLog");

            if (NLogBinding.GetDebugMethod() == null)
                throw new InvalidOperationException("Could not find Debug method of NLog");

            if (NLogBinding.GetInfoMethod() == null)
                throw new InvalidOperationException("Could not find Info method of NLog");

            if (NLogBinding.GetWarnMethod() == null)
                throw new InvalidOperationException("Could not find Warn method of NLog");

            if (NLogBinding.GetErrorMethod() == null)
                throw new InvalidOperationException("Could not find Error method of NLog");

            if (NLogBinding.GetErrorWithExceptionMethod() == null)
                throw new InvalidOperationException("Could not find Error with exception method of NLog");

            if (NLogBinding.GetFatalMethod() == null)
                throw new InvalidOperationException("Could not find Fatal method of NLog");

            if (NLogBinding.GetFatalWithExceptionMethod() == null)
                throw new InvalidOperationException("Could not find Fatal with exception method of NLog");
        }

        public NLogLogger(string name)
        {
            Name = name;
            _logger = NLogBinding.GetCreateLoggerMethod().Invoke(name);
        }

        public void Trace(string logText, params object[] formatValues)
        {
            NLogBinding.GetTraceMethod().Invoke(_logger, FormatText(logText, formatValues));
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
            if (exception == null) return;

            NLogBinding.GetErrorWithExceptionMethod().Invoke(_logger, exception, exception.ToString());
        }

        public void Error(Exception exception, string logText)
        {
            NLogBinding.GetErrorWithExceptionMethod().Invoke(_logger, exception, logText);
        }

        public void Error(string logText, params object[] formatValues)
        {
            NLogBinding.GetErrorMethod().Invoke(_logger, FormatText(logText, formatValues));
        }

        public void Fatal(Exception exception)
        {
            if (exception == null) return;

            NLogBinding.GetFatalWithExceptionMethod().Invoke(_logger, exception, exception.ToString());
        }

        public void Fatal(Exception exception, string logText)
        {
            NLogBinding.GetFatalWithExceptionMethod().Invoke(_logger, exception, logText);
        }

        public void Fatal(string logText, params object[] formatValues)
        {
            NLogBinding.GetFatalMethod().Invoke(_logger, FormatText(logText, formatValues));
        }
    }
}
