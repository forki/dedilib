using System;

namespace DediLib.Collections
{
    public interface ITimedDictionary
    {
        TimeSpan CleanUpPeriod { get; }
        void CleanUp();
    }
}
