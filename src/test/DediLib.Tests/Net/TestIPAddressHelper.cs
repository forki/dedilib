using System.Net;
using DediLib.Net;
using NUnit.Framework;

namespace DediLib.Tests.Net
{
    public class TestIPAddressHelper
    {
        [TestCase("192.168.1.1", "255.255.255.0", "192.168.1.255")]
        [TestCase("192.168.1.1", "255.255.0.0", "192.168.255.255")]
        [TestCase("192.168.1.1", "255.0.0.0", "192.255.255.255")]
        [TestCase("192.168.1.1", "255.255.255.192", "192.168.1.63")]
        [TestCase("192.168.1.65", "255.255.255.192", "192.168.1.127")]
        public void GetBroadcastAddress(string address, string subnetMask, string expected)
        {
            Assert.AreEqual(IPAddress.Parse(expected), IPAddressHelper.GetBroadcastAddress(IPAddress.Parse(address), IPAddress.Parse(subnetMask)));
        }

        [TestCase("192.168.1.1", "255.255.255.0", "192.168.1.0")]
        [TestCase("192.168.1.1", "255.255.0.0", "192.168.0.0")]
        [TestCase("192.168.1.1", "255.0.0.0", "192.0.0.0")]
        [TestCase("192.168.1.1", "255.255.255.192", "192.168.1.0")]
        [TestCase("192.168.1.65", "255.255.255.192", "192.168.1.64")]
        public void GetNetworkAddress(string address, string subnetMask, string expected)
        {
            Assert.AreEqual(IPAddress.Parse(expected), IPAddressHelper.GetNetworkAddress(IPAddress.Parse(address), IPAddress.Parse(subnetMask)));
        }

        [TestCase("192.168.1.1", "192.168.1.2", "255.255.255.0")]
        public void AreInSameSubnet_True(string address1, string address2, string subnetMask)
        {
            Assert.True(IPAddressHelper.AreInSameSubnet(IPAddress.Parse(address1), IPAddress.Parse(address2), IPAddress.Parse(subnetMask)));
        }

        [TestCase("192.168.1.1", "192.168.2.1", "255.255.255.0")]
        public void AreInSameSubnet_False(string address1, string address2, string subnetMask)
        {
            Assert.False(IPAddressHelper.AreInSameSubnet(IPAddress.Parse(address1), IPAddress.Parse(address2), IPAddress.Parse(subnetMask)));
        }

        [TestCase(0, "0.0.0.0")]
        [TestCase(1, "128.0.0.0")]
        [TestCase(24, "255.255.255.0")]
        [TestCase(25, "255.255.255.128")]
        [TestCase(31, "255.255.255.254")]
        [TestCase(32, "255.255.255.255")]
        public void CreateSubnetMaskIPv4(int cidr, string subnetMask)
        {
            Assert.AreEqual(subnetMask, IPAddressHelper.CreateSubnetMaskIPv4((byte)cidr).ToString());
        }

        [TestCase(0, "::")]
        [TestCase(1, "8000::")]
        [TestCase(24, "ffff:ff00::")]
        [TestCase(25, "ffff:ff80::")]
        [TestCase(32, "ffff:ffff::")]
        [TestCase(64, "ffff:ffff:ffff:ffff::")]
        [TestCase(128, "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        public void CreateSubnetMaskIPv6(int cidr, string subnetMask)
        {
            Assert.AreEqual(subnetMask, IPAddressHelper.CreateSubnetMaskIPv6((byte)cidr).ToString());
        }
    }
}
