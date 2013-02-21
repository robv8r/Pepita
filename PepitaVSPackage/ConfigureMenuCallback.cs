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
        bool isSolutionToolsDir;
        var toolsDirectory = CreateToolsDirectory(solutionDirectory, out isSolutionToolsDir);

        ExportBuildFile(toolsDirectory);
        //@"$(SolutionDir)\Tools\PepitaGet\"
        if (isSolutionToolsDir)
        {
            InjectIntoProject(project.FullName, @"$(SolutionDir)Tools\Pepita");   
        }
        else
        {
            var projectDir = Path.GetDirectoryName(project.FullName);
            var relativePath = PathEx.MakeRelativePath(projectDir, toolsDirectory);
            InjectIntoProject(project.FullName, Path.Combine("$(ProjectDir)", relativePath));   
        }
    }

    string CreateToolsDirectory(string solutionDirectory, out bool isSolutionToolsDir)
    {
        var solutionToolsDir = Path.Combine(solutionDirectory, @"Tools\Pepita");
        var pepitaGetDirectory = PepitaGetDirectoryFinder.TreeWalkForToolsPepitaGetDir(solutionDirectory);
        if (pepitaGetDirectory != null)
        {
            isSolutionToolsDir = pepitaGetDirectory == solutionToolsDir;
            return pepitaGetDirectory;
        }
        isSolutionToolsDir = true;
        if (!Directory.Exists(solutionToolsDir))
        {
            Directory.CreateDirectory(solutionToolsDir);
        }
        return solutionToolsDir;
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