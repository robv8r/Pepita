﻿using System;
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

    private void DownloadFromSpecificSource(string nupkgCacheFilePath, string packageLocation)
    {
        string tempFileName = null;

        try
        {
            WriteInfo("Downloading " + packageLocation);

            var uri = new Uri(packageLocation);
            if (uri.IsFile)
            {
                File.Copy(packageLocation, nupkgCacheFilePath, true);
            }
            else
            {
                using (var webClient = new WebClient())
                {
                    webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

                    tempFileName = Path.GetTempFileName();

                    
                    webClient.DownloadFile(packageLocation, tempFileName);
                    File.Copy(tempFileName, nupkgCacheFilePath, true);
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

    IEnumerable<string> GetUrls(PackageDef packageDef)
    {
        var urls = new List<string>();

        foreach (var feed in PackageFeeds)
        {
            if (feed.Contains("//"))
            {
                // Online
                string nugetUrl = feed;
                if (!nugetUrl.EndsWith("package/"))
                {
                    nugetUrl += "package/";
                }

                nugetUrl = string.Format("{0}/{1}/{2}", nugetUrl, packageDef.Id, packageDef.Version);
                urls.Add(nugetUrl);
            }
            else
            {
                // Local
                string nugetUrl = Path.Combine(feed, string.Format("{0}.{1}.nupkg", packageDef.Id, packageDef.Version));
                urls.Add(nugetUrl);
            }
        }

        return urls;

        //return AdditionalFeeds
        //    .Select(source => string.Format("{0}/api/v2/package/{1}/{2}", source, packageDef.Id, packageDef.Version));
    }
}