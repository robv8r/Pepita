using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;


public class ContainsPepitaGetChecker
{
    static XDocument LoadXmlForProject(string projectFile)
    {
        //HACK: for when VS incorrectly calls configure when no project is available
        if (string.IsNullOrWhiteSpace(projectFile))
        {
            return null;
        }

        //cant add to deployment projects
        if (projectFile.EndsWith(".vdproj"))
        {
            return null;
        }

        //HACK: for web projects
        if (!File.Exists(projectFile))
        {
            return null;
        }
        try
        {
            //validate is xml
            return XDocument.Load(projectFile);
        }
        catch (Exception)
        {
            //this means it is not xml and we cant do anything with it
            return null;
        }
    }

    public bool HasPepita(string projectFile)
    {
        var xml = LoadXmlForProject(projectFile);
        if (xml == null)
        {
            return false;
        }

        try
        {
            if (xml.BuildDescendants("PepitaGet.RestorePackagesTask").Any())
            {
                return true;
            }
            return xml.BuildDescendants("Import")
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