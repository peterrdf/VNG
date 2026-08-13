using System.IO;
using System.Xml.Serialization;
using VNGPortal.Workflows;

public static class SPARQLQueryDeserializer
{
    public static SPARQLQuery Deserialize(string xmlFilePath)
    {
        var serializer = new XmlSerializer(typeof(SPARQLQuery));
        using var reader = new StreamReader(xmlFilePath);
        return (SPARQLQuery)serializer.Deserialize(reader);
    }

    public static SPARQLQuery DeserializeFromString(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(SPARQLQuery));

        using var reader = new StringReader(xmlContent);
        return (SPARQLQuery)serializer.Deserialize(reader);
    }
}