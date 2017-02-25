using System.Diagnostics;
using System.Linq;
using System.Net;
using DediLib.Net;
using NUnit.Framework;

namespace DediLib.Tests.Net
{
    [TestFixture]
    public class TestReducedIpAddress
    {
        [Test]
        public void If_ip_is_null_Then_reduced_value_is_zero()
        {
            var reducedIp = new ReducedIpAddress(null);

            Assert.That(reducedIp.ReducedIpValue, Is.EqualTo(0));
        }

        [TestCase("127.0.0.1")]
        [TestCase("127.1.0.1")]
        [TestCase("::1")]
        public void If_ip_is_localhost_Then_reduced_values_are_same(string ipAddress)
        {
            var reducedIp = new ReducedIpAddress(IPAddress.Parse(ipAddress));

            Assert.That(reducedIp.ReducedIpValue, Is.EqualTo(1328304962299559429UL));
        }

        [Test]
        public void If_ipv4_is_mapped_to_ipv6_Then_reduced_values_are_same()
        {
            var ipv4 = IPAddress.Parse("142.52.77.134");
            var ipv6 = ipv4.MapToIPv6();

            var reducedIpv4 = new ReducedIpAddress(ipv4);
            var reducedIpv6 = new ReducedIpAddress(ipv6);

            Assert.That(reducedIpv6.ReducedIpValue, Is.EqualTo(reducedIpv4.ReducedIpValue));
        }

        [TestCase("2a02:8108:4740:780c:594f:4602:31ba:8e2e")]
        [TestCase("2a02:8108:4740:780c:594f:4602:31ba:9999")]
        [TestCase("2a02:8108:4740:780c:594f:4602:8888:9999")]
        [TestCase("2a02:8108:4740:780c:594f:7777:8888:9999")]
        [TestCase("2a02:8108:4740:780c:6666:7777:8888:9999")]
        [TestCase("2a02:8108:4740:780c::")]
        public void If_ipv6_has_same_64_bit_subnet_Then_reduced_values_are_same(string ipAddress)
        {
            var reducedIp = new ReducedIpAddress(IPAddress.Parse(ipAddress));

            Assert.That(reducedIp.ReducedIpValue, Is.EqualTo(9120338691296830918));
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark_ReducedIpAddress()
        {
            const int count = 1000000;
            var ips = new[] { IPAddress.Parse("2a02:8108:4740:780c::"), IPAddress.Loopback };
            var tasks = Enumerable.Range(0, count).Select(x => new ReducedIpAddress(ips[x % ips.Length]));

            var sw = Stopwatch.StartNew();
            var list = tasks.ToList();
            sw.Stop();

            Assert.That(list.Select(x => x.ReducedIpValue).Distinct().Count(), Is.EqualTo(ips.Length));

            var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{sw.Elapsed} ({opsPerSec:N0} ops/sec)");
        }
    }
}
