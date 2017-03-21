using System;
using System.Security.Cryptography;

namespace DediLib
{
    /// <summary>
    /// Creates cryptographic random numbers
    /// </summary>
    public static class RandomNumber
    {
        private static readonly RandomNumberGenerator Rng = RNGCryptoServiceProvider.Create();

        public static int Next()
        {
            return Next(0, int.MaxValue);
        }

        public static int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
            {
                throw new ArgumentOutOfRangeException($"maxValue must be larger than minValue (minValue: {minValue}, maxValue: {maxValue})");
            }

            var randomBytes = new byte[sizeof(long)];
            Rng.GetBytes(randomBytes);

            var randomLong = Math.Abs(BitConverter.ToInt64(randomBytes, 0));
            var diff = maxValue - minValue;
            return (int)(randomLong % diff + minValue);
        }
    }
}
