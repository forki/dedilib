using System;

namespace DediLib.Collections
{
    [Serializable]
    public class TimedValue<TValue>
    {
        public DateTime LastAccessUtc { get; set; }
        public TimeSpan Expiry { get; set; }
        public TValue Value { get; set; }

        public TimedValue(TValue value, TimeSpan expiry)
        {
            Value = value;
            Expiry = expiry;
            LastAccessUtc = DateTime.UtcNow;
        }

        public void UpdateAccessTime()
        {
            LastAccessUtc = DateTime.UtcNow;
        }
    }
}
