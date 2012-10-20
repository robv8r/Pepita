using System;
using System.Linq;
using System.Xml.Linq;

public class ContainsPepitaGetChecker
{

    public bool Check(XDocument xDocument)
    {
        try
        {
            if (xDocument.BuildDescendants("PepitaGet.RestorePackagesTask").Any())
            {
                return true;
            }
            return xDocument.BuildDescendants("Import")
                .Any(x =>
                         {
                             var xAttribute = x.Attribute("Project");
                             return xAttribute != null && xAttribute.Value.EndsWith("PepitaGet.targets");
                         });
        }
        catch (Exception exception)
        {
            throw new Exception("Could not check project for weaving task.", exception);
        }
    }
}