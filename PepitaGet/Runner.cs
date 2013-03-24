using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;

public partial class Runner
{
    public string ProjectDirectory;
    public string PackagesPath;
    public Action<string> WriteInfo = x => { };
    public Action<string> WriteError = x => { };
    public string CachePath;
    bool cacheCleanRequired;
    public string SolutionDirectory;
    public List<string> PackageFeeds; 

	
    public void GetCachePath()
    {
        CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NuGet","Cache");
		WriteInfo("\tCache path is " + CachePath);
        Directory.CreateDirectory(CachePath);
    }

	public bool Execute()
	{
		var stopwatch = Stopwatch.StartNew();
		WriteInfo(string.Format("Pepita (version {0}) Executing", GetType().Assembly.GetName().Version));

		try
		{
			Inner();
			return true;
		}
		catch (ExpectedException expectedException)
		{
			WriteError(expectedException.Message);
			return false;
		}
		catch (Exception exception)
		{
			WriteError(exception.ToString());
			return false;
		}
		finally
		{
			stopwatch.Stop();
			WriteInfo(string.Format("\tFinished ({0}ms)", stopwatch.ElapsedMilliseconds));
		}
	}

	void Inner()
	{
		GetAllFeeds();

		GetCachePath();

		var packagesPath = NugetConfigReader.GetPackagesPathFromConfig(ProjectDirectory);
		if (packagesPath == null)
		{
			packagesPath = Path.Combine(SolutionDirectory, "Packages");
		}

		PackagesPath = Path.GetFullPath(packagesPath);

		WriteInfo("\tUsing PackagesPath: " + PackagesPath);
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
	}

	void GetAllFeeds()
    {
        PackageFeeds = new List<string>();

        var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NuGet", "NuGet.Config");
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
                        var url = attribute.Value;
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
			PackageFeeds.Add("https://nuget.org/api/v2/package/");
        }

    }

    void ProcessPackageDef(PackageDef packageDef)
    {

        var packagePath = Path.Combine(PackagesPath, packageDef.Id + "." + packageDef.Version);
        if (Directory.Exists(packagePath))
        {
            WriteInfo("\tAlready exists so skipped " + packagePath);
            return;
        }

        bool createdNew;
        using (var mutex = new Mutex(true, GetMutexName(packagePath), out createdNew))
        {
            try
            {
                if (createdNew)
                {
                    SingleProcessPackageDef(packageDef, packagePath);
                }
                else
                {
                    mutex.WaitOne(TimeSpan.FromMinutes(2));
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

    }

    void SingleProcessPackageDef(PackageDef packageDef, string packagePath)
    {
        var packageCacheFile = GetPackageCacheFile(packageDef);

        Directory.CreateDirectory(packagePath);
        try
        {
            var nupkgFilePath = Path.Combine(packagePath, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));

            // Resolve relative paths
            nupkgFilePath = Path.GetFullPath(nupkgFilePath);

            File.Copy(packageCacheFile, nupkgFilePath, true);
            ProcessPackage(packagePath, nupkgFilePath);
        }
        catch (Exception)
        {
            Directory.Delete(packagePath, true);
            throw;
        }
    }

    static string GetMutexName(string packagePath)
    {
        var pathBytes = Encoding.UTF8.GetBytes(packagePath);

        using (var sha256Managed = new SHA256Managed())
        {
            return Convert.ToBase64String(sha256Managed.ComputeHash(pathBytes)).ToUpperInvariant();
        }
    }


    string GetPackageCacheFile(PackageDef packageDef)
    {
        var nupkgCacheFilePath = Path.Combine(CachePath, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));

        if (File.Exists(nupkgCacheFilePath))
        {
			WriteInfo("\tFound in cache " + nupkgCacheFilePath);
            File.SetLastWriteTime(nupkgCacheFilePath, DateTime.Now);
            return nupkgCacheFilePath;
        }
        Download(packageDef, nupkgCacheFilePath);

        return nupkgCacheFilePath;
    }


    void ProcessPackage(string packagePath, string nupkgFilePath)
    {
        using (var package = Package.Open(nupkgFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
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

       originalString = Uri.UnescapeDataString(originalString);
        if (originalString.StartsWith("/_rels") || originalString.StartsWith("/package"))
        {
            return;
        }
        var fullPath = Path.GetFullPath(Path.Combine(packagePath, "." + originalString));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        using (var stream = part.GetStream())
        using (var output = File.OpenWrite(fullPath))
        {
            stream.CopyTo(output);
        }
    }


}
