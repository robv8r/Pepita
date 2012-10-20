using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace PepitaGet.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void GetPackages()
        {
            var projectPath = Path.Combine(Environment.CurrentDirectory, "../../FakeSolution/FakeProject");
            var packagesPath = Path.Combine(Environment.CurrentDirectory, "../../FakeSolution/Packages");
            var solutionPath = Path.Combine(Environment.CurrentDirectory, "../../FakeSolution");

            if (Directory.Exists(packagesPath))
            {
                Directory.Delete(packagesPath, true);
            }
            Directory.CreateDirectory(packagesPath);
            new Runner
                {
                    ProjectDirectory = projectPath,
                    WriteInfo = Console.WriteLine,
                    SolutionDirectory = solutionPath 
                }.Execute();
        }

        [Test]
        [Ignore]
        public void Perf()
        {
            var solutionDir =Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../"));
            var projectDir = Path.Combine(solutionDir, "PepitaGetSample");
            var startNew = Stopwatch.StartNew();
            new RestorePackagesTask { ProjectDirectory = projectDir }.Execute();
            startNew.Stop();
            Debug.WriteLine(startNew.ElapsedMilliseconds);
        }
    }
}