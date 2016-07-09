using DediLib.Logging;
using NUnit.Framework;

namespace DediLib.Tests.Logging
{
    [TestFixture]
    public class TestLogger
    {
        [Test]
        public void GetCurrentClassLogger()
        {
            var logger = Logger.GetCurrentClassLogger();

            Assert.That(logger.Name, Is.EqualTo(typeof(TestLogger).FullName));
        }
    }
}
