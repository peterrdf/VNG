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
import org.apache.jena.sparql.function.FunctionBase5;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * SPARQL extension function:
 *   vng:gmColor(?base64Content, ?r, ?g, ?b, ?t)
 */
public class ColorFunction extends FunctionBase5 {

    private static final Logger LOG = LoggerFactory.getLogger(ColorFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public NodeValue exec(NodeValue base64Content, NodeValue r, NodeValue g, NodeValue b, NodeValue t) {
        if (!base64Content.isString()) {
            throw new ExprEvalException("gmColor: first argument must be a string");
        }
        if (!r.isNumber() || !g.isNumber() || !b.isNumber() || !t.isNumber()) {
            throw new ExprEvalException("gmColor: R, G, B, T must be numeric");
        }

        try {
            String body = form("base64Content", base64Content.getString())
                + "&" + form("R", Double.toString(r.getDouble()))
                + "&" + form("G", Double.toString(g.getDouble()))
                + "&" + form("B", Double.toString(b.getDouble()))
                + "&" + form("T", Double.toString(t.getDouble()));

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateColor");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmColor: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmColor: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmColor: unexpected response: {}", response.body());
                throw new ExprEvalException("gmColor: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmColor: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmColor failed", e);
            throw new ExprEvalException("gmColor failed: " + e.getMessage(), e);
        }
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}