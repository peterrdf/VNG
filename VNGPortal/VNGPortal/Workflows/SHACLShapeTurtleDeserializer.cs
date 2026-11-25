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
            Id = GetString(graph, shapeNode, "id")
        };

        // Same as <shape uri="..."> in the XML format (SHACLShape.ShapeUri)
        var shapeUri = GetString(graph, shapeNode, "shapeUri");
        if (shapeUri.Length > 0)
        {
            shape.ShapeElement.Attributes["uri"] = shapeUri;
        }

        var viewsHead = GetObject(graph, shapeNode, "views");
        if (viewsHead != null)
        {
            // RDF list keeps the original view order
            foreach (var viewNode in graph.GetListItems(viewsHead))
            {
                shape.Views.Add(ReadView(graph, viewNode));
            }
        }

        return shape;
    }

    private static SHACLView ReadView(IGraph graph, INode viewNode)
    {
        var view = new SHACLView
        {
            Type = GetString(graph, viewNode, "type"),
            Visualization = GetString(graph, viewNode, "visualization")
        };

        var queriesHead = GetObject(graph, viewNode, "queries");
        if (queriesHead != null)
        {
            foreach (var queryNode in graph.GetListItems(queriesHead))
            {
                view.Queries.Add(ReadQuery(graph, queryNode));
            }
        }

        return view;
    }

    private static SPARQLGeometryQuery ReadQuery(IGraph graph, INode queryNode) =>
        SPARQLQueryTurtleReader.ReadGeometryQuery(graph, queryNode, Ns);

    private static INode? GetObject(IGraph graph, INode subject, string localName)
    {
        var predicate = graph.CreateUriNode(new Uri(Ns + localName));
        return graph.GetTriplesWithSubjectPredicate(subject, predicate)
                    .Select(t => t.Object)
                    .FirstOrDefault();
    }

    private static string GetString(IGraph graph, INode subject, string localName)
    {
        return GetObject(graph, subject, localName) switch
        {
            ILiteralNode literal => literal.Value.Trim(),
            IUriNode uri => uri.Uri.ToString(),
            _ => string.Empty
        };
    }
}