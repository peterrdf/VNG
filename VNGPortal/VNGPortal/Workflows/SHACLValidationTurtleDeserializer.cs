using VDS.RDF;
using VDS.RDF.Parsing;

namespace VNGPortal.Workflows;

public static class SHACLValidationTurtleDeserializer
{
    private const string Ns = "https://vng.nl/shacl/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    public static SHACLValidation? Deserialize(string ttlFilePath)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, ttlFilePath);
        return Deserialize(graph);
    }

    public static SHACLValidation? DeserializeFromString(string ttlContent)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, new StringReader(ttlContent));
        return Deserialize(graph);
    }

    public static SHACLValidation? Deserialize(IGraph graph)
    {
        var validationType = graph.CreateUriNode(new Uri(Ns + "Validation"));
        var typePredicate = graph.CreateUriNode(RdfType);

        var validationNode = graph
            .GetTriplesWithPredicateObject(typePredicate, validationType)
            .Select(t => t.Subject)
            .FirstOrDefault();

        if (validationNode == null)
        {
            return null;
        }

        var validation = new SHACLValidation
        {
            Id = GetString(graph, validationNode, "id"),
            Description = GetString(graph, validationNode, "description")
        };

        var shapeUri = GetString(graph, validationNode, "shapeUri");
        if (shapeUri.Length > 0)
        {
            validation.ShapeElement.Attributes["uri"] = shapeUri;
        }

        var viewsHead = GetObject(graph, validationNode, "views");
        if (viewsHead != null)
        {
            // RDF list keeps the original view order
            foreach (var viewNode in graph.GetListItems(viewsHead))
            {
                validation.Views.Add(ReadView(graph, viewNode));
            }
        }

        return validation;
    }

    private static SHACLView ReadView(IGraph graph, INode viewNode)
    {
        var view = new SHACLView
        {
            Id = GetString(graph, viewNode, "id"),
            Type = GetString(graph, viewNode, "type"),
            Visualization = GetString(graph, viewNode, "visualization")
        };

        var queriesHead = GetObject(graph, viewNode, "queries");
        if (queriesHead != null)
        {
            foreach (var queryNode in graph.GetListItems(queriesHead))
            {
                view.Queries.Add(SPARQLQueryTurtleReader.ReadGeometryQuery(graph, queryNode, Ns));
            }
        }

        return view;
    }

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