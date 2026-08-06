using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VNGPortal.Workflows;

public class ParameterDictionary : Dictionary<string, string>, IXmlSerializable
{
    public XmlSchema GetSchema() => null;

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
                string key = reader.LocalName;
                string value = reader.ReadElementContentAsString().Trim();
                this[key] = value;
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
}