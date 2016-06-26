using System;
using System.Collections.Generic;
using DediLib.IO;
using NUnit.Framework;

namespace DediLib.Tests.IO
{
    [TestFixture]
    public class TestExecutableRunner
    {
        [Test]
        public void StartAndWaitForExit_EchoStandardOutput_StandardOutputIsBeingCaptured()
        {
            var runner = new ExecutableRunner();

            string standardOutput, errorOutput;
            runner.StartAndWaitForExit("cmd", "/c echo message", TimeSpan.FromSeconds(1), out standardOutput, out errorOutput);

            Assert.AreEqual("message", standardOutput.Trim());
        }

        [Test]
        public void SetEnvironmentVariable_StartAndWaitForExitEchoVariable_ValueOfVariable()
        {
            var runner = new ExecutableRunner();

            runner.SetEnvironmentVariable("testname", "testvalue");

            string standardOutput, errorOutput;
            runner.StartAndWaitForExit("cmd", "/c echo %testname%", TimeSpan.FromSeconds(1), out standardOutput, out errorOutput);

            Assert.AreEqual("testvalue", standardOutput.Trim());
        }

        [Test]
        public void StartAndWaitForExit_EchoErrorOutput_ErrorOutputIsBeingCaptured()
        {
            var runner = new ExecutableRunner();

            string standardOutput, errorOutput;
            runner.StartAndWaitForExit("cmd", "/c echo message 1>&2", TimeSpan.FromSeconds(1), out standardOutput, out errorOutput);

            Assert.AreEqual("message", errorOutput.Trim());
        }

        [Test]
        public void StartAndWaitForExit_EchoStandardOutput_OnOutputLineFiringEvent()
        {
            var runner = new ExecutableRunner();

            var lines = new List<string>();
            runner.OnOutputLine += lines.Add;
            runner.StartAndWaitForExit("cmd", "/c echo message", TimeSpan.FromSeconds(1));

            Assert.Contains("message", lines);
        }

        [Test]
        public void StartAndWaitForExit_EchoErrorOutput_OnErrorLineFiringEvent()
        {
            var runner = new ExecutableRunner();

            var lines = new List<string>();
            runner.OnErrorLine += line => lines.Add(line.Trim());
            runner.StartAndWaitForExit("cmd", "/c echo message 1>&2", TimeSpan.FromSeconds(1));

            Assert.Contains("message", lines);
        }

        [Test]
        public void StartAndWaitForExit_PingWithTimeout_ThrowsTimeoutException()
        {
            var runner = new ExecutableRunner();
            Assert.Throws<TimeoutException>(() => runner.StartAndWaitForExit("ping", "-n 10 127.0.0.1", TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void StartAndWaitForExit_StartProcessThatDoesNothing_StandardOutputIsEmpty()
        {
            var runner = new ExecutableRunner();

            string standardOutput, errorOutput;
            runner.StartAndWaitForExit("cmd", "/c rem", out standardOutput, out errorOutput);

            Assert.IsTrue(String.IsNullOrWhiteSpace(standardOutput));
        }

        [Test]
        public void StartAndWaitForExit_StartProcessThatDoesNothing_ErrorOutputIsEmpty()
        {
            var runner = new ExecutableRunner();

            string standardOutput, errorOutput;
            runner.StartAndWaitForExit("cmd", "/c rem", out standardOutput, out errorOutput);

            Assert.IsTrue(String.IsNullOrWhiteSpace(errorOutput));
        }
    }
}
