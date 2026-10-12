package org.vng;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase2;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;

/**
 * SPARQL extension function:
 *   vng:gmProjection2D(?base64Data1, ?base64Data2) -> base64 geometry (xsd:string)
 */
public class Projection2DFunction extends FunctionBase2 {

    private static final Logger LOG = LoggerFactory.getLogger(Projection2DFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(TIMEOUT)
            .build();

    @Override
    public NodeValue exec(NodeValue base64Content1, NodeValue base64Content2) {
        requireString("base64Content1", base64Content1);
        requireString("base64Content2", base64Content2);

        try {
            // The geometries are large, so they are sent in the POST body rather than the query string
            String body = form("base64Content1", base64Content1.getString())
                    + "&" + form("base64Content2", base64Content2.getString());

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateProjection2D");
            LOG.debug("gmProjection2D: POST {}", uri);

            HttpRequest request = HttpRequest.newBuilder(uri)
                    .timeout(TIMEOUT)
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .POST(HttpRequest.BodyPublishers.ofString(body))
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmProjection2D: HTTP error {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmProjection2D: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmProjection2D: unexpected response: {}", response.body());
                throw new ExprEvalException("gmProjection2D: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());

        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmProjection2D: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmProjection2D: HTTP call failed", e);
            throw new ExprEvalException("gmProjection2D: HTTP call failed: " + e.getMessage(), e);
        }
    }

    private static void requireString(String name, NodeValue v) {
        if (!v.isString())
            throw new ExprEvalException("gmProjection2D: '" + name + "' must be a string, got: " + v);
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
                + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}