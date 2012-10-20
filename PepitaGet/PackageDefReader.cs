using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public static class PackageDefReader
{
    public static IEnumerable<PackageDef> PackageDefs(string packagesConfigPath)
    {
        if (!File.Exists(packagesConfigPath))
        {
            return Enumerable.Empty<PackageDef>();
        }
        return XDocument.Load(packagesConfigPath)
            .Descendants("package")
            .Select(x => new PackageDef
                             {
                                 Id = x.Attribute("id").Value,
                                 Version = x.Attribute("version").Value
                             });
    }
}