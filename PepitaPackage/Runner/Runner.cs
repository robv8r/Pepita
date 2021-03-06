﻿using System;
using System.IO;
using System.IO.Packaging;

public partial class Runner
{
    public string PackageDirectory;
    string nuspecPath;

    public Action<string> WriteInfo = x => { };
    public string MetadataAssembly;
    public string Version;
    string nuspecContent;
    PackageData packageData;
    public string TargetDir;

    public void Execute()
    {
        GetNuspecPath();

        SubstituteNuspecContent();

        GetPackageData();

        var fileName = string.Format("{0}.{1}.nupkg", packageData.Id, packageData.Version);
        var nupkgPath = Path.Combine(PackageDirectory, fileName);
        File.Delete(nupkgPath);
        using (var package = Package.Open(nupkgPath, FileMode.CreateNew))
        {
            WriteManifest(package);
            WriteFiles(package);
            WriteMetadata(package);
            package.Flush();
            package.Close();
        }
        if (TargetDir != null)
        {
            Directory.CreateDirectory(TargetDir);
            foreach (var fileToDelete in Directory.EnumerateFiles(TargetDir, packageData.Id + ".*.nupkg"))
            {
                File.Delete(fileToDelete);
            }
            var destinationFileName = Path.Combine(TargetDir, fileName);
            File.Move(nupkgPath, destinationFileName);
        }
    }

    void WriteMetadata(Package package)
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

    void WriteFiles(Package package)
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
            CreatePart(package, entry);
        }
    }



    void WriteManifest(Package package)
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

}