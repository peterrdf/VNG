using System.IO;
using System.Xml.Serialization;
using VNGPortal.Workflows;

public static class SHACLShapeDeserializer
{
    public static SHACLShape Deserialize(string xmlFilePath)
    {
        var serializer = new XmlSerializer(typeof(SHACLShape));
        using var reader = new StreamReader(xmlFilePath);
        return (SHACLShape)serializer.Deserialize(reader);
    }

    public static SHACLShape DeserializeFromString(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(SHACLShape));

        using var reader = new StringReader(xmlContent);
        return (SHACLShape)serializer.Deserialize(reader);
    }
}