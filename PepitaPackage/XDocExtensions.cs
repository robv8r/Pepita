using System.Linq;
using System.Xml.Linq;

public static class XDocExtensions
{
    public static XDocument RemoveNamespace(this XDocument xdoc)
    {
        foreach (var e in xdoc.Root.DescendantsAndSelf())
        {

            if (e.Name.Namespace != XNamespace.None)
            {
                e.Name = XNamespace.None.GetName(e.Name.LocalName);
            }

            if (e.Attributes().Any(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None))
            {
                e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
            }
        }
        return xdoc;

    }
    public static string ElementValue(this XContainer xContainer, string name)
    {
        var xElement = xContainer.Element(name);
        if (xElement == null)
        {
            return null;
        }
        return xElement.Value;
    }
}