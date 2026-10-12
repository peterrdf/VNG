package org.vng;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase4;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.Locale;

/**
 * SPARQL extension function:
 *   vng:gmPlane(?a, ?b, ?c, ?d) -> base64 geometry (xsd:string)
 */
public class PlaneFunction extends FunctionBase4 {

    private static final Logger LOG = LoggerFactory.getLogger(PlaneFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(TIMEOUT)
            .build();

    @Override
    public NodeValue exec(NodeValue a, NodeValue b, NodeValue c, NodeValue d) {
        requireNumber("A", a);
        requireNumber("B", b);
        requireNumber("C", c);
        requireNumber("D", d);

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            String query = String.format(Locale.ROOT,
                    "handler=CreatePlane&A=%s&B=%s&C=%s&D=%s",
                    a.getDouble(), b.getDouble(), c.getDouble(), d.getDouble());

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            LOG.debug("gmPlane: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(TIMEOUT)
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmPlane: HTTP error {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmPlane: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmPlane: unexpected response: {}", response.body());
                throw new ExprEvalException("gmPlane: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());

        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmPlane: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmPlane: HTTP call failed", e);
            throw new ExprEvalException("gmPlane: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static void requireNumber(String name, NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("gmPlane: '" + name + "' must be numeric, got: " + v);
    }
}