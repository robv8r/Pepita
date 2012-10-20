using System;
using System.IO;
using System.IO.Packaging;

public partial class Runner
{
    private void CreatePart(Package package, string filePath, Stream sourceStream)
    {

        filePath = filePath.Replace(PackageDirectory, "").TrimStart('\\');
        var uri = PackUriHelper.CreatePartUri(new Uri(Uri.EscapeDataString(filePath), UriKind.Relative));
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);
        using (var stream = packagePart.GetStream())
        {
            sourceStream.CopyTo(stream);
        }
    }

}