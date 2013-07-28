using System;
using System.IO;

public class TaskFileProcessor
{
    TaskFileReplacer taskFileReplacer;
    MessageDisplayer messageDisplayer;

    public TaskFileProcessor(TaskFileReplacer taskFileReplacer, MessageDisplayer messageDisplayer)
    {
        this.taskFileReplacer = taskFileReplacer;
        this.messageDisplayer = messageDisplayer;
    }

    public void ProcessTaskFile(string solutionDirectory)
    {
        try
        {
            var pepitaGetDir = PepitaGetDirectoryFinder.TreeWalkForToolsPepitaGetDir(solutionDirectory);
            if (pepitaGetDir != null)
            {
                var pepitaGetFile = Path.Combine(pepitaGetDir, "PepitaGet.dll");
                if (File.Exists(pepitaGetFile))
                {
                    Check(pepitaGetFile);
                    return;
                }
            }
            foreach (var filePath in Directory.EnumerateFiles(solutionDirectory, "PepitaGet.dll", SearchOption.AllDirectories))
            {
                Check(filePath);
            }
        }
        catch (Exception exception)
        {
            messageDisplayer.ShowError(string.Format("PepitaGet: An exception occurred while trying to check for updates.\r\nException: {0}.", exception));
        }
    }

    void Check(string pepitaGetFile)
    {
        if (VersionChecker.IsVersionNewer(pepitaGetFile))
        {
            taskFileReplacer.AddFile(Path.GetDirectoryName(pepitaGetFile));
        }
    }
}