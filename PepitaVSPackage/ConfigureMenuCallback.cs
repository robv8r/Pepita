using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

public class ConfigureMenuCallback
{
    ContentsFinder contentsFinder;
    CurrentProjectFinder currentProjectFinder;
    ExceptionDialog exceptionDialog;

    public ConfigureMenuCallback(CurrentProjectFinder currentProjectFinder, ContentsFinder contentsFinder, ExceptionDialog exceptionDialog)
    {
        this.currentProjectFinder = currentProjectFinder;
        this.exceptionDialog = exceptionDialog;
        this.contentsFinder = contentsFinder;
    }


    public void ConfigureCallback()
    {
        try
        {
            var currentProjects = currentProjectFinder.GetCurrentProjects();
            if (currentProjects
                .Any(UnsaveProjectChecker.HasUnsavedPendingChanges))
            {
                return;
            }
            foreach (var project in currentProjects)
            {
                Configure(project);
            }
        }
        catch (COMException exception)
        {
            exceptionDialog.HandleException(exception);
        }
        catch (Exception exception)
        {
            exceptionDialog.HandleException(exception);
        }
    }

    void Configure(Project project)
    {
        var dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
        var solutionDirectory = Path.GetDirectoryName(dte.Solution.FullName);
        var toolsDirectory = CreateToolsDirectory(solutionDirectory);
        ExportBuildFile(toolsDirectory);
        //@"$(SolutionDir)\Tools\PepitaGet\"
        var relativePath = PathEx.MakeRelativePath(project.FullName, toolsDirectory);
        InjectIntoProject(project.FullName, Path.Combine("$(ProjectPath)", relativePath));
    }

    string CreateToolsDirectory(string solutionDirectory)
    {
        var pepitaGetDirectory = PepitaGetDirectoryFinder.TreeWalkForToolsPepitaGetDir(solutionDirectory);
        if (pepitaGetDirectory != null)
        {
            return pepitaGetDirectory;
        }
        var packagesPath = NugetConfigReader.GetPackagesPathFromConfig(solutionDirectory);

        if (packagesPath != null)
        {
            pepitaGetDirectory = Path.Combine(Directory.GetParent(packagesPath).FullName, @"Tools\Pepita");
        }
        else
        {
            pepitaGetDirectory = Path.Combine(solutionDirectory, @"Tools\Pepita");
        }
        if (!Directory.Exists(pepitaGetDirectory))
        {
            Directory.CreateDirectory(pepitaGetDirectory);
        }
        return pepitaGetDirectory;
    }


    void InjectIntoProject(string projectFilePath, string pepitaGetToolsDirectory)
    {
        var projectInjector = new ProjectInjector
                                  {
                                      ProjectFile = projectFilePath,
                                      PepitaGetToolsDirectory = pepitaGetToolsDirectory 
                                  };
        projectInjector.Execute();
    }

    void ExportBuildFile(string toolsDirectory)
    {
        foreach (var file in Directory.GetFiles(Path.Combine(contentsFinder.ContentFilesPath, "Pepita")))
        {
            var destFileName = Path.Combine(toolsDirectory, Path.GetFileName(file));
            if (!File.Exists(destFileName))
            {
                File.Copy(file, destFileName);
            }
        }
    }

}