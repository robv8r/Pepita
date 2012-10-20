using System.IO;

public static class PepitaGetDirectoryFinder
{
    public static string TreeWalkForToolsPepitaGetDir(string currentDirectory)
    {
        while (true)
        {
            var pepitaGetDir = Path.Combine(currentDirectory, @"Tools\Pepita");
            if (Directory.Exists(pepitaGetDir))
            {
                return pepitaGetDir;
            }
            try
            {
                var parent = Directory.GetParent(currentDirectory);
                if (parent == null)
                {
                    break;
                }
                currentDirectory = parent.FullName;
            }
            catch
            {
                // trouble with tree walk.
                return null;
            }
        }
        return null;
    }
}