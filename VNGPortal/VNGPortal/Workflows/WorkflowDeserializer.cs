using System.IO;
using System.Xml.Serialization;
using VNGPortal.Workflows;

public static class WorkflowDeserializer
{
    public static Workflow Deserialize(string xmlFilePath)
    {
        var serializer = new XmlSerializer(typeof(Workflow));

        using var reader = new StreamReader(xmlFilePath);
        return (Workflow)serializer.Deserialize(reader);
    }

    public static Workflow DeserializeFromString(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(Workflow));

        using var reader = new StringReader(xmlContent);
        return (Workflow)serializer.Deserialize(reader);
    }
}