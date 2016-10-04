using System;
using System.Net;
using System.Numerics;

namespace DediLib.Net
{
    public static class IPAddressHelper
    {
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (subnetMask == null) throw new ArgumentNullException(nameof(subnetMask));

            var addressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (addressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("IP address length does not match subnet mask length");

            var broadcastAddress = new byte[addressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(addressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (subnetMask == null) throw new ArgumentNullException(nameof(subnetMask));

            var addressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (addressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("IP address length does not match subnet mask length");

            var broadcastAddress = new byte[addressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(addressBytes[i] & subnetMaskBytes[i]);
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool AreInSameSubnet(IPAddress first, IPAddress second, IPAddress subnetMask)
        {
            var network1 = GetNetworkAddress(first, subnetMask);
            var network2 = GetNetworkAddress(second, subnetMask);

            return network1.Equals(network2);
        }

        public static IPAddress CreateSubnetMaskIPv4(byte cidr)
        {
            const byte maskLength = 32;
            if (cidr > maskLength) throw new ArgumentOutOfRangeException(nameof(cidr), cidr, "CIDR network prefix cannot be larger than 32 for IPv4");

            var zeroBits = maskLength - cidr;
            var result = uint.MaxValue;
            result &= (uint) ((((ulong) 0x1 << cidr) - 1) << zeroBits);
            result = (uint)IPAddress.HostToNetworkOrder((int)result);
            return new IPAddress(BitConverter.GetBytes(result));
        }

        public static IPAddress CreateSubnetMaskIPv6(byte cidr)
        {
            const byte maskLength = 128;
            if (cidr > maskLength) throw new ArgumentOutOfRangeException(nameof(cidr), cidr, "CIDR network prefix cannot be larger than 128 for IPv6");

            var bMaskData = new byte[maskLength / 8];
            for (byte i = 0; i < maskLength; i++)
            {
                var index = (maskLength - 1 - i) / 8 * 8 + i % 8;
                var bitValue = i >= maskLength - cidr ? (byte)1 : (byte)0;
                bMaskData[index / 8] |= (byte)(bitValue << (i%8));
            }

            return new IPAddress(bMaskData);
        }

        public static BigInteger BigIntegerFromIpAddress(IPAddress ipAddress)
        {
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));

            var addressBytes = ipAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(addressBytes);

            var paddedAddressBytes = new byte[addressBytes.Length + 1];
            Array.Copy(addressBytes, paddedAddressBytes, addressBytes.Length);
            return new BigInteger(paddedAddressBytes);
        }
    }
}
