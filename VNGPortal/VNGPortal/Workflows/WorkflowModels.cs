using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;

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

public class SPARQLQueryElement
{
    /// <summary>Attributes of the &lt;query&gt; element, e.g. uri.</summary>
    [XmlIgnore]
    public AttributeDictionary Attributes { get; set; } = new AttributeDictionary();

    [XmlAnyAttribute]
    public XmlAttribute[] XmlAttributes
    {
        get => Attributes.ToXmlAttributes();
        set => Attributes = AttributeDictionary.FromXmlAttributes(value);
    }

    /// <summary>Inline query (text/CDATA), if any.</summary>
    [XmlText]
    public string Text { get; set; } = string.Empty;

    public static SPARQLQueryElement FromXml(XElement? e)
    {
        var result = new SPARQLQueryElement();
        if (e == null)
        {
            return result;
        }

        result.Text = e.Value;
        foreach (var attribute in e.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            result.Attributes[attribute.Name.LocalName] = attribute.Value;
        }

        return result;
    }
}

[XmlRoot("sparql")]
public class SPARQLQuery
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("query")]
    public SPARQLQueryElement QueryElement { get; set; } = new SPARQLQueryElement();

    /// <summary>Inline query text.</summary>
    [XmlIgnore]
    public string Query
    {
        get => QueryElement.Text.Trim();
        set => QueryElement.Text = value;
    }

    /// <summary>Value of &lt;query uri="..."&gt;, if any.</summary>
    [XmlIgnore]
    public string? QueryUri => QueryElement.Attributes.GetValueOrDefault("uri");

    [XmlElement("parameters")]
    public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();

    /// <summary>Attributes of the &lt;sparql&gt; element.</summary>
    [XmlIgnore]
    public AttributeDictionary Attributes { get; set; } = new AttributeDictionary();

    [XmlAnyAttribute]
    public XmlAttribute[] XmlAttributes
    {
        get => Attributes.ToXmlAttributes();
        set => Attributes = AttributeDictionary.FromXmlAttributes(value);
    }
}

public class SHACLShapeElement
{
    /// <summary>Attributes of the &lt;shape&gt; element, e.g. uri.</summary>
    [XmlIgnore]
    public AttributeDictionary Attributes { get; set; } = new AttributeDictionary();

    [XmlAnyAttribute]
    public XmlAttribute[] XmlAttributes
    {
        get => Attributes.ToXmlAttributes();
        set => Attributes = AttributeDictionary.FromXmlAttributes(value);
    }

    /// <summary>Inline shape (text/CDATA), if any.</summary>
    [XmlText]
    public string Text { get; set; } = string.Empty;
}

[XmlRoot("shacl")]
public class SHACLShape
{
    [XmlElement("id")]
    public string Id { get; set; } = string.Empty;

    [XmlElement("shape")]
    public SHACLShapeElement ShapeElement { get; set; } = new SHACLShapeElement();

    [XmlIgnore]
    public string Shape
    {
        get => ShapeElement.Text.Trim();
        set => ShapeElement.Text = value;
    }

    [XmlIgnore]
    public string? ShapeUri => ShapeElement.Attributes.GetValueOrDefault("uri");

    [XmlElement("parameters")]
    public ParameterDictionary Parameters { get; set; } = new ParameterDictionary();

    /// <summary>Attributes of the &lt;shacl&gt; element.</summary>
    [XmlIgnore]
    public AttributeDictionary Attributes { get; set; } = new AttributeDictionary();

    [XmlAnyAttribute]
    public XmlAttribute[] XmlAttributes
    {
        get => Attributes.ToXmlAttributes();
        set => Attributes = AttributeDictionary.FromXmlAttributes(value);
    }

    [XmlArray("views")]
    [XmlArrayItem("view")]
    public List<SHACLView> Views { get; set; } = new();
}

[XmlRoot("sparql")]
public class SPARQLGeometryQuery : SPARQLQuery
{
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
            QueryElement = SPARQLQueryElement.FromXml(e.Element("query")),
            GeometryVariable = e.Element("geometryVariable")?.Value.Trim() ?? "",
            ZoomTo = e.Element("zoomTo")?.Value.Trim().ToLower() == "true",
            Parameters = parameters,
            Attributes = AttributeDictionary.FromXElement(e)
        };
    }

    [XmlElement("geometryVariable")]
    public string GeometryVariable { get; set; } = string.Empty;

    [XmlElement("zoomTo")]
    public bool ZoomTo { get; set; } = false;
}

[XmlRoot("sparql")]
public class SPARQLVariableQuery : SPARQLQuery
{
    [XmlElement("valueVariable")]
    public string ValueVariable { get; set; } = string.Empty;

    [XmlElement("valueVariableLabel")]
    public string ValueVariableLabel { get; set; } = string.Empty;    

    public static SPARQLVariableQuery FromXml(XElement e)
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

        return new SPARQLVariableQuery
        {
            Id = e.Element("id")?.Value.Trim() ?? "",
            QueryElement = SPARQLQueryElement.FromXml(e.Element("query")),
            ValueVariable = e.Element("valueVariable")?.Value.Trim() ?? "",
            ValueVariableLabel = e.Element("valueVariableLabel")?.Value.Trim() ?? "",
            Parameters = parameters,
            Attributes = AttributeDictionary.FromXElement(e)
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