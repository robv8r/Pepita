using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;

public partial class Runner
{
    void Download(PackageDef packageDef, string nupkgCacheFilePath)
    {
        cacheCleanRequired = true;
        var errors = new StringBuilder("Failed to download. This package may no longer exist on the nuget feed or there is a problem with your internet connection. Errors:");

        var addresses = GetUrls(packageDef).ToList();
        foreach (var address in addresses)
        {
            try
            {
                DownloadFromSpecificSource(nupkgCacheFilePath, address);
                return;
            }
            catch (Exception ex)
            {
                errors.AppendLine(string.Format("Failed to download '{0}' due to '{1}'.", address, ex.Message));
            }
        }

        throw new ExpectedException(errors.ToString());
    }

    void DownloadFromSpecificSource(string nupkgCacheFilePath, string packageLocation)
    {
        string tempFileName = null;

        try
        {
			WriteInfo("\tDownloading " + packageLocation);

            var uri = new Uri(packageLocation);
            if (uri.IsFile)
            {
                FileCopy.Copy(packageLocation, nupkgCacheFilePath);
            }
            else
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                    tempFileName = Path.GetTempFileName();

                    
                    webClient.DownloadFile(packageLocation, tempFileName);
                    FileCopy.Copy(tempFileName, nupkgCacheFilePath);
                }
            }
        }
        catch (Exception)
        {
            if (tempFileName != null)
            {
                File.Delete(tempFileName);
            }

            throw;
        }
    }
	static bool IsLocalPath(string path)
	{
		return new Uri(path).IsFile;
	}

    IEnumerable<string> GetUrls(PackageDef packageDef)
    {
        foreach (var feed in PackageFeeds)
        {
	        if (IsLocalPath(feed))
	        {
		        // Local
		        yield return Path.Combine(feed, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));
	        }
	        else
	        {
		        // Online
		        var nugetUrl = feed;
				nugetUrl = nugetUrl.TrimEnd('/');
		        if (!nugetUrl.EndsWith("package",StringComparison.OrdinalIgnoreCase))
		        {
			        nugetUrl += "/package";
		        }

		        yield return string.Format("{0}/{1}/{2}", nugetUrl, packageDef.Id, packageDef.Version);
	        }
        }

        //return AdditionalFeeds
        //    .Select(source => string.Format("{0}/api/v2/package/{1}/{2}", source, packageDef.Id, packageDef.Version));
    }
}