using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

public partial class Runner
{

    public void SubstituteNuspecContent()
    {
        nuspecContent = File.ReadAllText(nuspecPath);
        if (Version != null)
        {
            nuspecContent = nuspecContent.Replace("$version$", Version);
        }
        if (MetadataAssembly != null)
        {
            var assemblyPath = GetAssemblyPath();
            nuspecContent = nuspecContent.Replace("$id$", Path.GetFileNameWithoutExtension(MetadataAssembly));
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);
            nuspecContent = nuspecContent.Replace("$version$", fileVersionInfo.FileVersion);
            if (fileVersionInfo.CompanyName != null)
            {
                nuspecContent = nuspecContent.Replace("$authors$", fileVersionInfo.CompanyName);
            }
            if (fileVersionInfo.FileDescription != null)
            {
                nuspecContent = nuspecContent.Replace("$description$", fileVersionInfo.FileDescription);
            }
        }
        if (nuspecContent.Contains(">$") || nuspecContent.Contains("$<"))
        {
            throw new ExpectedException(string.Format("Found '>$' or '$<'. It is likely there is an property that could not be replaced. nuspec content={0}{1}", Environment.NewLine, nuspecContent));
        }
    }

    string GetAssemblyPath()
    {
        if (File.Exists(MetadataAssembly))
        {
            return MetadataAssembly;
        }
        var combine = Path.Combine(Environment.CurrentDirectory, MetadataAssembly);
        if (File.Exists(combine))
        {
            return combine;
        }

        if (!MetadataAssembly.Contains(Path.DirectorySeparatorChar))
        {
            var assemblyPath = Directory.GetFiles(PackageDirectory, MetadataAssembly, SearchOption.AllDirectories).FirstOrDefault();
            if (assemblyPath != null)
            {
                return assemblyPath;
            }
        }
        throw new ExpectedException(string.Format("Could not find MetadataAssembly file matching '{0}'.", MetadataAssembly));
    }
}