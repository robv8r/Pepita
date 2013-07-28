using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

public class MenuStatusChecker
{
    CurrentProjectFinder currentProjectFinder;
    ExceptionDialog exceptionDialog;
    ContainsPepitaGetChecker containsPepitaGetChecker;

    public MenuStatusChecker(CurrentProjectFinder currentProjectFinder, ExceptionDialog exceptionDialog, ContainsPepitaGetChecker containsPepitaGetChecker)
    {
        this.currentProjectFinder = currentProjectFinder;
        this.exceptionDialog = exceptionDialog;
        this.containsPepitaGetChecker = containsPepitaGetChecker;
    }

    public void DisableCommandStatusCheck(OleMenuCommand disableCommand)
    {
        try
        {
            disableCommand.Enabled = false;
            foreach (var project in currentProjectFinder.GetCurrentProjects())
            {
                if (containsPepitaGetChecker.HasPepita(project.GetPath()))
                {
                    disableCommand.Enabled = true;
                    return;
                }
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
    public void ConfigureCommandStatusCheck(OleMenuCommand configureCommand)
    {
        try
        {
            configureCommand.Enabled = false;
            foreach (var project in currentProjectFinder.GetCurrentProjects())
            {
                if (!containsPepitaGetChecker.HasPepita(project.GetPath()))
                {
                    configureCommand.Enabled = true;
                    return;
                }
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