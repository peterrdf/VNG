using VDS.RDF;

namespace VNGPortal.Workflows;

/// <summary>
/// Reads a single query node (id, query text/uri, parameters, view variables) from an RDF graph.
/// The vocabulary namespace is passed in, so the same code serves sp:, shacl: and wf: Turtle files.
/// </summary>
public static class SPARQLQueryTurtleReader
{
    // Same keys the XML format stores in <parameters>
    private static readonly string[] ParameterNames =
    {
        "templateArgRegExpr",
        "templateArgRegExprMatch",
        "templateArgName"
    };

    public static SPARQLGeometryQuery ReadGeometryQuery(IGraph graph, INode queryNode, string ns)
    {
        var query = new SPARQLGeometryQuery
        {
            GeometryVariable = GetString(graph, queryNode, ns, "geometryVariable"),
            ZoomTo = bool.TryParse(GetString(graph, queryNode, ns, "zoomTo"), out var zoomTo) && zoomTo
        };

        ReadCommon(graph, queryNode, ns, query);
        return query;
    }

    public static SPARQLVariableQuery ReadVariableQuery(IGraph graph, INode queryNode, string ns)
    {
        var query = new SPARQLVariableQuery
        {
            ValueVariable = GetString(graph, queryNode, ns, "valueVariable"),
            ValueVariableLabel = GetString(graph, queryNode, ns, "valueVariableLabel")
        };

        ReadCommon(graph, queryNode, ns, query);
        return query;
    }

    private static void ReadCommon(IGraph graph, INode queryNode, string ns, SPARQLQuery query)
    {
        query.Id = GetString(graph, queryNode, ns, "id");

        // <query>...</query> text
        var inlineQuery = GetString(graph, queryNode, ns, "query");
        if (inlineQuery.Length > 0)
        {
            query.Query = inlineQuery;
        }

        // <query uri="..."> attribute
        var queryUri = GetString(graph, queryNode, ns, "queryUri");
        if (queryUri.Length > 0)
        {
            query.QueryElement.Attributes["uri"] = queryUri;
        }

        // <parameters>
        foreach (var parameterName in ParameterNames)
        {
            var value = GetString(graph, queryNode, ns, parameterName);
            if (value.Length > 0)
            {
                query.Parameters[parameterName] = value;
            }
        }
    }

    public static string GetString(IGraph graph, INode subject, string ns, string localName)
    {
        var predicate = graph.CreateUriNode(new Uri(ns + localName));
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