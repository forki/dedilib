using System;
using DediLib.Configuration;
using NUnit.Framework;

namespace DediLib.Tests.Configuration
{
    [TestFixture]
    public class TestConnectionStringBuilder
    {
        [Test]
        public void With_NameHasIllegalCharacters_Throws()
        {
            var builder = ConnectionStringBuilder.New();
            Assert.Throws<ArgumentException>(() => builder.With("name with illegal chars #!@", ""));
        }

        [Test]
        public void With_NameDoesNotStartWithLetter_Throws()
        {
            var builder = ConnectionStringBuilder.New();
            Assert.Throws<ArgumentException>(() => builder.With("1name", ""));
        }

        [Test]
        public void With_ValueHasIllegalCharacters_Throws()
        {
            var builder = ConnectionStringBuilder.New();
            Assert.Throws<ArgumentException>(() => builder.With("name", "value with illegal chars \r\n"));
        }

        [TestCase("1+1=2")]
        [TestCase(" precedingspace")]
        [TestCase("spaceattheend ")]
        [TestCase("semicolon;value")]
        public void With_ValueThatNeedsToBeEscaped_ConnectionString(string value)
        {
            var builder = ConnectionStringBuilder.New();
            builder.With("name", value);

            Assert.That(builder.Build(), Is.EqualTo($"name='{value}'"));
        }

        [Test]
        public void Build_SingleName_ConnectionString()
        {
            var builder = ConnectionStringBuilder.New();
            builder.With("name", null);

            Assert.That(builder.Build(), Is.EqualTo("name"));
        }

        [Test]
        public void Build_SingleNameAndStringValue_ConnectionString()
        {
            var builder = ConnectionStringBuilder.New();
            builder.With("name", "value");

            Assert.That(builder.Build(), Is.EqualTo("name=value"));
        }

        [Test]
        public void Build_SingleNameAndBoolValue_ConnectionString()
        {
            var builder = ConnectionStringBuilder.New();
            builder.With("name", true);

            Assert.That(builder.Build(), Is.EqualTo("name=True"));
        }

        [Test]
        public void Build_OverrideExistingNameAndValue_ConnectionString()
        {
            var builder =
                ConnectionStringBuilder.New()
                .With("name", "value")
                .With("Name", "VALUE");

            Assert.That(builder.Build(), Is.EqualTo("name=VALUE"));
        }

        [Test]
        public void Build_MultipleNamesAndValues_ConnectionString()
        {
            var builder =
                ConnectionStringBuilder.New()
                .With("name1", "value1")
                .With("name2", "value2");

            Assert.That(builder.Build(), Is.EqualTo("name1=value1;name2=value2"));
        }

        [TestCase("name1", "name1")]
        [TestCase("name1;", "name1")]
        [TestCase("name1;;", "name1")]
        [TestCase("name1; ;", "name1")]
        [TestCase("name1; name2;", "name1;name2")]
        [TestCase("name1=value1; name2=value2;", "name1=value1;name2=value2")]
        [TestCase("name1 = value1; name2 = value2", "name1=value1;name2=value2")]
        [TestCase("name1 = 'value;1'; name2 = value2", "name1='value;1';name2=value2")]
        public void Parse_Build_ConnectionString(string connectionString, string buildConnectionString)
        {
            var builder =
                ConnectionStringBuilder.Parse(connectionString);

            Assert.That(builder.Build(), Is.EqualTo(buildConnectionString));
        }
    }
}
