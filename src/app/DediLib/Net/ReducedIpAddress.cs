using System;
using System.Net;
using System.Net.Sockets;
using DediLib.Crypto;

namespace DediLib.Net
{
    public class ReducedIpAddress
    {
        private static readonly byte[] LocalIpAddress = GetLocalAddressBytes();

        public readonly IPAddress IpAddress;

        public readonly ulong ReducedIpValue;

        public ReducedIpAddress(IPAddress ipAddress)
        {
            IpAddress = ipAddress;
            ReducedIpValue = CalculateReducedValue(ipAddress);
        }

        private static ulong CalculateReducedValue(IPAddress ipAddress)
        {
            if (ipAddress == null)
            {
                return 0;
            }

            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }

            var originalBytes = ipAddress.GetAddressBytes();
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(originalBytes);
            }
            var bytesToUse = originalBytes;
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                if (originalBytes[0] == 127)
                {
                    bytesToUse = LocalIpAddress;
                }
            }

            var sha1 = SHA1Pool.Aquire();
            var hash = sha1.ComputeHash(bytesToUse, 0, Math.Min(8, bytesToUse.Length));
            SHA1Pool.Release(sha1);
            return BitConverter.ToUInt64(hash, 0);
        }

        public override int GetHashCode()
        {
            return (int)ReducedIpValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var reducedIp = obj as ReducedIpAddress;
            return reducedIp?.ReducedIpValue == ReducedIpValue;
        }

        public override string ToString()
        {
            return ReducedIpValue.ToString();
        }

        private static byte[] GetLocalAddressBytes()
        {
            var bytes = IPAddress.Parse("::1").GetAddressBytes();
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}
