using System.IO;
using System.Linq;
using System.Xml.Linq;

public class ProjectRemover
{
    XDocument xDocument;

    public ProjectRemover(string projectFile)
    {
        new FileInfo(projectFile).IsReadOnly = false;
        xDocument = XDocument.Load(projectFile);
        RemoveImport();
        xDocument.Save(projectFile);
    }

    void RemoveImport()
    {
        xDocument.BuildDescendants("Import")
            .Where(x =>
                       {
                           var xAttribute = x.Attribute("Project");
                           return xAttribute != null && xAttribute.Value.EndsWith("PepitaGet.targets");
                       })
            .Remove();
    }
}