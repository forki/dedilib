using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DediLib.Logging;

namespace DediLib.IO
{
    /// <summary>
    /// Executes a process with timeout and captures the output
    /// </summary>
    public class ExecutableRunner
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger();

        private readonly string _workingDirectory;

        public bool CreateNoWindow { get; set; }
        public bool UseShellExecute { get; set; }

        public event Action<string> OnOutputLine = line => { };
        public event Action<string> OnErrorLine = line => { };

        private readonly Dictionary<string, string> _envVars = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public ExecutableRunner()
            : this(null)
        {
        }

        public ExecutableRunner(string workingDirectory)
        {
            _workingDirectory = workingDirectory;

            CreateNoWindow = true;
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            _envVars[name] = value;
        }

        public int StartAndWaitForExit(string commandFileName, string arguments)
        {
            string standardOutput, errorOutput;
            return StartAndWaitForExit(commandFileName, arguments, TimeSpan.FromMilliseconds(-1), out standardOutput, out errorOutput);
        }

        public int StartAndWaitForExit(string commandFileName, string arguments, TimeSpan timeout)
        {
            string standardOutput, errorOutput;
            return StartAndWaitForExit(commandFileName, arguments, timeout, out standardOutput, out errorOutput);
        }

        public int StartAndWaitForExit(string commandFileName, string arguments, out string standardOutput, out string errorOutput)
        {
            return StartAndWaitForExit(commandFileName, arguments, TimeSpan.FromMilliseconds(-1), out standardOutput, out errorOutput);
        }

        public int StartAndWaitForExit(string commandFileName, string arguments, TimeSpan timeout, out string standardOutput, out string errorOutput)
        {
            var process = CreateProcess(commandFileName, arguments);

            var sbErrorData = new StringBuilder();
            var sbOutputData = new StringBuilder();

            Logger.Info("Execute: {0} {1}", commandFileName, arguments);

            if (!process.Start()) throw new InvalidOperationException("Could not start process");

            var tasks = new[]
            {
                BeginReadOutput(process.StandardOutput, sbOutputData, line => OnOutputLine(line)),
                BeginReadOutput(process.StandardError, sbErrorData, line => OnErrorLine(line))
            };

            if (!process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                Logger.Warning("Process not responding: {0} {1}", commandFileName, arguments);

                try
                {
                    Task.Factory.StartNew(process.Kill, TaskCreationOptions.LongRunning).Wait(5000);
                }
                catch
                {
                    Logger.Warning("Process kill failed: {0} {1}", commandFileName, arguments);
                }
                throw new TimeoutException(String.Format("Process didn't respond within {0}", timeout));
            }

            var exitCode = process.ExitCode;
            process.Close();

            Task.WaitAll(tasks);

            standardOutput = sbOutputData.ToString();
            errorOutput = sbErrorData.ToString();

            return exitCode;
        }

        private Task BeginReadOutput(StreamReader reader, StringBuilder sb, Action<string> lineEvent)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (sb == null) throw new ArgumentNullException("sb");

            return Task.Factory.StartNew(() =>
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    if (lineEvent != null) lineEvent(line);
                    sb.AppendLine(line);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public Process Start(string commandFileName, string arguments)
        {
            var process = CreateProcess(commandFileName, arguments);

            Logger.Info("Execute: {0} {1}", commandFileName, arguments);

            if (!process.Start()) throw new InvalidOperationException("Could not start process");

            return process;
        }

        private Process CreateProcess(string commandFileName, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = commandFileName,
                Arguments = arguments,
                CreateNoWindow = CreateNoWindow,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = UseShellExecute
            };

            foreach (var envVar in _envVars)
            {
                startInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }

            if (_workingDirectory != null)
            {
                startInfo.WorkingDirectory = _workingDirectory;
            }

            var process = new Process
            {
                StartInfo = startInfo
            };

            return process;
        }
    }
}