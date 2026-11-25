using VDS.RDF;
using VDS.RDF.Parsing;

namespace VNGPortal.Workflows;

internal static class SPARQLTurtleLoader
{
    public const string Ns = "https://vng.nl/sparql/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    public static IGraph LoadFile(string ttlFilePath)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, ttlFilePath);
        return graph;
    }

    public static IGraph LoadString(string ttlContent)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, new StringReader(ttlContent));
        return graph;
    }

    public static INode? FindQueryNode(IGraph graph)
    {
        var typePredicate = graph.CreateUriNode(RdfType);
        var queryType = graph.CreateUriNode(new Uri(Ns + "Query"));

        return graph.GetTriplesWithPredicateObject(typePredicate, queryType)
                    .Select(t => t.Subject)
                    .FirstOrDefault();
    }
}

public static class SPARQLGeometryQueryTurtleDeserializer
{
    public static SPARQLGeometryQuery? Deserialize(string ttlFilePath) =>
        Deserialize(SPARQLTurtleLoader.LoadFile(ttlFilePath));

    public static SPARQLGeometryQuery? DeserializeFromString(string ttlContent) =>
        Deserialize(SPARQLTurtleLoader.LoadString(ttlContent));

    public static SPARQLGeometryQuery? Deserialize(IGraph graph)
    {
        var queryNode = SPARQLTurtleLoader.FindQueryNode(graph);
        return queryNode == null
            ? null
            : SPARQLQueryTurtleReader.ReadGeometryQuery(graph, queryNode, SPARQLTurtleLoader.Ns);
    }
}

public static class SPARQLVariableQueryTurtleDeserializer
{
    public static SPARQLVariableQuery? Deserialize(string ttlFilePath) =>
        Deserialize(SPARQLTurtleLoader.LoadFile(ttlFilePath));

    public static SPARQLVariableQuery? DeserializeFromString(string ttlContent) =>
        Deserialize(SPARQLTurtleLoader.LoadString(ttlContent));

    public static SPARQLVariableQuery? Deserialize(IGraph graph)
    {
        var queryNode = SPARQLTurtleLoader.FindQueryNode(graph);
        return queryNode == null
            ? null
            : SPARQLQueryTurtleReader.ReadVariableQuery(graph, queryNode, SPARQLTurtleLoader.Ns);
    }
}