﻿using System;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

[ProvideAutoLoad("F1536EF8-92EC-443C-9ED7-FDADF150DA82")] //SolutionExists
[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F ")] //NoSolution
[ProvideAutoLoad("4d7a79c7-e2e3-4140-93cc-f0e68a6cae56")]
[PackageRegistration(UseManagedResourcesOnly = true)]
[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid("e5f9a48d-3edf-4f40-a55e-2c39f895cd9b")]
public sealed class PepitaVSPackagePackage : Package
{

    protected override void Initialize()
    {
        base.Initialize();
        var exceptionDialog = new ExceptionDialog();
        try
        {
            var menuCommandService = (IMenuCommandService) GetService(typeof (IMenuCommandService));
            var errorListProvider = new ErrorListProvider(ServiceProvider.GlobalProvider);

            var currentProjectFinder = new CurrentProjectFinder();
            var contentsFinder = new ContentsFinder();
            var configureMenuCallback = new ConfigureMenuCallback(currentProjectFinder, contentsFinder, exceptionDialog);
            var messageDisplayer = new MessageDisplayer(errorListProvider);
            var disableMenuConfigure = new DisableMenuCallback(currentProjectFinder, messageDisplayer, exceptionDialog);
            var containsPepitaGetChecker = new ContainsPepitaGetChecker();
            var menuStatusChecker = new MenuStatusChecker(currentProjectFinder, exceptionDialog, containsPepitaGetChecker);
            new MenuConfigure(configureMenuCallback, disableMenuConfigure, menuCommandService, menuStatusChecker).RegisterMenus();
            var taskFileReplacer = new TaskFileReplacer(messageDisplayer, contentsFinder);
            var taskFileProcessor = new TaskFileProcessor(taskFileReplacer, messageDisplayer);
            var msBuildKiller = new MSBuildKiller();
            new SolutionEvents(taskFileProcessor, exceptionDialog, msBuildKiller).RegisterSolutionEvents();
            new TaskFileReplacer(messageDisplayer, contentsFinder).CheckForFilesToUpdate();
        }
        catch (Exception exception)
        {
            exceptionDialog.HandleException(exception);
        }
    }

}