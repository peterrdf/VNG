package org.vng;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.List;
import java.util.stream.Collectors;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.query.QueryBuildException;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.ExprList;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * SPARQL extension function:
 *   vng:gmCollection(?base64Content1, ..., ?base64ContentN) -> base64 geometry (xsd:string)
 */
public class CollectionFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(CollectionFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);
    private static final String DELIMITER = ";";

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        if (args.size() < 1) {
            throw new QueryBuildException("gmCollection: at least one argument is required");
        }
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        if (args == null || args.isEmpty()) {
            throw new ExprEvalException("gmCollection: at least one argument is required");
        }
        for (NodeValue arg : args) {
            if (!arg.isString()) {
                throw new ExprEvalException("gmCollection: all arguments must be strings");
            }
        }

        try {
            String joined = args.stream()
                .map(NodeValue::getString)
                .collect(Collectors.joining(DELIMITER));

            String body = form("base64Contents", joined);

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateCollection");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmCollection: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmCollection: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmCollection: unexpected response: {}", response.body());
                throw new ExprEvalException("gmCollection: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmCollection: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmCollection failed", e);
            throw new ExprEvalException("gmCollection failed: " + e.getMessage(), e);
        }
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}