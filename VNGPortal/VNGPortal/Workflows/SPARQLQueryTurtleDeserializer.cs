using VDS.RDF;
using VDS.RDF.Parsing;

namespace VNGPortal.Workflows;

public static class SPARQLQueryTurtleDeserializer
{
    private const string Ns = "https://vng.nl/sparql/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    public static SPARQLQuery? Deserialize(string ttlFilePath)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, ttlFilePath);
        return Deserialize(graph);
    }

    public static SPARQLQuery? DeserializeFromString(string ttlContent)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, new StringReader(ttlContent));
        return Deserialize(graph);
    }

    public static SPARQLQuery? Deserialize(IGraph graph)
    {
        var queryType = graph.CreateUriNode(new Uri(Ns + "Query"));
        var typePredicate = graph.CreateUriNode(RdfType);

        var queryNode = graph
            .GetTriplesWithPredicateObject(typePredicate, queryType)
            .Select(t => t.Subject)
            .FirstOrDefault();

        if (queryNode == null)
        {
            return null;
        }

        var sparqlQuery = new SPARQLQuery
        {
            Id = GetString(graph, queryNode, "id")
        };

        // Same as <query>...</query> text in the XML format (SPARQLQuery.Query trims on read)
        sparqlQuery.Query = GetString(graph, queryNode, "query");

        // Optional: same as <query uri="..."> in the XML format (SPARQLQuery.QueryUri)
        var queryUri = GetString(graph, queryNode, "queryUri");
        if (queryUri.Length > 0)
        {
            sparqlQuery.QueryElement.Attributes["uri"] = queryUri;
        }

        return sparqlQuery;
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