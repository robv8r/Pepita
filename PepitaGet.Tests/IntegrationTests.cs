using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace PepitaGet.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void GetPackages()
        {
            var projectDir = Path.Combine(Path.Combine(Environment.CurrentDirectory, "../../../"), "PepitaGetSample");
            var solutionPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../FakeSolution"));
            if (Directory.Exists(solutionPath))
            {
                Directory.Delete(solutionPath, true);
            }

            new Runner
                {
                    ProjectDirectory = projectDir,
                    WriteInfo = Console.WriteLine,
                    SolutionDirectory = solutionPath 
                }.Execute();
        }

        [Test]
        public void Perf()
        {
            var projectDir = Path.Combine(Path.Combine(Environment.CurrentDirectory, "../../../"), "PepitaGetSample");
            var solutionPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../FakeSolution"));
            if (Directory.Exists(solutionPath))
            {
                Directory.Delete(solutionPath,true);
            }

            var startNew = Stopwatch.StartNew();

            new Runner
            {
                ProjectDirectory = projectDir,
                WriteInfo = Console.WriteLine,
                SolutionDirectory = solutionPath
            }.Execute();
            Debug.WriteLine(startNew.ElapsedMilliseconds);
        }
        [Test]
        public void MultiThreaded()
        {
            var projectDir = Path.Combine(Path.Combine(Environment.CurrentDirectory, "../../../"), "PepitaGetSample");
            var solutionPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../FakeSolution"));
            if (Directory.Exists(solutionPath))
            {
                Directory.Delete(solutionPath,true);
            }


            var thread1 = new Thread(() => Run(projectDir, solutionPath));
            var thread2 = new Thread(() => Run(projectDir, solutionPath));
            var thread3 = new Thread(() => Run(projectDir, solutionPath));
            var thread4 = new Thread(() => Run(projectDir, solutionPath));
            var thread5 = new Thread(() => Run(projectDir, solutionPath));
            var thread6 = new Thread(() => Run(projectDir, solutionPath));
            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();
            thread5.Start();
            thread6.Start();
            thread1.Join();
            thread2.Join();
            thread3.Join();
            thread4.Join();
            thread5.Join();
            thread6.Join();

        }

        static void Run(string projectDir, string solutionPath)
        {
            new Runner
                {
                    ProjectDirectory = projectDir,
                    WriteInfo = Console.WriteLine,
                    SolutionDirectory = solutionPath
                }.Execute();
        }
    }
}