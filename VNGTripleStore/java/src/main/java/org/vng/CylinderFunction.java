package org.vng;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.ExprList;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase;
import org.apache.jena.query.QueryBuildException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.Locale;

/**
 * SPARQL: vng:csgCylinder(?length, ?radius [, ?segmentationParts = 36]) -> base64 geometry (xsd:string)
 */
public class CylinderFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(CylinderFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);
    private static final long DEFAULT_SEGMENTATION_PARTS = 36;

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(TIMEOUT)
            .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        if (args.size() < 2 || args.size() > 3)
            throw new QueryBuildException(
                    "csgCylinder: expects 2 or 3 arguments, got " + args.size());
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        NodeValue segmentationParts = args.size() > 2
                ? args.get(2)
                : NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
        return exec(args.get(0), args.get(1), segmentationParts);
    }

    public NodeValue exec(NodeValue length, NodeValue radius, NodeValue segmentationParts) {
        requireNumber("length", length);
        requireNumber("radius", radius);
        requireNumber("segmentationParts", segmentationParts);

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            String query = String.format(Locale.ROOT,
                    "handler=CreateCylinder&length=%s&radius=%s&segmentationParts=%d",
                    length.getDouble(), radius.getDouble(), segmentationParts.getInteger().longValueExact());

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            LOG.debug("csgCylinder: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(TIMEOUT)
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("csgCylinder: HTTP error {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("csgCylinder: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("csgCylinder: unexpected response: {}", response.body());
                throw new ExprEvalException("csgCylinder: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());

        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("csgCylinder: interrupted", e);
        } catch (Exception e) {
            LOG.error("csgCylinder: HTTP call failed", e);
            throw new ExprEvalException("csgCylinder: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static void requireNumber(String name, NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("csgCylinder: '" + name + "' must be numeric, got: " + v);
    }
}