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

public class CubeVolumeFunction extends FunctionBase1 {

    private static final Logger LOG = LoggerFactory.getLogger(CubeVolumeFunction.class);
    private static final HttpClient HTTP_CLIENT;

    static {
        try {
            HTTP_CLIENT = HttpClient.newBuilder()
                    .connectTimeout(Duration.ofMinutes(5))
                    .build();
            LOG.info("CubeVolumeFunction: initialized OK");
        } catch (Throwable t) {
            LoggerFactory.getLogger(CubeVolumeFunction.class)
                    .error("CubeVolumeFunction: static initializer FAILED — {}", t.toString(), t);
            throw t;
        }
    }

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("cubeVolume: expected number, got: " + v);

        LOG.debug("cubeVolume: calling service with length={}", v.getDouble());

        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/Calculate?handler=CubeVolume&length=" + v.getDouble());
            LOG.debug("cubeVolume: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("cubeVolume: response status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                LOG.error("cubeVolume: HTTP error {} from service: {}", response.statusCode(), response.body());
                throw new ExprEvalException(
                        "cubeVolume: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            // Jena built-in JSON
            JsonObject json = JSON.parse(response.body()).getAsObject();

            // Case-insensitive: handles both "volume" (camelCase) and "Volume" (PascalCase)
            JsonValue volumeValue = json.hasKey("volume") ? json.get("volume")
                                  : json.hasKey("Volume") ? json.get("Volume")
                                  : null;

            if (volumeValue == null || !volumeValue.isNumber()) {
                LOG.error("cubeVolume: unexpected response: {}", response.body());
                throw new ExprEvalException(
                        "cubeVolume: missing or non-numeric 'volume' in response: " + response.body());
            }

            double result = volumeValue.getAsNumber().value().doubleValue();
            LOG.info("cubeVolume: length={} => volume={}", v.getDouble(), result);
            return NodeValue.makeDouble(result);

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("cubeVolume: HTTP call failed", e);
            throw new ExprEvalException("cubeVolume: HTTP call failed: " + e.getMessage(), e);
        }
    }
}