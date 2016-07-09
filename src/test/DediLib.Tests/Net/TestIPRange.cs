using System;
using System.Net;
using System.Numerics;
using DediLib.Net;
using NUnit.Framework;

namespace DediLib.Tests.Net
{
    public class TestIPRange
    {
        [Test]
        public void Parse_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IPRange.Parse(null));
        }

        [Test]
        public void Parse_Empty_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => IPRange.Parse(string.Empty));
        }

        [Test]
        public void Parse_SingleIP_FromToSame()
        {
            var ip = IPAddress.Parse("1.2.3.4");
            var range = IPRange.Parse(ip.ToString());
            Assert.AreEqual(ip, range.From);
            Assert.AreEqual(ip, range.To);
        }

        [Test]
        public void Parse_FromLargerThanTo_ReverseFromAndTo()
        {
            var from = IPAddress.Parse("1.2.3.5");
            var to = IPAddress.Parse("1.2.3.4");
            var range = new IPRange(from, to);
            Assert.AreEqual(to, range.From);
            Assert.AreEqual(from, range.To);
        }

        [Test]
        public void Parse_InvalidIP_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => IPRange.Parse("257.2.3.4"));
        }

        [TestCase("1.1.1.1", "1.1.1.1", "1.1.1.1", "1.1.1.1")]
        [TestCase("1.1.1.1", "1.1.1.100", "1.1.1.50", "1.1.1.150")]
        [TestCase("1.1.1.1", "1.1.1.100", "1.1.1.100", "1.1.1.200")]
        [TestCase("1.1.1.1", "1.1.1.100", "1.1.1.50", "1.1.1.60")]
        [TestCase("1.1.1.50", "1.1.1.150", "1.1.1.1", "1.1.1.100")]
        [TestCase("1.1.1.100", "1.1.1.200", "1.1.1.1", "1.1.1.100")]
        [TestCase("1.1.1.50", "1.1.1.60", "1.1.1.1", "1.1.1.100")]
        public void Overlaps_true(string from1, string to1, string from2, string to2)
        {
            var ipRange1 = new IPRange(IPAddress.Parse(from1), IPAddress.Parse(to1));
            var ipRange2 = new IPRange(IPAddress.Parse(from2), IPAddress.Parse(to2));

            Assert.AreEqual(true, ipRange1.Overlaps(ipRange2));
        }

        [TestCase("1.1.1.1", "1.1.1.100", "1.1.1.101", "1.1.1.200")]
        [TestCase("1.1.1.101", "1.1.1.200", "1.1.1.1", "1.1.1.100")]
        public void Overlaps_false(string from1, string to1, string from2, string to2)
        {
            var ipRange1 = new IPRange(IPAddress.Parse(from1), IPAddress.Parse(to1));
            var ipRange2 = new IPRange(IPAddress.Parse(from2), IPAddress.Parse(to2));

            Assert.AreEqual(false, ipRange1.Overlaps(ipRange2));
        }

        [TestCase("192.168.1.1", "192.168.1.1", "192.168.1.1")]
        [TestCase("192.168.1.1/32", "192.168.1.1", "192.168.1.1")]
        [TestCase("192.168.1.1/31", "192.168.1.0", "192.168.1.1")]
        [TestCase("192.168.1.1/24", "192.168.1.0", "192.168.1.255")]
        public void Parse(string network, string from, string to)
        {
            var fromIp = IPAddress.Parse(from);
            var toIp = IPAddress.Parse(to);
            var range = IPRange.Parse(network);
            Assert.AreEqual(fromIp, range.From);
            Assert.AreEqual(toIp, range.To);
        }

        [TestCase("192.168.1.1", "192.168.1.1", "192.168.1.1/32")]
        [TestCase("192.168.1.0", "192.168.1.1", "192.168.1.0/31")]
        [TestCase("192.168.1.0", "192.168.1.255", "192.168.1.0/24")]
        [TestCase("46.51.216.0", "46.51.223.255", "46.51.216.0/21")]
        [TestCase("2604:A880::", "2604:A880:FFFF:FFFF:FFFF:FFFF:FFFF:FFFF", "2604:A880::/32")]
        public void GetNetwork(string from, string to, string expectedNetwork)
        {
            var fromIp = IPAddress.Parse(from);
            var toIp = IPAddress.Parse(to);
            var network = new IPRange(fromIp, toIp).GetNetwork();
            Assert.AreEqual(expectedNetwork, network.ToUpperInvariant());
        }

        [TestCase("127.0.0.1", 1)]
        [TestCase("127.0.0.1/32", 1)]
        [TestCase("127.0.0.1/31", 2)]
        [TestCase("127.0.0.1/24", 256)]
        [TestCase("127.0.0.1/16", 65536)]
        public void Count(string network, long count)
        {
            Assert.AreEqual((ulong)count, IPRange.Parse(network).Count);
        }

        [TestCase("127.0.0.1", 1)]
        [TestCase("127.0.0.1/24", 256)]
        public void BigCount(string network, long count)
        {
            Assert.AreEqual(new BigInteger(count), IPRange.Parse(network).BigCount);
        }
    }
}
