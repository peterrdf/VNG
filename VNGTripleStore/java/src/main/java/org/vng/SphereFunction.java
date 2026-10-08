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
 * SPARQL extension function:
 *   vng:gmSphere(?radius [, ?segmentationParts = 36]) -> base64 geometry (xsd:string)
 */
public class SphereFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(SphereFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);
    private static final long DEFAULT_SEGMENTATION_PARTS = 36;

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(TIMEOUT)
            .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        if (args.size() < 1 || args.size() > 2)
            throw new QueryBuildException(
                "gmSphere: expects 1 or 2 arguments, got " + args.size());
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        NodeValue segmentationParts = args.size() > 1
                ? args.get(1)
                : NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
        return exec(args.get(0), segmentationParts);
    }

    public NodeValue exec(NodeValue radius, NodeValue segmentationParts) {
        requireNumber("radius", radius);
        requireNumber("segmentationParts", segmentationParts);

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            String query = String.format(Locale.ROOT,
                    "handler=CreateSphere&radius=%s&segmentationParts=%d",
                    radius.getDouble(), segmentationParts.getInteger().longValueExact());

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            LOG.debug("gmSphere: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(TIMEOUT)
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmSphere: HTTP error {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmSphere: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmSphere: unexpected response: {}", response.body());
                throw new ExprEvalException("gmSphere: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());

        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmSphere: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmSphere: HTTP call failed", e);
            throw new ExprEvalException("gmSphere: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static void requireNumber(String name, NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("gmSphere: '" + name + "' must be numeric, got: " + v);
    }
}