using System;
using EnvDTE;

public static class ProjectExtensions
{
    public static string GetPath(this Project project)
    {
        try
        {
            return project.FullName;
        }
        catch (NotImplementedException)
        {
            return null;
        }
    }
}