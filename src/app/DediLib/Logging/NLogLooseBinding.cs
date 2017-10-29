using System;
using System.Linq;
using System.Reflection;

namespace DediLib.Logging
{
    class NLogLooseBinding
    {
        private Action<object, string> GetValueMethod(string name)
        {
            var methodInfo =
                GetLoggerType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(x => x.IsGenericMethod && x.Name == name && x.GetParameters().Length == 1);

            if (methodInfo == null)
                throw new InvalidOperationException("Could not find method: " + name);

            methodInfo = methodInfo.MakeGenericMethod(typeof(string));

            return (instance, logText) => methodInfo.Invoke(instance, new object[] { logText });
        }

        public Func<string, object> GetCreateLoggerMethod()
        {
            var methodInfos = GetLogManagerType()
                .GetMethods(BindingFlags.Static | BindingFlags.Public);

            var methodInfo = methodInfos
                    .FirstOrDefault(x => x.Name == "GetLogger" && x.GetParameters().Length == 1);

            if (methodInfo == null)
                throw new InvalidOperationException("Could not find 'GetLogger' method");

            return name => methodInfo.Invoke(null, new object[] { name ?? "" });
        }

        private Type _logger;
        private Type GetLoggerType()
        {
            return _logger = _logger ?? GetNLogAssembly().GetExportedTypes().Single(x => x.Name == "Logger");
        }

        private Type _logManagerType;
        private Type GetLogManagerType()
        {
            return _logManagerType = _logManagerType ?? GetNLogAssembly().GetExportedTypes().Single(x => x.Name == "LogManager");
        }

        private Assembly _assembly;
        private Assembly GetNLogAssembly()
        {
            if ((_assembly = _assembly ?? TryGetNLogAssembly()) == null)
                throw new InvalidOperationException("Could not find NLog assembly, please reference and initialize NLog first");
            return _assembly;
        }

        private Action<object, string> _trace;
        public Action<object, string> GetTraceMethod()
        {
            return _trace = _trace ?? GetValueMethod("Trace");
        }

        private Action<object, string> _debug;
        public Action<object, string> GetDebugMethod()
        {
            return _debug = _debug ?? GetValueMethod("Debug");
        }

        private Action<object, string> _info;
        public Action<object, string> GetInfoMethod()
        {
            return _info = _info ?? GetValueMethod("Info");
        }

        private Action<object, string> _warn;
        public Action<object, string> GetWarnMethod()
        {
            return _warn = _warn ?? GetValueMethod("Warn");
        }

        private Action<object, string> _error;
        public Action<object, string> GetErrorMethod()
        {
            return _error = _error ?? GetValueMethod("Error");
        }

        private Action<object, Exception, string> _errorWithException;
        public Action<object, Exception, string> GetErrorWithExceptionMethod()
        {
            if (_errorWithException != null)
                return _errorWithException;

            var methodInfo =
                GetLoggerType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.Name == "Error")
                    .FirstOrDefault(x => x.GetParameters().Length == 3
                                         && x.GetParameters()[0].ParameterType == typeof(Exception)
                                         && x.GetParameters()[1].ParameterType == typeof(string));

            if (methodInfo == null)
                throw new InvalidOperationException("Could not find method: Error(Exception)");

            _errorWithException = (instance, exception, message) => methodInfo.Invoke(instance, new[] { exception, message, (object)null });
            return _errorWithException;
        }

        private Action<object, string> _fatal;
        public Action<object, string> GetFatalMethod()
        {
            return _fatal = _fatal ?? GetValueMethod("Fatal");
        }

        private Action<object, Exception, string> _fatalWithException;
        public Action<object, Exception, string> GetFatalWithExceptionMethod()
        {
            if (_fatalWithException != null)
                return _fatalWithException;

            var methodInfo =
                GetLoggerType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.Name == "Fatal")
                    .FirstOrDefault(x => x.GetParameters().Length == 3
                                         && x.GetParameters()[0].ParameterType == typeof(Exception)
                                         && x.GetParameters()[1].ParameterType == typeof(string));

            if (methodInfo == null)
                throw new InvalidOperationException("Could not find method: Fatal(Exception)");

            _fatalWithException = (instance, exception, message) => methodInfo.Invoke(instance, new[] { exception, message, (object)null });
            return _fatalWithException;
        }
        private Assembly TryGetNLogAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = assemblies.FirstOrDefault(a => !a.IsDynamic && a.FullName.StartsWith("NLog, "));
            return assembly;
        }
    }
}