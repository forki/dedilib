using System;
using System.Diagnostics;

namespace DediLib.Logging
{
    public static class Logger
    {
        private static Func<Type, ILogger> _mapping = type => new NullLogger(type.FullName);
        public static Func<Type, ILogger> Mapping
        {
            get => _mapping;
            set { _mapping = value ?? (type => new NullLogger(type.FullName)); }
        }

        public static ILogger GetLogger()
        {
            return GetCurrentClassLogger();
        }
            
        public static ILogger GetLogger(Type type)
        {
            return _mapping(type);
        }

        public static ILogger GetCurrentClassLogger()
        {
            Type declaringType;
            var framesToSkip = 1;

            do
            {
                var frame = new StackFrame(framesToSkip, false);
                var method = frame.GetMethod();
                declaringType = method.DeclaringType;

                framesToSkip++;
            } while (declaringType != null && declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return GetLogger(declaringType);
        }
    }
}
