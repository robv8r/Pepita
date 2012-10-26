using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace PepitaGet
{

    public class RestorePackagesTask : Task
    {
        [Required]
        public string ProjectDirectory { get; set; }
        [Required]
        public string SolutionDirectory { get; set; }

        public override bool Execute()
        {
            var stopwatch = Stopwatch.StartNew(); 
            BuildEngine.LogMessageEvent(new BuildMessageEventArgs(string.Format("Pepita (version {0}) Executing", GetType().Assembly.GetName().Version), "", "Pepita", MessageImportance.High));

            try
            {
                GetProjectPath(Console.Out);

                var runner = new Runner
                                 {
                                     ProjectDirectory = ProjectDirectory,
                                     SolutionDirectory = SolutionDirectory,
                                     WriteInfo = s => BuildEngine.LogMessageEvent(new BuildMessageEventArgs("\t" + s, "", "Pepita", MessageImportance.High)),
                                 };
                runner.Execute();
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

        void GetProjectPath(TextWriter outputWriter)
        {
            if (ProjectDirectory == null)
            {
                outputWriter.WriteLine("\tNo parameter provided so using Environment.CurrentDirectory '{0}' as ProjectDir.", Environment.CurrentDirectory);
                ProjectDirectory = Environment.CurrentDirectory;
            }
            //trim trailing quotes because MSBuild is a POS
            if (!Directory.Exists(ProjectDirectory))
            {
                throw new ExpectedException(string.Format("Could not find path '{0}'", ProjectDirectory));
            }
        }
    }
}