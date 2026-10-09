package org.vng;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.List;

import org.apache.jena.atlas.json.JSON;
import org.apache.jena.atlas.json.JsonObject;
import org.apache.jena.atlas.json.JsonValue;
import org.apache.jena.sparql.expr.ExprEvalException;
import org.apache.jena.sparql.expr.ExprList;
import org.apache.jena.sparql.expr.NodeValue;
import org.apache.jena.sparql.function.FunctionBase;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * SPARQL extension function:
 *   vng:gmBox(?length, ?width, ?height)
 *   vng:gmBox(?length, ?width, ?height, ?r, ?g, ?b, ?t)
 */
public class BoxFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(BoxFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        if (args.size() != 3 && args.size() != 7) {
            throw new ExprEvalException("gmBox: expected 3 (l,w,h) or 7 (l,w,h,r,g,b,t) arguments");
        }
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        int n = args.size();
        if (n != 3 && n != 7) {
            throw new ExprEvalException("gmBox: expected 3 or 7 arguments");
        }
        for (NodeValue v : args) {
            if (!v.isNumber()) {
                throw new ExprEvalException("gmBox: all arguments must be numeric");
            }
        }

        try {
            StringBuilder query = new StringBuilder(param("handler", "CreateBox"))
                .append("&").append(param("length", Double.toString(args.get(0).getDouble())))
                .append("&").append(param("width", Double.toString(args.get(1).getDouble())))
                .append("&").append(param("height", Double.toString(args.get(2).getDouble())));

            if (n == 7) {
                query.append("&").append(param("r", Double.toString(args.get(3).getDouble())))
                    .append("&").append(param("g", Double.toString(args.get(4).getDouble())))
                    .append("&").append(param("b", Double.toString(args.get(5).getDouble())))
                    .append("&").append(param("t", Double.toString(args.get(6).getDouble())));
            }

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?" + query);
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .GET()
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmBox: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmBox: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmBox: unexpected response: {}", response.body());
                throw new ExprEvalException("gmBox: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmBox: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmBox failed", e);
            throw new ExprEvalException("gmBox failed: " + e.getMessage(), e);
        }
    }

    private static String param(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}