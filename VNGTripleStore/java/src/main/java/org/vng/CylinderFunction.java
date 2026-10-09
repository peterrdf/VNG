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
 *  vng:gmCylinder(?length, ?radius)
 *  vng:gmCylinder(?length, ?radius, ?segmentationParts)
 *  vng:gmCylinder(?length, ?radius, ?r, ?g, ?b, ?t)
 *  vng:gmCylinder(?length, ?radius, ?segmentationParts, ?r, ?g, ?b, ?t)
 *  -> base64 geometry (xsd:string)
 *  segmentationParts defaults to 36.
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
        int n = args.size();
        if (n != 2 && n != 3 && n != 6 && n != 7)
            throw new QueryBuildException(
                    "gmCylinder: expects 2, 3, 6 or 7 arguments, got " + n);
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        int n = args.size();
        if (n != 2 && n != 3 && n != 6 && n != 7)
            throw new ExprEvalException("gmCylinder: expected 2, 3, 6 or 7 arguments, got " + n);

        NodeValue length = args.get(0);
        NodeValue radius = args.get(1);
        NodeValue segmentationParts;
        int colorIndex;

        switch (n) {
            case 2:
                segmentationParts = NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
                colorIndex = -1;
                break;
            case 3:
                segmentationParts = args.get(2);
                colorIndex = -1;
                break;
            case 6:
                segmentationParts = NodeValue.makeInteger(DEFAULT_SEGMENTATION_PARTS);
                colorIndex = 2;
                break;
            default: // 7
                segmentationParts = args.get(2);
                colorIndex = 3;
                break;
        }

        return exec(length, radius, segmentationParts,
                colorIndex < 0 ? null : args.subList(colorIndex, colorIndex + 4));
    }

    public NodeValue exec(NodeValue length, NodeValue radius, NodeValue segmentationParts) {
        return exec(length, radius, segmentationParts, null);
    }

    private NodeValue exec(NodeValue length, NodeValue radius, NodeValue segmentationParts, List<NodeValue> rgbt) {
        requireNumber("length", length);
        requireNumber("radius", radius);
        requireNumber("segmentationParts", segmentationParts);

        String[] colorNames = { "r", "g", "b", "t" };
        if (rgbt != null) {
            for (int i = 0; i < rgbt.size(); i++)
                requireNumber(colorNames[i], rgbt.get(i));
        }

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            StringBuilder query = new StringBuilder(String.format(Locale.ROOT,
                    "handler=CreateCylinder&length=%s&radius=%s&segmentationParts=%d",
                    length.getDouble(), radius.getDouble(), segmentationParts.getInteger().longValueExact()));

            if (rgbt != null) {
                for (int i = 0; i < rgbt.size(); i++) {
                    query.append(String.format(Locale.ROOT, "&%s=%s", colorNames[i], rgbt.get(i).getDouble()));
                }
            }

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            LOG.debug("gmCylinder: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(TIMEOUT)
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmCylinder: HTTP error {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmCylinder: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmCylinder: unexpected response: {}", response.body());
                throw new ExprEvalException("gmCylinder: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());

        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmCylinder: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmCylinder: HTTP call failed", e);
            throw new ExprEvalException("gmCylinder: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static void requireNumber(String name, NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("gmCylinder: '" + name + "' must be numeric, got: " + v);
    }
}