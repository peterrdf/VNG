package org.example;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.*;
import org.apache.jena.sparql.function.FunctionBase2;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.net.ConnectException;
import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;

public class TopologicalRelationFunction extends FunctionBase2 {

    private static final Logger LOG = LoggerFactory.getLogger(TopologicalRelationFunction.class);
    private static final HttpClient HTTP_CLIENT;

    private static final int    MAX_RETRIES      = 3;
    private static final long   RETRY_DELAY_MS   = 2_000;

    static {
        try {
            HTTP_CLIENT = HttpClient.newBuilder()
                    .connectTimeout(Duration.ofMinutes(5))
                    .build();
            LOG.info("TopologicalRelationFunction: initialized OK");
        } catch (Throwable t) {
            LoggerFactory.getLogger(TopologicalRelationFunction.class)
                    .error("TopologicalRelationFunction: static initializer FAILED — {}", t.toString(), t);
            throw t;
        }
    }

    private static final String SERVICE_BASE_URL = "http://vngservice:8080";

    @Override
    public NodeValue exec(NodeValue v1, NodeValue v2) {
        if (!v1.isString())
            throw new ExprEvalException("topologicalRelation: expected string, got: " + v1);
        if (!v2.isString())
            throw new ExprEvalException("topologicalRelation: expected string, got: " + v2);

        Exception lastException = null;

        for (int attempt = 1; attempt <= MAX_RETRIES; attempt++) {
            try {
                URI uri = URI.create(SERVICE_BASE_URL + "/Geometry?handler=TopologicalRelation2");

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

                LOG.debug("topologicalRelation: response status={} body={}", response.statusCode(), response.body());

                if (response.statusCode() != 200) {
                    LOG.error("topologicalRelation: HTTP error {} from service: {}", response.statusCode(), response.body());
                    throw new ExprEvalException(
                            "topologicalRelation: HTTP error " + response.statusCode()
                            + " from service: " + response.body());
                }

                JsonObject json = JSON.parse(response.body()).getAsObject();

                JsonValue topologicalRelationValue = json.hasKey("topologicalRelation") ? json.get("topologicalRelation")
                                      : json.hasKey("TopologicalRelation") ? json.get("TopologicalRelation")
                                      : null;

                if (topologicalRelationValue == null || !topologicalRelationValue.isString()) {
                    LOG.error("topologicalRelation: unexpected response: {}", response.body());
                    throw new ExprEvalException(
                            "topologicalRelation: missing or non-string 'topologicalRelation' in response: " + response.body());
                }

                return NodeValue.makeString(topologicalRelationValue.getAsString().value());

            } catch (ExprEvalException e) {
                throw e;  // never retry on logic errors
            } catch (ConnectException e) {
                lastException = e;
                LOG.warn("topologicalRelation: attempt {}/{} failed — ConnectException: {}",
                        attempt, MAX_RETRIES, e.getMessage());
            } catch (Exception e) {
                lastException = e;
                LOG.warn("topologicalRelation: attempt {}/{} failed — {}",
                        attempt, MAX_RETRIES, e.getMessage());
            }

            if (attempt < MAX_RETRIES) {
                long delay = RETRY_DELAY_MS * attempt; // 2s, 4s
                LOG.info("topologicalRelation: retrying in {}ms...", delay);
                try { Thread.sleep(delay); } catch (InterruptedException ie) {
                    Thread.currentThread().interrupt();
                    throw new ExprEvalException("topologicalRelation: interrupted during retry", ie);
                }
            }
        }

        LOG.error("topologicalRelation: all {} attempts failed", MAX_RETRIES, lastException);
        throw new ExprEvalException("topologicalRelation: HTTP call failed after "
                + MAX_RETRIES + " attempts: " + lastException.getMessage(), lastException);
    }
}