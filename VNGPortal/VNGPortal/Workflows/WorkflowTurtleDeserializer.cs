using VDS.RDF;
using VDS.RDF.Parsing;
using System.Xml.Linq;

namespace VNGPortal.Workflows;

public static class WorkflowTurtleDeserializer
{
    private const string Ns = "https://vng.nl/workflow/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    // TTL predicate local name -> Step.Parameters key (same keys the XML format produces)
    private static readonly Dictionary<string, string> ParameterMap = new()
    {
        ["idsFile"] = "idsFile",
        ["queryRef"] = "queryRef",
        ["shapeRef"] = "shapeRef"
    };

    public static Workflow? Deserialize(string ttlFilePath)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, ttlFilePath);
        return Deserialize(graph);
    }

    public static Workflow? DeserializeFromString(string ttlContent)
    {
        var graph = new Graph();
        new TurtleParser().Load(graph, new StringReader(ttlContent));
        return Deserialize(graph);
    }

    public static Workflow? Deserialize(IGraph graph)
    {
        var workflowType = graph.CreateUriNode(new Uri(Ns + "Workflow"));
        var typePredicate = graph.CreateUriNode(RdfType);

        var workflowNode = graph
            .GetTriplesWithPredicateObject(typePredicate, workflowType)
            .Select(t => t.Subject)
            .FirstOrDefault();

        if (workflowNode == null)
        {
            return null;
        }

        var workflow = new Workflow
        {
            Id = GetString(graph, workflowNode, "id"),
            Name = GetString(graph, workflowNode, "name"),
            Description = GetString(graph, workflowNode, "description")
        };

        var stepsHead = GetObject(graph, workflowNode, "steps");
        if (stepsHead != null)
        {
            // RDF list keeps the original step order
            foreach (var stepNode in graph.GetListItems(stepsHead))
            {
                workflow.Steps.Add(ReadStep(graph, stepNode));
            }
        }

        return workflow;
    }

    private static Step ReadStep(IGraph graph, INode stepNode)
    {
        var step = new Step
        {
            Id = GetString(graph, stepNode, "id"),
            Name = GetString(graph, stepNode, "name"),
            Description = GetString(graph, stepNode, "description"),
            Type = GetString(graph, stepNode, "type")
        };

        foreach (var (predicateName, parameterKey) in ParameterMap)
        {
            var value = GetString(graph, stepNode, predicateName);
            if (value.Length > 0)
            {
                step.Parameters[parameterKey] = value;
            }
        }

        var queriesXml = BuildQueriesXml(graph, stepNode);
        if (queriesXml.Length > 0)
        {
            step.Parameters["queries"] = queriesXml;
        }

        return step;
    }

    /// <summary>
    /// Rebuilds the inner XML of &lt;queries&gt; (a sequence of &lt;sparql&gt; elements),
    /// identical in shape to what ParameterDictionary.ReadXml produces from the XML format.
    /// </summary>
    private static string BuildQueriesXml(IGraph graph, INode stepNode)
    {
        var queriesHead = GetObject(graph, stepNode, "queries");
        if (queriesHead == null)
        {
            return string.Empty;
        }

        var sparqlElements = new List<XElement>();
        foreach (var queryNode in graph.GetListItems(queriesHead))
        {
            var sparql = new XElement("sparql",
                new XElement("id", new XCData(GetString(graph, queryNode, "id"))),
                new XElement("query", new XCData(GetString(graph, queryNode, "query"))));

            AddOptional(graph, queryNode, sparql, "geometryVariable");
            AddOptional(graph, queryNode, sparql, "zoomTo");
            AddOptional(graph, queryNode, sparql, "valueVariable");
            AddOptional(graph, queryNode, sparql, "valueVariableLabel");

            sparqlElements.Add(sparql);
        }

        return string.Concat(sparqlElements.Select(e => e.ToString()));
    }

    private static void AddOptional(IGraph graph, INode queryNode, XElement sparql, string localName)
    {
        var value = GetString(graph, queryNode, localName);
        if (value.Length > 0)
        {
            sparql.Add(new XElement(localName, new XCData(value)));
        }
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