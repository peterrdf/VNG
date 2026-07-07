package org.example;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.sparql.expr.*;
import org.apache.jena.sparql.function.FunctionBase2;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;

public class GetBuildingsFunction extends FunctionBase2 {

    private static final Logger LOG = LoggerFactory.getLogger(GetBuildingsFunction.class);
    private static final HttpClient HTTP_CLIENT;

    static {
        try {
            HTTP_CLIENT = HttpClient.newBuilder()
                    .connectTimeout(Duration.ofMinutes(5))
                    .build();
            LOG.info("GetBuildingsFunction: initialized OK");
        } catch (Throwable t) {
            LoggerFactory.getLogger(GetBuildingsFunction.class)
                    .error("GetBuildingsFunction: static initializer FAILED — {}", t.toString(), t);
            throw t;
        }
    }

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue eastings, NodeValue northings) {
        if (!eastings.isNumber())
            throw new ExprEvalException("getBuildings: expected number for eastings, got: " + eastings);

        if (!northings.isNumber())
            throw new ExprEvalException("getBuildings: expected number for northings, got: " + northings);

        LOG.debug("getBuildings: calling service with eastings={} northings={}",
                eastings.getDouble(), northings.getDouble());

        try {
            URI uri = URI.create(SERVICE_BASE_URL + "/LandRegistry?handler=GetBuildings&eastings="
                    + eastings.getDouble() + "&northings=" + northings.getDouble());
            LOG.debug("getBuildings: GET {}", uri);

            HttpRequest request = HttpRequest.newBuilder()
                    .uri(uri)
                    .timeout(Duration.ofMinutes(5))
                    .GET()
                    .build();

            HttpResponse<String> response = HTTP_CLIENT.send(
                    request, HttpResponse.BodyHandlers.ofString());

            LOG.debug("getBuildings: response status={} body={}", response.statusCode(), response.body());

            if (response.statusCode() != 200) {
                LOG.error("getBuildings: HTTP error {} from service: {}", response.statusCode(), response.body());
                throw new ExprEvalException(
                        "getBuildings: HTTP error " + response.statusCode()
                        + " from service: " + response.body());
            }

            // Validate and count returned buildings
            org.apache.jena.atlas.json.JsonArray buildings =
                    JSON.parseAny(response.body()).getAsArray();
            LOG.info("getBuildings: eastings={} northings={} => {} building(s) returned",
                    eastings.getDouble(), northings.getDouble(), buildings.size());

            // Return the raw JSON as a plain string literal
            return NodeValue.makeString(response.body());

        } catch (ExprEvalException e) {
            throw e;
        } catch (Exception e) {
            LOG.error("getBuildings: HTTP call failed", e);
            throw new ExprEvalException("getBuildings: HTTP call failed: " + e.getMessage(), e);
        }
    }
}