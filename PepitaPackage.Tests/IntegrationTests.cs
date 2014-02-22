using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

[TestFixture]
public class IntegrationTests
{
    [Test]
    public void Execute()
    {

        var packagePath = Path.Combine(Environment.CurrentDirectory, "NugetPackageFiles");
        packagePath = Path.GetFullPath(packagePath);
        using (var runner = new Runner
        {
            PackageDirectory = packagePath,
            MetadataAssembly = "Standard.dll",
            WriteInfo = s => Debug.WriteLine(s)
        })
        {
            runner.Execute();
        }

        var outputFile = Path.Combine(packagePath, "MyPackage.1.0.nupkg");
        var expectedFile = Path.Combine(Environment.CurrentDirectory, "MyPackage.1.0.nupkg ");

        using (var package1 = Package.Open(expectedFile))
        using (var package2 = Package.Open(outputFile))
        {
            foreach (var part1 in package1.GetParts())
            {
                if (part1.Uri.OriginalString.EndsWith("psmdcp"))
                {
                    continue;
                }
                if (part1.Uri.OriginalString.EndsWith("rels"))
                {
                    continue;
                }
                var part2 = package2.GetPart(part1.Uri);
                var hash1 = GetFileHash(part1);
                var hash2 = GetFileHash(part2);
                Assert.AreEqual(hash1, hash2, part1.Uri.OriginalString);
            }
        }
    }

    public string GetFileHash(PackagePart part)
    {
        using (var inputStream = part.GetStream())
        using (var md5 = new MD5CryptoServiceProvider())
        {
            var hash = md5.ComputeHash(inputStream);
            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(string.Format("{0:X2}", b));
            }
            return sb.ToString();
        }
    }
}