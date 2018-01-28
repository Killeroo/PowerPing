using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

using PowerPing;

using NUnit.Framework;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace PowerPing.Tests
{
    [TestFixture]
    public class UnitTests
    {
        TextWriter m_normalOutput;
        StringWriter m_testingConsole;
        StringBuilder m_testingSB;

        #region test fixture setup/teardown methods
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            // Set current folder to testing folder
            string assemblyCodeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            string dirName = Path.GetDirectoryName(assemblyCodeBase);

            if (dirName.StartsWith("file:\\"))
                dirName = dirName.Substring(6);

            Environment.CurrentDirectory = dirName;

            m_testingSB = new StringBuilder();
            m_testingConsole = new StringWriter(m_testingSB);
            m_normalOutput = System.Console.Out;
            System.Console.SetOut(m_testingConsole);
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            System.Console.SetOut(m_normalOutput);
        }
        #endregion

        [SetUp]
        public void SetUp()
        {
            // clear string builder
            m_testingSB.Remove(0, m_testingSB.Length);
        }

        [TearDown]
        public void TearDown()
        {
            // Verbose output in console
            m_normalOutput.Write(m_testingSB.ToString());
        }

        [Test]
        public void ShowCmdHelpIfNoArguments()
        {
            // Check exit is normal
            Assert.AreEqual(0, StartConsoleApplication(""));

            // Check that help information shown correctly.
            Assert.IsTrue(m_testingSB.ToString().Contains("Advanced ping utility"));//Program.COMMAND_LINE_HELP));
        }

        /// &lt;span class="code-SummaryComment">&lt;summary>&lt;/span>
        /// Starts the console application.
        /// &lt;span class="code-SummaryComment">&lt;/summary>&lt;/span>
        /// &lt;span class="code-SummaryComment">&lt;param name="arguments">The arguments for console application. &lt;/span>
        /// Specify empty string to run with no arguments&lt;/param />
        /// &lt;span class="code-SummaryComment">&lt;returns>exit code&lt;/returns>&lt;/span>
        private int StartConsoleApplication(string arguments)
        {
            // Initialize process here
            Process proc = new Process();
            proc.StartInfo.FileName = "PowerPing.exe";
            // add arguments as whole string
            proc.StartInfo.Arguments = arguments;

            // use it to start from testing environment
            proc.StartInfo.UseShellExecute = false;

            // redirect outputs to have it in testing console
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            // set working directory
            proc.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

            // start and wait for exit
            proc.Start();
            proc.WaitForExit();

            // get output to testing console.
            System.Console.WriteLine(proc.StandardOutput.ReadToEnd());
            System.Console.Write(proc.StandardError.ReadToEnd());

            // return exit code
            return proc.ExitCode;
        }

    }
}
