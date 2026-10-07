package org.vng;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase4;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * SPARQL extension function:
 *   vng:csgRotation(?base64Content, ?alpha, ?beta, ?gamma)
 * Calls the VNGService CSG page handler OnPostCreateRotation and returns
 * the updated base64 geometry as xsd:string.
 */
public class RotationFunction extends FunctionBase4 {

    private static final Logger LOG = LoggerFactory.getLogger(RotationFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public NodeValue exec(NodeValue base64Content, NodeValue alpha, NodeValue beta, NodeValue gamma) {
        if (!base64Content.isString()) {
            throw new ExprEvalException("csgRotation: first argument must be a string");
        }
        if (!alpha.isNumber() || !beta.isNumber() || !gamma.isNumber()) {
            throw new ExprEvalException("csgRotation: alpha, beta, gamma must be numeric");
        }

        try {
            String body = form("base64Content", base64Content.getString())
                + "&" + form("alpha", Double.toString(alpha.getDouble()))
                + "&" + form("beta", Double.toString(beta.getDouble()))
                + "&" + form("gamma", Double.toString(gamma.getDouble()));

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateRotation");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("csgRotation: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("csgRotation: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("csgRotation: unexpected response: {}", response.body());
                throw new ExprEvalException("csgRotation: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("csgRotation: interrupted", e);
        } catch (Exception e) {
            LOG.error("csgRotation failed", e);
            throw new ExprEvalException("csgRotation failed: " + e.getMessage(), e);
        }
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}