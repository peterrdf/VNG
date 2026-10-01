using System.Collections.Generic;
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
}

[XmlRoot("shacl")]
public class SHACLShape
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("shape")]
    public string Shape { get; set; } = string.Empty;
}