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
			var runner = new Runner
				{
					ProjectDirectory = ProjectDirectory,
					SolutionDirectory = SolutionDirectory,
					WriteInfo = s => BuildEngine.LogMessageEvent(new BuildMessageEventArgs(s, "", "Pepita", MessageImportance.High)),
					WriteError = s => BuildEngine.LogErrorEvent(new BuildErrorEventArgs("", "", "", 0, 0, 0, 0, string.Format("Pepita: {0}", s), "", "Pepita")),
				};
			return runner.Execute();
		}

	}
}