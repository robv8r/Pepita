using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Xml.Linq;
using System.Xml.XPath;

public partial class Runner
{
    public string ProjectDirectory;
    public string PackagesPath;
    public Action<string> WriteInfo = x => { };
    public string CachePath;
    bool cacheCleanRequired;
    public string SolutionDirectory;
    public List<string> PackageFeeds; 

    public void GetCachePath()
    {
        CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet","Cache");
        WriteInfo("Cache path is " + CachePath);
        Directory.CreateDirectory(CachePath);
    }

    public void Execute()
    {
        GetAllFeeds();

        GetCachePath();

        var packagesPath = NugetConfigReader.GetPackagesPathFromConfig(ProjectDirectory);
        if (packagesPath == null)
        {
            packagesPath = Path.Combine(SolutionDirectory, "Packages");
        }

        PackagesPath = Path.GetFullPath(packagesPath);

        WriteInfo("Using PackagesPath: " + PackagesPath);
        var projectPackagesConfigPath = Path.Combine(ProjectDirectory, "packages.config");
        foreach (var packageDef in PackageDefReader.PackageDefs(projectPackagesConfigPath))
        {
            ProcessPackageDef(packageDef);
        }
        var solutionPackagesConfigPath = Path.Combine(SolutionDirectory, ".nuget", "packages.config");
        foreach (var packageDef in PackageDefReader.PackageDefs(solutionPackagesConfigPath))
        {
            ProcessPackageDef(packageDef);
        }
        CleanCache();
    }

    void GetAllFeeds()
    {
        PackageFeeds = new List<string>();

#if DEBUG
        System.Diagnostics.Debugger.Launch();
#endif

        string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config");
        if (File.Exists(configPath))
        {
            var doc = XDocument.Load(configPath);
            var packageSources = doc.XPathSelectElement("/configuration/packageSources");
            if (packageSources != null)
            {
                foreach (var packageSource in packageSources.Elements())
                {
                    var attribute = packageSource.Attribute("value");
                    if (attribute != null)
                    {
                        string url = attribute.Value;
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            if (!PackageFeeds.Contains(url))
                            {
                                PackageFeeds.Add(url);
                            }
                        }
                    }
                }
            }
        }

        if (PackageFeeds.Count == 0)
        {
            PackageFeeds.Add("http://packages.nuget.org");
        }

    }

    void ProcessPackageDef(PackageDef packageDef)
    {
        var packagePath = Path.Combine(PackagesPath, packageDef.Id + "." + packageDef.Version);
        if (Directory.Exists(packagePath))
        {
            WriteInfo("Already exists so skipped " + packagePath);
            return;
        }

        var packageCacheFile = GetPackageCacheFile(packageDef);

        Directory.CreateDirectory(packagePath);
        try
        {
            var nupkgFilePath = Path.Combine(packagePath, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));

            // Resolve relative paths
            nupkgFilePath = Path.GetFullPath(nupkgFilePath);

            if (Directory.Exists(nupkgFilePath))
            {
                return;
            }
            File.Copy(packageCacheFile, nupkgFilePath);

            ProcessPackage(packagePath, nupkgFilePath);
        }
        catch (Exception)
        {
            Directory.Delete(packagePath, true);
            throw;
        }
    }


    string GetPackageCacheFile(PackageDef packageDef)
    {

        var nupkgCacheFilePath = Path.Combine(CachePath, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));

        if (File.Exists(nupkgCacheFilePath))
        {
            WriteInfo("Found in cache " + nupkgCacheFilePath);
            File.SetLastWriteTime(nupkgCacheFilePath, DateTime.Now);
            return nupkgCacheFilePath;
        }
        Download(packageDef, nupkgCacheFilePath);

        return nupkgCacheFilePath;
    }


    void ProcessPackage(string packagePath, string nupkgFilePath)
    {
        using (var package = Package.Open(nupkgFilePath))
        {
            foreach (var part in package.GetParts())
            {
                ProcessPart(packagePath, part);
            }
        }
    }

    void ProcessPart(string packagePath, PackagePart part)
    {
        var originalString = part.Uri.OriginalString;
        if (originalString.StartsWith("/_rels") || originalString.StartsWith("/package"))
        {
            return;
        }
        var fullPath = Path.GetFullPath(Path.Combine(packagePath, "." + originalString));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        if (Directory.Exists(fullPath))
        {
            return;
        }
        using (var stream = part.GetStream())
        using (var output = File.OpenWrite(fullPath))
        {
            stream.CopyTo(output);
        }
    }

}