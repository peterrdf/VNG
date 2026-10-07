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
 *   vng:csgTranslation(?base64Content, ?x, ?y, ?z)
 * Calls the VNGService CSG page handler OnPostCreateTranslation and returns
 * the updated base64 geometry as xsd:string.
 */
public class TranslationFunction extends FunctionBase4 {

    private static final Logger LOG = LoggerFactory.getLogger(TranslationFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public NodeValue exec(NodeValue base64Content, NodeValue x, NodeValue y, NodeValue z) {
        if (!base64Content.isString()) {
            throw new ExprEvalException("csgTranslation: first argument must be a string");
        }
        if (!x.isNumber() || !y.isNumber() || !z.isNumber()) {
            throw new ExprEvalException("csgTranslation: x, y, z must be numeric");
        }

        try {
            String body = form("base64Content", base64Content.getString())
                + "&" + form("_41", Double.toString(x.getDouble()))
                + "&" + form("_42", Double.toString(y.getDouble()))
                + "&" + form("_43", Double.toString(z.getDouble()));

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateTranslation");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("csgTranslation: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("csgTranslation: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("csgTranslation: unexpected response: {}", response.body());
                throw new ExprEvalException("csgTranslation: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("csgTranslation: interrupted", e);
        } catch (Exception e) {
            LOG.error("csgTranslation failed", e);
            throw new ExprEvalException("csgTranslation failed: " + e.getMessage(), e);
        }
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}