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
        using (var webClient = new WebClient())
        {
            webClient.Credentials = CredentialCache.DefaultNetworkCredentials;

            var addresses = GetUrls(packageDef).ToList();
            foreach (var address in addresses)
            {
                string tempFileName = null;
                try
                {
                    tempFileName = Path.GetTempFileName();
                    WriteInfo("Downloading " + address);
                    webClient.DownloadFile(address, tempFileName);
                    File.Copy(tempFileName, nupkgCacheFilePath, true);
                    return;
                }
                catch (WebException webException)
                {
                    if (tempFileName != null)
                    {
                        File.Delete(tempFileName);
                    }
                    errors.AppendLine(string.Format("Failed to download '{0}' due to '{1}'.", address, webException.Message));
                }
            }

            throw new ExpectedException(errors.ToString());
        }
    }

    IEnumerable<string> GetUrls(PackageDef packageDef )
    {
        return AdditionalFeeds
            .Select(source => string.Format("{0}/api/v2/package/{1}/{2}", source, packageDef.Id, packageDef.Version));
    }
}