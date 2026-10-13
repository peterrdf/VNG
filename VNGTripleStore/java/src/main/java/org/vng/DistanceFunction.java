package org.vng;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.*;
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

public class DistanceFunction extends FunctionBase2 {

    private static final Logger LOG = LoggerFactory.getLogger(DistanceFunction.class);
    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(Duration.ofMinutes(5))
            .build();

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue v1, NodeValue v2) {
        if (!v1.isString())
            throw new ExprEvalException("distance: expected string for argument 1, got: " + v1);
        if (!v2.isString())
            throw new ExprEvalException("distance: expected string for argument 2, got: " + v2);

        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/Geometry?handler=Distance");

            String formBody = "base64Content1=" + URLEncoder.encode(v1.getString(), StandardCharsets.UTF_8)
                    + "&base64Content2=" + URLEncoder.encode(v2.getString(), StandardCharsets.UTF_8);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .POST(HttpRequest.BodyPublishers.ofString(formBody))
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("distance: response status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                LOG.error("distance: HTTP error {} from service: {}", response.statusCode(), response.body());
                throw new ExprEvalException(
                        "distance: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();

            JsonValue distanceValue = json.hasKey("distance") ? json.get("distance")
                                    : json.hasKey("Distance") ? json.get("Distance")
                                    : null;

            if (distanceValue == null || !distanceValue.isNumber()) {
                LOG.error("distance: unexpected response: {}", response.body());
                throw new ExprEvalException(
                        "distance: missing or non-numeric 'distance' in response: " + response.body());
            }

            double result = distanceValue.getAsNumber().value().doubleValue();
            return NodeValue.makeDouble(result);

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("distance: HTTP call failed", e);
            throw new ExprEvalException("distance: HTTP call failed: " + e.getMessage(), e);
        }
    }
}