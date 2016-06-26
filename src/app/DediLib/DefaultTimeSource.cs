using System;

namespace DediLib
{
    public class DefaultTimeSource : ITimeSource
    {
        public DateTime UtcNow { get { return DateTime.UtcNow; } }
    }
}
