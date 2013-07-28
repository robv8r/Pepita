using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using EnvDTE;
using Microsoft.VisualStudio.Shell;

public class ConfigureMenuCallback
{
    ContentsFinder contentsFinder;
    CurrentProjectFinder currentProjectFinder;
    ExceptionDialog exceptionDialog;
    ContainsPepitaGetChecker containsPepitaGetChecker;

    public ConfigureMenuCallback(CurrentProjectFinder currentProjectFinder, ContentsFinder contentsFinder, ExceptionDialog exceptionDialog,
        ContainsPepitaGetChecker containsPepitaGetChecker)
    {
        this.currentProjectFinder = currentProjectFinder;
        this.exceptionDialog = exceptionDialog;
        this.containsPepitaGetChecker = containsPepitaGetChecker;
        this.contentsFinder = contentsFinder;
    }

    public void ConfigureCallback()
    {
        try
        {
            var currentProjects = currentProjectFinder.GetCurrentProjects();
            if (currentProjects.Any(UnSavedProjectChecker.HasUnsavedPendingChanges))
            {
                return;
            }

            var projectsToConfigure = (from project in currentProjects
                                       where !containsPepitaGetChecker.HasPepita(project.FullName)
                                       select project).ToList();
                                       
            if (projectsToConfigure.Count <= 0)
            {
                return;
            }

            var messageBoxText = new StringBuilder();
            messageBoxText.AppendLine(string.Format("Are you sure you want to enable Pepita for the following {0} project(s):", projectsToConfigure.Count));
            messageBoxText.AppendLine();
            foreach (var project in projectsToConfigure)
            {
                messageBoxText.AppendLine("- " + project.Name);
            }

            if (MessageBox.Show(messageBoxText.ToString(), "Enable pepita for selected projects?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            foreach (var project in projectsToConfigure)
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
            var destinationFileName = Path.Combine(toolsDirectory, Path.GetFileName(file));
            if (!File.Exists(destinationFileName))
            {
                File.Copy(file, destinationFileName);
            }
        }
    }

}