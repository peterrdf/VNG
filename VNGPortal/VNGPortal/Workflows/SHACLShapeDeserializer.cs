using System.IO;
using System.Xml.Serialization;
using VNGPortal.Workflows;

public static class SHACLShapeDeserializer
{
    public static SHACLValidation? Deserialize(string xmlFilePath)
    {
        var serializer = new XmlSerializer(typeof(SHACLValidation));
        using var reader = new StreamReader(xmlFilePath);
        return (SHACLValidation?)serializer.Deserialize(reader);
    }

    public static SHACLValidation? DeserializeFromString(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(SHACLValidation));

        using var reader = new StringReader(xmlContent);
        return (SHACLValidation?)serializer.Deserialize(reader);
    }
}