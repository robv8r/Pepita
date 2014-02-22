using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PepitaPackage
{
    public class CreatePackageTask : Task
    {
        public string NuGetBuildDirectory;
        public string MetadataAssembly;
        public string TargetDir;
        public string Version;

        public override bool Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("PepitaPackage (version {0}) Executing", GetType().Assembly.GetName().Version), "", "Pepita", MessageImportance.High));
            try
            {
                ValidatePackageDir();
                ValidateMetaDataAssembly();

                using (var runner = new Runner
                                        {
                                            PackageDirectory = NuGetBuildDirectory,
                                            MetadataAssembly = MetadataAssembly,
                                            Version = Version,
                                            TargetDir = TargetDir,
                                            WriteInfo = s => BuildEngine.LogMessageEvent(new BuildMessageEventArgs("\t" + s, "", "Pepita", MessageImportance.High)),
                                        })
                {
                    runner.Execute();
                }
            }
            catch (ExpectedException expectedException)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("Pepita: {0}", expectedException.Message), "", "Pepita"));
                return false;
            }
            catch (Exception exception)
            {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("Pepita: {0}", exception), "", "Pepita"));
                return false;
            }
            finally
            {
                stopwatch.Stop();
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("\tFinished ({0}ms)", stopwatch.ElapsedMilliseconds), "", "Pepita", MessageImportance.High));
            }

            return true;
        }

        void ValidateMetaDataAssembly()
        {
            if (MetadataAssembly == null)
            {
                return;
            }
            if (MetadataAssembly.Length == 0)
            {
                throw new ExpectedException("Invalid MetadataAssembly");
            }
        }

        void ValidatePackageDir()
        {
            if (string.IsNullOrWhiteSpace(NuGetBuildDirectory))
            {
                throw new ExpectedException("Expected a path to the directory containing the nuget Files.");
            }
            if (!Directory.Exists(NuGetBuildDirectory))
            {
                throw new ExpectedException(string.Format("Could not find path '{0}'.", NuGetBuildDirectory));
            }
        }

    }
}