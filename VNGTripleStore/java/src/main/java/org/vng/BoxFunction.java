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
import org.apache.jena.sparql.function.FunctionBase3;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * SPARQL extension function:
 *   vng:gmBox(?length, ?width, ?height)
 */
public class BoxFunction extends FunctionBase3 {

    private static final Logger LOG = LoggerFactory.getLogger(BoxFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public NodeValue exec(NodeValue length, NodeValue width, NodeValue height) {
        if (!length.isNumber() || !width.isNumber() || !height.isNumber()) {
            throw new ExprEvalException("gmBox: length, width, height must be numeric");
        }

        try {
            String query = param("handler", "CreateBox")
                + "&" + param("length", Double.toString(length.getDouble()))
                + "&" + param("width", Double.toString(width.getDouble()))
                + "&" + param("height", Double.toString(height.getDouble()));

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .GET()
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmBox: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmBox: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmBox: unexpected response: {}", response.body());
                throw new ExprEvalException("gmBox: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmBox: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmBox failed", e);
            throw new ExprEvalException("gmBox failed: " + e.getMessage(), e);
        }
    }

    private static String param(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}