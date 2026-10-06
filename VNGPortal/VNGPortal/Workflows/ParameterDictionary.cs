using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VNGPortal.Workflows;

public class ParameterDictionary : Dictionary<string, string>, IXmlSerializable
{
    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return;
        }

        reader.ReadStartElement(); // <parameters>

        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                // ReadFrom consumes the whole element, including nested children
                var element = (XElement)XNode.ReadFrom(reader);

                this[element.Name.LocalName] = element.HasElements
                    ? string.Concat(element.Nodes().Select(n => n.ToString()))   // keep nested XML (CDATA preserved)
                    : element.Value.Trim();
            }
            else
            {
                reader.Read();
            }
        }

        reader.ReadEndElement(); // </parameters>
    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (var kvp in this)
        {
            writer.WriteStartElement(kvp.Key);
            writer.WriteCData(kvp.Value);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Parses a nested &lt;queries&gt; parameter into SPARQLGeometryQuery items.
    /// </summary>
    public List<SPARQLGeometryQuery> GetGeometryQueries(string key = "queries")
    {
        if (!TryGetValue(key, out var xml) || string.IsNullOrWhiteSpace(xml))
        {
            return new List<SPARQLGeometryQuery>();
        }

        return XElement.Parse($"<{key}>{xml}</{key}>")
            .Elements("sparql")
            .Select(SPARQLGeometryQuery.FromXml)
            .ToList();
    }
}