using System;
using System.IO;
using System.IO.Packaging;

public partial class Runner
{
    void CreatePart(Package package, string filePath, Stream sourceStream)
    {

        filePath = filePath.Replace(PackageDirectory, "").TrimStart('\\');
	    var escapeDataString = Uri.EscapeUriString(filePath);
	    var uri = PackUriHelper.CreatePartUri(new Uri(escapeDataString, UriKind.Relative));
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);
        using (var stream = packagePart.GetStream())
        {
            sourceStream.CopyTo(stream);
        }
    }

}