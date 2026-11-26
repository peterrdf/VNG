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
            Id = GetString(graph, queryNode, "id"),
            Description = GetString(graph, queryNode, "description")
        };

        // Same as <query>...</query> text in the XML format (SPARQLQuery.Query trims on read)
        sparqlQuery.Query = GetString(graph, queryNode, "query");

        // Optional: same as <query uri="..."> in the XML format (SPARQLQuery.QueryUri)
        var queryUri = GetString(graph, queryNode, "queryUri");
        if (queryUri.Length > 0)
        {
            sparqlQuery.QueryElement.Attributes["uri"] = queryUri;
        }

        // Optional: sp:parameters ( [ ... ] [ ... ] )
        sparqlQuery.ParameterList = GetParameters(graph, queryNode);

        return sparqlQuery;
    }

    private static List<SPARQLQueryParameter> GetParameters(IGraph graph, INode queryNode)
    {
        var result = new List<SPARQLQueryParameter>();

        var predicate = graph.CreateUriNode(new Uri(Ns + "parameters"));
        var listRoot = graph.GetTriplesWithSubjectPredicate(queryNode, predicate)
                            .Select(t => t.Object)
                            .FirstOrDefault();

        if (listRoot == null)
        {
            return result;
        }

        foreach (var item in graph.GetListItems(listRoot))
        {
            result.Add(new SPARQLQueryParameter
            {
                Type = GetString(graph, item, "type"),
                Name = GetString(graph, item, "name"),
                Value = GetString(graph, item, "value"),
                RegExpr = GetString(graph, item, "regExpr"),
                RegExprGroup = GetString(graph, item, "regExprGroup"),
                Target = GetString(graph, item, "target")
            });
        }

        return result;
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