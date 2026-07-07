package org.example;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonArray;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.graph.Node;
import org.apache.jena.graph.NodeFactory;
import org.apache.jena.sparql.core.Var;
import org.apache.jena.sparql.engine.ExecutionContext;
import org.apache.jena.sparql.engine.QueryIterator;
import org.apache.jena.sparql.engine.binding.Binding;
import org.apache.jena.sparql.engine.binding.BindingBuilder;
import org.apache.jena.sparql.engine.iterator.QueryIterPlainWrapper;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.pfunction.PropFuncArg;
import org.apache.jena.sparql.pfunction.PropertyFunctionBase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.ArrayList;
import java.util.List;

public class GetPDOKFeaturePropFunction extends PropertyFunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(GetPDOKFeaturePropFunction.class);
    private static final HttpClient HTTP_CLIENT;
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    static {
        HTTP_CLIENT = HttpClient.newBuilder()
                .connectTimeout(Duration.ofMinutes(5))
                .build();
        LOG.info("GetPDOKFeaturePropFunction: initialized OK");
    }

    @Override
    public QueryIterator exec(Binding binding, PropFuncArg argSubject,
                               Node predicate, PropFuncArg argObject,
                               ExecutionContext execCxt) {

        // Subject: (?feature ?eastings ?northings)
        List<Node> subjArgs = argSubject.getArgList();
        if (subjArgs.size() != 3)
            throw new ExprEvalException(
                    "pdokFeature: subject must be (?eastings ?northings), got " + subjArgs.size());

        // Object: (?id ?name ?geometry)
        List<Node> objArgs = argObject.getArgList();
        if (objArgs.size() != 3)
            throw new ExprEvalException(
                    "pdokFeature: object must be (?id ?name ?geometry), got " + objArgs.size());

        // Resolve any variables in the subject from the current binding
        Node featureNode = resolve(subjArgs.get(0), binding);
        Node eastNode  = resolve(subjArgs.get(1), binding);
        Node northNode = resolve(subjArgs.get(2), binding);

        if (featureNode == null || eastNode == null || northNode == null)
            throw new ExprEvalException("pdokFeature: unbound feature, eastings or northings");

        String feature = NodeValue.makeNode(featureNode).getString();
        double eastings  = NodeValue.makeNode(eastNode).getDouble();
        double northings = NodeValue.makeNode(northNode).getDouble();

        Var idVar   = (Var) objArgs.get(0);
        Var nameVar = (Var) objArgs.get(1);
        Var geometryVar = (Var) objArgs.get(2);

        try {
            // Use %.2f to avoid scientific notation (e.g. 5.3E5) confusing the ASP.NET handler
            URI uri = URI.create(SERVICE_BASE_URL + "/LandRegistry?handler=" + feature
                    + "&eastings="  + String.format("%.2f", eastings)
                    + "&northings=" + String.format("%.2f", northings));
            LOG.debug("pdokFeature: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("pdokFeature: status={} Content-Type={}",
                    response.statusCode(),
                    response.headers().firstValue("Content-Type").orElse("(none)"));
            LOG.debug("pdokFeature: body (first 500): {}",
                    response.body().substring(0, Math.min(500, response.body().length())));

            if (response.statusCode() != 200)
                throw new ExprEvalException("pdokFeature: HTTP error " + response.statusCode()
                        + " body: " + response.body());

            // Guard: ASP.NET Core returns 200 HTML on misconfigured routes
            String contentType = response.headers()
                    .firstValue("Content-Type").orElse("(none)");
            if (!contentType.contains("json")) {
                String preview = response.body()
                        .substring(0, Math.min(300, response.body().length()))
                        .replaceAll("\\s+", " ");
                LOG.error("pdokFeature: expected JSON but got Content-Type='{}', body: {}",
                        contentType, preview);
                throw new ExprEvalException(
                        "pdokFeature: service returned Content-Type='" + contentType
                        + "' (expected application/json). "
                        + "Check handler=PDOKFeature exists. Body starts: " + preview);
            }

            JsonArray features = JSON.parseAny(response.body()).getAsArray();
            LOG.info("pdokFeature: features={} eastings={} northings={} => {} feature(s) returned",
                    feature, eastings, northings, features.size());

            // One Binding per feature → one row per feature in SPARQL results
            List<Binding> bindings = new ArrayList<>();
            for (JsonValue bindingValue : features) {
                JsonObject bindingObject = bindingValue.getAsObject();
                BindingBuilder bindingBuilder = BindingBuilder.create(binding);
                bindingBuilder.add(idVar,   NodeFactory.createLiteralString(field(bindingObject, "id",   "Id")));
                bindingBuilder.add(nameVar, NodeFactory.createLiteralString(field(bindingObject, "name", "Name")));
                bindingBuilder.add(geometryVar, NodeFactory.createLiteralString(field(bindingObject, "geometry", "Geometry")));
                bindings.add(bindingBuilder.build());
            }

            return QueryIterPlainWrapper.create(bindings.iterator(), execCxt);

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("pdokFeature: HTTP call failed", e);
            throw new ExprEvalException("pdokFeature: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static Node resolve(Node node, Binding binding) {
        return Var.isVar(node) ? binding.get((Var) node) : node;
    }

    // Handles both camelCase ("name") and PascalCase ("Name") from ASP.NET Core
    private static String field(JsonObject obj, String camel, String pascal) {
        JsonValue v = obj.hasKey(camel)  ? obj.get(camel)
                    : obj.hasKey(pascal) ? obj.get(pascal)
                    : null;
        if (v == null)
            throw new ExprEvalException(
                    "pdokFeature: missing field '" + camel + "' in feature object");
        return v.getAsString().value();
    }
}