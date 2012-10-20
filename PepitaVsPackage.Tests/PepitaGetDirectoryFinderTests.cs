using System.IO;
using NUnit.Framework;

[TestFixture]
public class PepitaGetDirectoryFinderTests
{

    [Test]
    public void Existing()
    {
        var pepitaGetDir = PepitaGetDirectoryFinder.TreeWalkForToolsPepitaGetDir(AssemblyLocation.CurrentDirectory());
        Assert.IsTrue(Directory.Exists(pepitaGetDir));
    }

    [Test]
    public void NotExisting()
    {
        Assert.IsNull(PepitaGetDirectoryFinder.TreeWalkForToolsPepitaGetDir(Path.GetTempPath()));
    }
}