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
 *   vng:gmSphere(?radius)
 *   vng:gmSphere(?radius, ?segmentationParts)
 *   vng:gmSphere(?radius, ?r, ?g, ?b, ?t)
 *   vng:gmSphere(?radius, ?segmentationParts, ?r, ?g, ?b, ?t)
 *   -> base64 geometry (xsd:string)
 *   segmentationParts defaults to 36.
 */
public class SphereFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(SphereFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);
    private static final long DEFAULT_SEGMENTATION_PARTS = 36;
    private static final String[] COLOR_NAMES = { "r", "g", "b", "t" };

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(TIMEOUT)
            .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        int n = args.size();
        if (n != 1 && n != 2 && n != 5 && n != 6)
            throw new QueryBuildException(
                "gmSphere: expects 1, 2, 5 or 6 arguments, got " + n);
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        int n = args.size();
        if (n != 1 && n != 2 && n != 5 && n != 6)
            throw new ExprEvalException("gmSphere: expected 1, 2, 5 or 6 arguments, got " + n);

        NodeValue radius = args.get(0);
        NodeValue segmentationParts;
        List<NodeValue> rgbt = null;

        switch (n) {
            case 1:
                segmentationParts = NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
                break;
            case 2:
                segmentationParts = args.get(1);
                break;
            case 5:
                segmentationParts = NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
                rgbt = args.subList(1, 5);
                break;
            default: // 6
                segmentationParts = args.get(1);
                rgbt = args.subList(2, 6);
                break;
        }

        return exec(radius, segmentationParts, rgbt);
    }

    public NodeValue exec(NodeValue radius, NodeValue segmentationParts) {
        return exec(radius, segmentationParts, null);
    }

    private NodeValue exec(NodeValue radius, NodeValue segmentationParts, List<NodeValue> rgbt) {
        requireNumber("radius", radius);
        requireNumber("segmentationParts", segmentationParts);

        if (rgbt != null) {
            for (int i = 0; i < rgbt.size(); i++)
                requireNumber(COLOR_NAMES[i], rgbt.get(i));
        }

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            StringBuilder query = new StringBuilder(String.format(Locale.ROOT,
                    "handler=CreateSphere&radius=%s&segmentationParts=%d",
                    radius.getDouble(), segmentationParts.getInteger().longValueExact()));

            if (rgbt != null) {
                for (int i = 0; i < rgbt.size(); i++) {
                    query.append(String.format(Locale.ROOT, "&%s=%s", COLOR_NAMES[i], rgbt.get(i).getDouble()));
                }
            }

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