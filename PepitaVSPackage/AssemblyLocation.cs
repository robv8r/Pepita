using System.IO;

public static class AssemblyLocation
{
    public static string CurrentDirectory()
    {
        var location = typeof(AssemblyLocation).Assembly.CodeBase.Replace("file:///", "");

        return Path.GetDirectoryName(location);
    }
}