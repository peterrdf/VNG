package org.example;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonArray;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.graph.Node;
import org.apache.jena.sparql.core.Var;
import org.apache.jena.sparql.engine.ExecutionContext;
import org.apache.jena.sparql.engine.QueryIterator;
import org.apache.jena.sparql.engine.binding.Binding;
import org.apache.jena.sparql.engine.binding.BindingFactory;
import org.apache.jena.sparql.engine.iterator.QueryIterPlainWrapper;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.pfunction.PropFuncArg;
import org.apache.jena.sparql.pfunction.PropertyFunctionBase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.List;

public class BBoxPropFunction extends PropertyFunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(BBoxPropFunction.class);

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(Duration.ofMinutes(5))
            .build();

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    /** Expected: minX, minY, minZ, maxX, maxY, maxZ */
    private static final int BBOX_SIZE = 6;

    @Override
    public QueryIterator exec(Binding binding,
                              PropFuncArg argSubject,   // (?minX ?minY ?minZ ?maxX ?maxY ?maxZ)
                              Node predicate,
                              PropFuncArg argObject,    // ?content  (base64 string)
                              ExecutionContext execCxt) {

        // --- resolve the input (base64 content) ---
        Node contentNode = argObject.getArg();
        if (contentNode.isVariable()) {
            contentNode = binding.get(Var.alloc(contentNode));
        }
        if (contentNode == null || !contentNode.isLiteral()) {
            throw new ExprEvalException("BBoxPropFunction: object must be a bound string literal");
        }
        String base64Content = contentNode.getLiteralLexicalForm();

        // --- validate output variable list ---
        if (!argSubject.isList() || argSubject.getArgList().size() != BBOX_SIZE) {
            throw new ExprEvalException(
                    "BBoxPropFunction: subject must be a list of exactly " + BBOX_SIZE
                    + " variables: (?minX ?minY ?minZ ?maxX ?maxY ?maxZ)");
        }
        List<Node> outVars = argSubject.getArgList();

        // --- call the service ---
        double[] bbox = callService(base64Content);

        // --- build one result binding ---
        Binding result = binding;
        for (int i = 0; i < BBOX_SIZE; i++) {
            Node varNode = outVars.get(i);
            if (!varNode.isVariable()) {
                throw new ExprEvalException(
                        "BBoxPropFunction: subject list slot " + i + " must be a variable, got: " + varNode);
            }
            Node value = NodeValue.makeDouble(bbox[i]).asNode();
            result = BindingFactory.binding(result, Var.alloc(varNode), value);
        }

        return QueryIterPlainWrapper.create(List.of(result).iterator(), execCxt);
    }

    /**
     * Calls the BBox service and returns [minX, minY, minZ, maxX, maxY, maxZ].
     */
    private double[] callService(String base64Content) {
        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/Geometry?handler=BBox");
            String formBody = "base64Content=" + URLEncoder.encode(base64Content, StandardCharsets.UTF_8);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .POST(HttpRequest.BodyPublishers.ofString(formBody))
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("BBoxPropFunction: status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                throw new ExprEvalException(
                        "BBoxPropFunction: HTTP error " + response.statusCode() + ": " + response.body());
            }

            // Parse JSON array: [minX, minY, minZ, maxX, maxY, maxZ]
            JsonValue parsed = JSON.parseAny(response.body());
            if (!parsed.isArray()) {
                throw new ExprEvalException(
                        "BBoxPropFunction: expected JSON array, got: " + response.body());
            }
            JsonArray arr = parsed.getAsArray();
            if (arr.size() != BBOX_SIZE) {
                throw new ExprEvalException(
                        "BBoxPropFunction: expected array of " + BBOX_SIZE + " values, got: " + arr.size());
            }

            double[] bbox = new double[BBOX_SIZE];
            for (int i = 0; i < BBOX_SIZE; i++) {
                bbox[i] = arr.get(i).getAsNumber().value().doubleValue();
            }
            return bbox;

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("BBoxPropFunction: HTTP call failed", e);
            throw new ExprEvalException("BBoxPropFunction: HTTP call failed: " + e.getMessage(), e);
        }
    }
}