using VDS.RDF;
using VDS.RDF.Parsing;

namespace VNGPortal.Workflows;

public static class SHACLShapeTurtleDeserializer
{
    private const string Ns = "https://vng.nl/shacl/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    public static SHACLShape? Deserialize(string ttlFilePath)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, ttlFilePath);
        return Deserialize(graph);
    }

    public static SHACLShape? DeserializeFromString(string ttlContent)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, new StringReader(ttlContent));
        return Deserialize(graph);
    }

    public static SHACLShape? Deserialize(IGraph graph)
    {
        var shapeType = graph.CreateUriNode(new Uri(Ns + "Shape"));
        var typePredicate = graph.CreateUriNode(RdfType);

        var shapeNode = graph
            .GetTriplesWithPredicateObject(typePredicate, shapeType)
            .Select(t => t.Subject)
            .FirstOrDefault();

        if (shapeNode == null)
        {
            return null;
        }

        var shape = new SHACLShape
        {
            Id = GetString(graph, shapeNode, "id"),
            Description = GetString(graph, shapeNode, "description")
        };

        // Same as <shape uri="..."> in the XML format (SHACLShape.ShapeUri)
        var shapeUri = GetString(graph, shapeNode, "shapeUri");
        if (shapeUri.Length > 0)
        {
            shape.ShapeElement.Attributes["uri"] = shapeUri;
        }

        // Inline plain SHACL, same as the text of <shape>...</shape> in the XML format (SHACLShape.Shape)
        var inlineShape = GetString(graph, shapeNode, "shape");
        if (inlineShape.Length > 0)
        {
            shape.Shape = inlineShape;
        }

        return shape;
    }

    private static string GetString(IGraph graph, INode subject, string localName)
    {
        var predicate = graph.CreateUriNode(new Uri(Ns + localName));
        var node = graph.GetTriplesWithSubjectPredicate(subject, predicate)
                        .Select(t => t.Object)
                        .FirstOrDefault();

        return node switch
        {
            ILiteralNode literal => literal.Value.Trim(),
            IUriNode uri => uri.Uri.ToString(),
            _ => string.Empty
        };
    }
}