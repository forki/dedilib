using System;
using System.Reflection;

namespace DediLib.Logging
{
    public static class Logger
    {
        private static Func<Type, ILogger> _mapping = type => new NullLogger();
        public static Func<Type, ILogger> Mapping
        {
            get { return _mapping; }
            set { _mapping = value ?? (type => new NullLogger()); }
        }

        public static ILogger GetLogger()
        {
            return _mapping(MethodBase.GetCurrentMethod().DeclaringType);
        }
            
        public static ILogger GetLogger(Type type)
        {
            return _mapping(type);
        }
    }
}
