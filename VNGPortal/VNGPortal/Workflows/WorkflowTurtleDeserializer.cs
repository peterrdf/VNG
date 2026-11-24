using VDS.RDF;
using VDS.RDF.Parsing;

namespace VNGPortal.Workflows;

public static class WorkflowTurtleDeserializer
{
    private const string Ns = "https://vng.nl/workflow/";

    private static readonly Uri RdfType = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    // TTL predicate local name -> Step.Parameters key (same keys the XML format produces)
    private static readonly Dictionary<string, string> ParameterMap = new()
    {
        ["idsFile"] = "ids_file",
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

        return step;
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