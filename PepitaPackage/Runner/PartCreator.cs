using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;

public partial class Runner
{
    void CreatePart(Package package, string entry)
    {
        var filePath = entry.Replace(PackageDirectory, "").TrimStart('\\');
        var segments = filePath.Split(new[] { '/', Path.DirectorySeparatorChar }, StringSplitOptions.None)
                   .Select(Uri.EscapeDataString);
        var escapedPath = String.Join("/", segments);
        var uri = PackUriHelper.CreatePartUri(new Uri(escapedPath, UriKind.Relative));
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);
        using (var inputFileStream = File.OpenRead(entry))
        using (var zipStream = packagePart.GetStream())
        {
            inputFileStream.CopyTo(zipStream);
        }
    }
}