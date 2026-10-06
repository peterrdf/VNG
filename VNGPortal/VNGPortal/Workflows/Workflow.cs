using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace VNGPortal.Workflows;

[XmlRoot("workflow")]
public class Workflow
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlArray("steps")]
    [XmlArrayItem("step")]
    public List<Step> Steps { get; set; } = new List<Step>();
}

public class Step
{
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    [XmlElement("parameters")]
    public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();
}

[XmlRoot("sparql")]
public class SPARQLQuery
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("query")]
    public string Query { get; set; } = string.Empty;

    [XmlElement("parameters")]
    public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();
}

[XmlRoot("shacl")]
public class SHACLShape
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("shape")]
    public string Shape { get; set; } = string.Empty;

    [XmlArray("views")]
    [XmlArrayItem("view")]
    public List<SHACLView> Views { get; set; } = new();
}

[XmlRoot("sparql")]
public class SPARQLGeometryQuery : SPARQLQuery
{
    [XmlElement("geometryVariable")]
    public string GeometryVariable { get; set; } = string.Empty;

    public static SPARQLGeometryQuery FromXml(XElement e)
    {
        var parameters = new ParameterDictionary();
        var parametersElement = e.Element("parameters");
        if (parametersElement != null)
        {
            foreach (var p in parametersElement.Elements())
            {
                parameters[p.Name.LocalName] = p.Value.Trim();
            }
        }

        return new SPARQLGeometryQuery
        {
            Id = e.Element("id")?.Value.Trim() ?? "",
            Query = e.Element("query")?.Value.Trim() ?? "",
            GeometryVariable = e.Element("geometryVariable")?.Value.Trim() ?? "",
            Parameters = parameters
        };
    }
}

public class SHACLView
{
    [XmlElement("type")]
    public string Type { get; set; } = string.Empty;

    [XmlElement("visualization")]
    public string Visualization { get; set; } = string.Empty;

    [XmlArray("queries")]
    [XmlArrayItem("sparql")]
    public List<SPARQLGeometryQuery> Queries { get; set; } = new();
}