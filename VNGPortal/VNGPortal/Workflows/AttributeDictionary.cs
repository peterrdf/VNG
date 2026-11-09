using System.Xml;
using System.Xml.Linq;

namespace VNGPortal.Workflows;

/// <summary>
/// XML attributes of an element, e.g. <c>&lt;shacl uri="x.ttl"&gt;</c> => <c>["uri"] = "x.ttl"</c>.
/// </summary>
public class AttributeDictionary : Dictionary<string, string>
{
    public AttributeDictionary() : base(StringComparer.Ordinal)
    {
    }

    public static AttributeDictionary FromXmlAttributes(IEnumerable<XmlAttribute>? attributes)
    {
        var result = new AttributeDictionary();
        if (attributes == null)
        {
            return result;
        }

        foreach (var attribute in attributes)
        {
            // namespace declarations are not data
            if (attribute.Prefix == "xmlns" || attribute.Name == "xmlns")
            {
                continue;
            }

            result[attribute.LocalName] = attribute.Value;
        }

        return result;
    }

    public XmlAttribute[] ToXmlAttributes()
    {
        var document = new XmlDocument();
        var result = new List<XmlAttribute>(Count);

        foreach (var kvp in this)
        {
            var attribute = document.CreateAttribute(kvp.Key);
            attribute.Value = kvp.Value;
            result.Add(attribute);
        }

        return result.ToArray();
    }

    public string? GetValueOrDefault(string key) => TryGetValue(key, out var value) ? value : null;

    public static AttributeDictionary FromXElement(XElement e)
    {
        var result = new AttributeDictionary();
        foreach (var attribute in e.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            result[attribute.Name.LocalName] = attribute.Value;
        }

        return result;
    }
}
