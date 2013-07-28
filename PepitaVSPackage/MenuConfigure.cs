using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

public class MenuConfigure
{
    OleMenuCommand configureCommand;
    OleMenuCommand disableCommand;
    ConfigureMenuCallback configureMenuCallback;
    DisableMenuCallback disableMenuCallback;
    IMenuCommandService menuCommandService;
    MenuStatusChecker menuStatusChecker;
    Guid commandSet = new Guid("25097461-9b2d-42c3-8668-1132922b98b8");

    public MenuConfigure(ConfigureMenuCallback configureMenuCallback, DisableMenuCallback disableMenuCallback, IMenuCommandService menuCommandService, MenuStatusChecker menuStatusChecker)
    {
        this.configureMenuCallback = configureMenuCallback;
        this.disableMenuCallback = disableMenuCallback;
        this.menuCommandService = menuCommandService;
        this.menuStatusChecker = menuStatusChecker;
    }

    public void RegisterMenus()
    {
        CreateConfigCommand();
        CreateDisableCommand();
    }

    void CreateDisableCommand()
    {
        var disableCommandId = new CommandID(commandSet, 2);
        disableCommand = new OleMenuCommand(delegate { disableMenuCallback.DisableCallback(); }, disableCommandId)
                             {
                                 Enabled = false
                             };
        disableCommand.BeforeQueryStatus += delegate { menuStatusChecker.DisableCommandStatusCheck(disableCommand); };
        menuCommandService.AddCommand(disableCommand);
    }
 
    void CreateConfigCommand()
    {
        var configureCommandId = new CommandID(commandSet, 1);
        configureCommand = new OleMenuCommand(delegate { configureMenuCallback.ConfigureCallback(); }, configureCommandId);
        configureCommand.BeforeQueryStatus += delegate { menuStatusChecker.ConfigureCommandStatusCheck(configureCommand); };
        menuCommandService.AddCommand(configureCommand);
    }
}