using System;

namespace DediLib
{
    public interface ITimeSource
    {
        DateTime UtcNow { get; }
    }
}
