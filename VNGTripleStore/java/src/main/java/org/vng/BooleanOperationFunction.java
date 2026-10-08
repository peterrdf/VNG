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
 *   vng:gmBooleanOperation(?base64Content1, ?base64Content2, ?type)
     type:
        0  Union
        1  Difference
        2  Inverse Difference
        3  Intersection

        4  Union (ignoring any face from secondObject in the end result)
        5  Difference (ignoring any face from secondObject in the end result)
        6  Inverse Difference (ignoring any face from secondObject in the end result)
        7  Intersection (ignoring any face from secondObject in the end result)

        8  Union (ignoring any face exculsively from firstObject in the end result)
        9  Difference (ignoring any face exculsively from firstObject in the end result)
        10  Inverse Difference (ignoring any face exculsively from firstObject in the end result)
        11  Intersection (ignoring any face exculsively from firstObject in the end result)

        12  Union (ignoring any face exculsively from firstObject and secondObject in the end result)
        13  Difference (ignoring any face exculsively from firstObject and secondObject in the end result)
        14  Inverse Difference (ignoring any face exculsively from firstObject and secondObject in the end result)
        15  Intersection (ignoring any face exculsively from firstObject and secondObject in the end result)
 * Calls the VNGService GeometryModeling page handler OnPostCreateBooleanOperation and returns
 * the resulting base64 geometry as xsd:string.
 */
public class BooleanOperationFunction extends FunctionBase3 {

    private static final Logger LOG = LoggerFactory.getLogger(BooleanOperationFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public NodeValue exec(NodeValue base64Content1, NodeValue base64Content2, NodeValue type) {
        if (!base64Content1.isString() || !base64Content2.isString()) {
            throw new ExprEvalException("gmBooleanOperation: first two arguments must be strings");
        }

        if (!type.isInteger()) {
            throw new ExprEvalException("gmBooleanOperation: 'type' must be an integer, got: " + type);
        }

        int operation = type.getInteger().intValueExact();
        if (operation < 0 || operation > 15) {
            throw new ExprEvalException("gmBooleanOperation: 'type' must be in the range 0..15, got: " + operation);
        }

        try {
            String body = form("base64Content1", base64Content1.getString())
                + "&" + form("base64Content2", base64Content2.getString())
                + "&" + form("type", String.valueOf(operation));

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateBooleanOperation");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmBooleanOperation: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmBooleanOperation: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmBooleanOperation: unexpected response: {}", response.body());
                throw new ExprEvalException("gmBooleanOperation: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmBooleanOperation: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmBooleanOperation failed", e);
            throw new ExprEvalException("gmBooleanOperation failed: " + e.getMessage(), e);
        }
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}