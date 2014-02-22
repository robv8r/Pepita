using System;
using System.IO;
using System.IO.Packaging;

public partial class Runner : IDisposable
{
    public string PackageDirectory;
    string nuspecPath;

    public Action<string> WriteInfo = x => { };
    public string MetadataAssembly;
    public string Version;
    string nuspecContent;
    PackageData packageData;
    Package package;
    string nupkgPath;
    public string TargetDir;

    public void Execute()
    {
        GetNuspecPath();

        SubstituteNuspecContent();

        GetPackageData();

        var fileName = string.Format("{0}.{1}.nupkg", packageData.Id, packageData.Version);
        nupkgPath = Path.Combine(PackageDirectory, fileName);
        CreatePackage();
        WriteManifest();
        WriteFiles();
        WriteMetadata();
		package.Flush();
        package.Close();
        if (TargetDir != null)
        {
            Directory.CreateDirectory(TargetDir);
            var destFileName = Path.Combine(TargetDir, fileName);
            File.Delete(destFileName);
            File.Move(nupkgPath, destFileName);
        }
    }

    void CreatePackage()
    {
        File.Delete(nupkgPath);
        package = Package.Open(nupkgPath, FileMode.CreateNew);
    }

    void WriteMetadata()
    {
        package.PackageProperties.Creator = packageData.Authors;
        package.PackageProperties.Description = packageData.Description;
        package.PackageProperties.Identifier = packageData.Id;
        package.PackageProperties.Version = packageData.Version;
        package.PackageProperties.Language = packageData.Language;
        package.PackageProperties.Keywords = packageData.Tags;
    }


    void GetNuspecPath()
    {
        var strings = Directory.GetFiles(PackageDirectory, "*.nuspec");
        if (strings.Length != 1)
        {
            throw new ExpectedException(string.Format("Expected 1 .nuspec file in '{0}'.", PackageDirectory));
        }
        nuspecPath = strings[0];
    }

    void WriteFiles()
    {
        foreach (var entry in Directory.GetFiles(PackageDirectory, "*", SearchOption.AllDirectories))
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


    void WriteManifest()
    {
        var uri = PackUriHelper.CreatePartUri(new Uri(Uri.EscapeDataString(Path.GetFileName(nuspecPath)), UriKind.Relative));

        // Create the manifest relationship
        package.CreateRelationship(uri, TargetMode.Internal, "http://schemas.microsoft.com/packaging/2010/07/manifest");

        // Create the part
        var packagePart = package.CreatePart(uri, "application/octet", CompressionOption.Maximum);

        using (var stream = packagePart.GetStream())
        using (var streamWriter = new StreamWriter(stream))
        {
            streamWriter.Write(nuspecContent);
        }
    }


    public void Dispose()
    {
        if (package != null)
        {
            package.Close();
        }

    }
}