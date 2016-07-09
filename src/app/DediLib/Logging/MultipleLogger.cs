using System;
using System.Linq;

namespace DediLib.Logging
{
    public class MultipleLogger : ILogger
    {
        private readonly ILogger[] _loggers;

        public string Name { get; }

        public ITimeSource TimeSource
        {
            get { return _loggers[0].TimeSource; }
            set
            {
                if (value == null) value = new DefaultTimeSource();
                foreach (var logger in _loggers)
                {
                    logger.TimeSource = value;
                }
            }
        }

        public MultipleLogger(ILogger logger, params ILogger[] loggers)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _loggers = (new[] { logger }.Union(loggers ?? new ILogger[0]).Where(x => x != null)).ToArray();
        }

        public MultipleLogger(ITimeSource timeSource, ILogger logger, params ILogger[] loggers)
            : this(null, timeSource, logger, loggers)
        {
        }

        public MultipleLogger(string name, ITimeSource timeSource, ILogger logger, params ILogger[] loggers)
            : this(logger, loggers)
        {
            if (timeSource == null) throw new ArgumentNullException(nameof(timeSource));
            Name = name;
            TimeSource = timeSource;
        }

        public void Debug(string logText, params object[] formatValues)
        {
            foreach (var logger in _loggers)
                logger.Debug(logText, formatValues);
        }

        public void Info(string logText, params object[] formatValues)
        {
            foreach (var logger in _loggers)
                logger.Info(logText, formatValues);
        }

        public void Warning(string logText, params object[] formatValues)
        {
            foreach (var logger in _loggers)
                logger.Warning(logText, formatValues);
        }

        public void Error(Exception exception)
        {
            foreach (var logger in _loggers)
                logger.Error(exception);
        }

        public void Error(Exception exception, string logText)
        {
            foreach (var logger in _loggers)
                logger.Error(exception, logText);
        }

        public void Error(string logText, params object[] formatValues)
        {
            foreach (var logger in _loggers)
                logger.Error(logText, formatValues);
        }
    }
}
