using System.Xml.Linq;

public class PackageProperty
{
    public string Authors;
    public string Description;
    public string Version;
    public string Tags;
    public string Language;
    public string Id;
}
static class PackagePropertyReader
{
    public static PackageProperty GetPackageProperties(string nuspecPath)
    {

        var xElement = XDocument.Load(nuspecPath)
            .RemoveNamespace()
            .Element("package")
            .Element("metadata");

        return new PackageProperty
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