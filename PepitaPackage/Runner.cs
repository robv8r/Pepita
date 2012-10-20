using System;
using System.IO;
using System.IO.Packaging;

public class Runner 
{
    public string PackageDirectory;
    string nuspecPath;

    public Action<string> WriteLog = x => { };

    public void Execute()
    {
        GetNuspecPath();
        
        var packageProperties = PackagePropertyReader.GetPackageProperties(nuspecPath);
        var nupkgPath = Path.Combine(PackageDirectory, string.Format("{0}.{1}.nupkg", packageProperties.Id, packageProperties.Version));
        File.Delete(nupkgPath);

        using (var package = Package.Open(nupkgPath, FileMode.CreateNew))
        {
            WriteManifest(package, nuspecPath);
            WriteFiles(package);
            WriteMetadata(packageProperties, package);
        }
    }

    static void WriteMetadata(PackageProperty packageProperties, Package package)
    {
        package.PackageProperties.Creator = packageProperties.Authors;
        package.PackageProperties.Description = packageProperties.Description;
        package.PackageProperties.Identifier = packageProperties.Id;
        package.PackageProperties.Version = packageProperties.Version;
        package.PackageProperties.Language = packageProperties.Language;
        package.PackageProperties.Keywords = packageProperties.Tags;
    }

    void GetNuspecPath()
    {
        var strings = Directory.GetFiles(PackageDirectory, "*.nuspec");
        if (strings.Length == 0)
        {
            throw new ExpectedException(string.Format("Could not find a .nuspec file in '{0}'.", PackageDirectory));
        }
        if (strings.Length > 1)
        {
            throw new ExpectedException(string.Format("Found more than 1 .nuspec files in '{0}'.", PackageDirectory));
        }
        nuspecPath = strings[0];
    }

    void WriteFiles(Package package)
    {
        foreach (var entry in Directory.EnumerateFiles(PackageDirectory, "*", SearchOption.AllDirectories))
        {
            if (entry.EndsWith(".nuspec"))
            {
                continue;
            }
            if (entry.EndsWith(".nupkg"))
            {
                continue;
            }
            using (Stream stream = File.OpenRead(entry))
            {
                CreatePart(package, entry, stream);
            }
        }
    }

    void CreatePart(Package package, string filePath, Stream sourceStream)
    {

        filePath = filePath.Replace(PackageDirectory, "").TrimStart('\\');
        var uri = PackUriHelper.CreatePartUri(new Uri(Uri.EscapeDataString(filePath), UriKind.Relative));
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);
        using (var stream = packagePart.GetStream())
        {
            sourceStream.CopyTo(stream);
        }
    }


    static void WriteManifest(Package package, string nuspecPath)
    {
        var uri = PackUriHelper.CreatePartUri(new Uri(Uri.EscapeDataString(Path.GetFileName(nuspecPath)), UriKind.Relative));

        // Create the manifest relationship
        package.CreateRelationship(uri, TargetMode.Internal, "http://schemas.microsoft.com/packaging/2010/07/manifest");

        // Create the part
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);

        using (var stream = packagePart.GetStream())
        using (var fileStream = File.OpenRead(nuspecPath))
        {
            fileStream.CopyTo(stream);
        }
    }

   
}