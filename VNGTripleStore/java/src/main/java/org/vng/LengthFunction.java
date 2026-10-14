package org.vng;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.*;
import org.apache.jena.sparql.function.FunctionBase1;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;

public class LengthFunction extends FunctionBase1 {

    private static final Logger LOG = LoggerFactory.getLogger(LengthFunction.class);
    private static final HttpClient HTTP_CLIENT = HttpClient.newBuilder()
            .connectTimeout(Duration.ofMinutes(5))
            .build();

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue v) {
        if (!v.isString())
            throw new ExprEvalException("length: expected string, got: " + v);

        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/Geometry?handler=Length");

            String formBody = "base64Content=" + URLEncoder.encode(v.getString(), StandardCharsets.UTF_8);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .header("Content-Type", "application/x-www-form-urlencoded")
                    .POST(HttpRequest.BodyPublishers.ofString(formBody))
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("length: response status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                LOG.error("length: HTTP error {} from service: {}", response.statusCode(), response.body());
                throw new ExprEvalException(
                        "length: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();

            JsonValue lengthValue = json.hasKey("length") ? json.get("length")
                                  : json.hasKey("Length") ? json.get("Length")
                                  : null;

            if (lengthValue == null || !lengthValue.isNumber()) {
                LOG.error("length: unexpected response: {}", response.body());
                throw new ExprEvalException(
                        "length: missing or non-numeric 'length' in response: " + response.body());
            }

            double result = lengthValue.getAsNumber().value().doubleValue();
            return NodeValue.makeDouble(result);

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("length: HTTP call failed", e);
            throw new ExprEvalException("length: HTTP call failed: " + e.getMessage(), e);
        }
    }
}