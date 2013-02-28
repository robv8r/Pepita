using System;
using System.IO;
using System.Linq;

public partial class Runner
{
    void CleanCache()
    {
        if (!cacheCleanRequired)
        {
            return;
        }
		WriteInfo("\tCleaning Cache");
        foreach (var file in Directory.GetFiles(CachePath, "*.nupkg")
            .Select(s => new FileInfo(s))
            .OrderByDescending(s => s.LastWriteTime)
            .Skip(100))
        {
            try
            {
                if (file.Exists)
                {
                    file.Delete();
                }
            }
            catch (Exception exception)
            {
                //Dont care about delete fail. Will try again next time
				WriteInfo(string.Format("\tFailed to delete '{0}' from cache. Exception: {1}", file.FullName, exception));
            }
        }
    }
}