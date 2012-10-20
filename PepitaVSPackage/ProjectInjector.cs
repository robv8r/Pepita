using System.IO;
using System.Linq;
using System.Xml.Linq;

public class ProjectInjector
{
    public string ProjectFile;
    public string PepitaGetToolsDirectory;
    XDocument xDocument;

    public void Execute()
    {
        new FileInfo(ProjectFile).IsReadOnly = false;
        xDocument = XDocument.Load(ProjectFile);
        InjectImport();
        xDocument.Save(ProjectFile);
    }


    void InjectImport()
    {
        // <Import Project="$(SolutionDir)\Tools\Pepita\PepitaGet.targets" />
        var imports = xDocument.BuildDescendants("Import").ToList();
        var exists = imports
            .Any(x =>
                {
                    var xAttribute = x.Attribute("Project");
                    return xAttribute != null && xAttribute.Value.EndsWith("PepitaGet.targets");
                });
        if (exists)
        {
            return;
        }
        var importAttribute = new XAttribute("Project", Path.Combine(PepitaGetToolsDirectory, "PepitaGet.targets"));
        xDocument.Root.Add(new XElement(MsBuildXmlExtensions.BuildNamespace + "Import", importAttribute));

    }
}