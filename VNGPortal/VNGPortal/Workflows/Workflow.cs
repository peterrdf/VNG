using System.Collections.Generic;
using System.Xml.Serialization;

namespace VNGPortal.Workflows;

[XmlRoot("workflow")]
public class Workflow
{
    [XmlElement("id")]
    public string Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("description")]
    public string Description { get; set; }

    [XmlArray("steps")]
    [XmlArrayItem("step")]
    public List<Step> Steps { get; set; }
}

public class Step
{
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("description")]
    public string Description { get; set; }

    [XmlElement("type")]
    public string Type { get; set; }

    [XmlElement("parameters")]
    public ParameterDictionary Parameters { get; set; }
}

[XmlRoot("sparql")]
public class SPARQLQuery
{
    [XmlElement("id")]
    public string Id { get; set; }

    [XmlElement("query")]
    public string Query { get; set; }
}

[XmlRoot("shacl")]
public class SHACLShape
{
    [XmlElement("id")]
    public string Id { get; set; }

    [XmlElement("shape")]
    public string Shape { get; set; }
}