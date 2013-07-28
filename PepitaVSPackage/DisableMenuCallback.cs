using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

public class DisableMenuCallback
{
    CurrentProjectFinder currentProjectFinder;
    MessageDisplayer messageDisplayer;
    ExceptionDialog exceptionDialog;
    ContainsPepitaGetChecker containsPepitaGetChecker;

    public DisableMenuCallback(CurrentProjectFinder currentProjectFinder, MessageDisplayer messageDisplayer, ExceptionDialog exceptionDialog,
        ContainsPepitaGetChecker containsPepitaGetChecker)
    {
        this.exceptionDialog = exceptionDialog;
        this.messageDisplayer = messageDisplayer;
        this.currentProjectFinder = currentProjectFinder;
        this.containsPepitaGetChecker = containsPepitaGetChecker;
    }

    public void DisableCallback()
    {
        try
        {
            var projects = currentProjectFinder.GetCurrentProjects();
            if (projects.Any(UnSavedProjectChecker.HasUnsavedPendingChanges))
            {
                return;
            }

            var projectsToDisable = (from project in projects
                                     where containsPepitaGetChecker.HasPepita(project.FullName)
                                     select project).ToList();

            if (projectsToDisable.Count <= 0)
            {
                return;
            }

            var messageBoxText = new StringBuilder();
            messageBoxText.AppendLine(string.Format("Are you sure you want to disable Pepita for the following {0} project(s):", projectsToDisable.Count));
            messageBoxText.AppendLine();
            foreach (var project in projectsToDisable)
            {
                messageBoxText.AppendLine("- " + project.Name);
            }

            if (MessageBox.Show(messageBoxText.ToString(), "Disable pepita for selected projects?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            foreach (var project in projectsToDisable)
            {
                messageDisplayer.ShowInfo(string.Format("PepitaGet: Removed from the project '{0}'. However no binary files will be removed in case they are being used by other projects.", project.Name));
                new ProjectRemover(project.FullName);
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
}