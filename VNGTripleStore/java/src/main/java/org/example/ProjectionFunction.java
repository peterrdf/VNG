package org.example;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.*;
import org.apache.jena.sparql.function.FunctionBase1;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;

public class ProjectionFunction extends FunctionBase1 {

    private static final Logger LOG = LoggerFactory.getLogger(ProjectionFunction.class);
    private static final HttpClient HTTP_CLIENT;

    static {
        try {
            HTTP_CLIENT = HttpClient.newBuilder()
                    .connectTimeout(Duration.ofMinutes(5))
                    .build();
            LOG.info("ProjectionFunction: initialized OK");
        } catch (Throwable t) {
            LoggerFactory.getLogger(ProjectionFunction.class)
                    .error("ProjectionFunction: static initializer FAILED — {}", t.toString(), t);
            throw t;
        }
    }

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue v) {
        if (!v.isString())
            throw new ExprEvalException("projection: expected string, got: " + v);

        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/Geometry?handler=Projection");

            String formBody = "base64Content=" + URLEncoder.encode(v.getString(), StandardCharsets.UTF_8);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .POST(HttpRequest.BodyPublishers.ofString(formBody))
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("projection: response status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                LOG.error("projection: HTTP error {} from service: {}", response.statusCode(), response.body());
                throw new ExprEvalException(
                        "projection: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            // Jena built-in JSON
            JsonObject json = JSON.parse(response.body()).getAsObject();

            // Case-insensitive: handles both "projection" (camelCase) and "Projection" (PascalCase)
            JsonValue projectionValue = json.hasKey("projection") ? json.get("projection")
                                  : json.hasKey("Projection") ? json.get("Projection")
                                  : null;

            if (projectionValue == null || !projectionValue.isString()) {
                LOG.error("projection: unexpected response: {}", response.body());
                throw new ExprEvalException(
                        "projection: missing or non-string 'projection' in response: " + response.body());
            }

            String result = projectionValue.getAsString().value();
            return NodeValue.makeString(result);

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("projection: HTTP call failed", e);
            throw new ExprEvalException("projection: HTTP call failed: " + e.getMessage(), e);
        }
    }
}