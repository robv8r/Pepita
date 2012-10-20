using System.IO;
using System.Xml.Linq;

partial class Runner
{
    public void GetPackageData()
    {
        XElement xElement;
        using (var stringReader = new StringReader(nuspecContent))
        {
            xElement = XDocument.Load(stringReader).RemoveNamespace().Element("package").Element("metadata");
        }

        packageData = new PackageData
                          {
                              Id = xElement.ElementValue("id"),
                              Authors = xElement.ElementValue("authors"),
                              Description = xElement.ElementValue("description"),
                              Version = xElement.ElementValue("version"),
                              Language = xElement.ElementValue("language"),
                              Tags = xElement.ElementValue("tags"),
                          };
    }

}