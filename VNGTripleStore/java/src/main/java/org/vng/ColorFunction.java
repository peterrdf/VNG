package org.vng;

import java.net.URI;
import java.net.URLEncoder;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.util.List;
import java.util.Locale;

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
 *   vng:gmColor(?base64Content, ?r, ?g, ?b [, ?a]) -> base64 geometry (xsd:string)
 */
public class ColorFunction extends FunctionBase {

    private static final Logger LOG = LoggerFactory.getLogger(ColorFunction.class);
    private static final String SERVICE_BASE_URL = "http://vngservice:8080";
    private static final Duration TIMEOUT = Duration.ofMinutes(5);

    private static final HttpClient CLIENT = HttpClient.newBuilder()
        .connectTimeout(TIMEOUT)
        .build();

    @Override
    public void checkBuild(String uri, ExprList args) {
        if (args.size() < 4 || args.size() > 5)
            throw new QueryBuildException("gmColor: expects 4 or 5 arguments, got " + args.size());
    }

    @Override
    public NodeValue exec(List<NodeValue> args) {
        NodeValue a = args.size() > 4 ? args.get(4) : null;
        return exec(args.get(0), args.get(1), args.get(2), args.get(3), a);
    }

    public NodeValue exec(NodeValue base64Content, NodeValue r, NodeValue g, NodeValue b, NodeValue a) {
        if (!base64Content.isString()) {
            throw new ExprEvalException("gmColor: first argument must be a string");
        }
        requireNumber("R", r);
        requireNumber("G", g);
        requireNumber("B", b);
        if (a != null) {
            requireNumber("A", a);
        }

        try {
            // Locale.ROOT guarantees '.' as decimal separator
            StringBuilder body = new StringBuilder()
                .append(form("base64Content", base64Content.getString()))
                .append('&').append(form("R", number(r)))
                .append('&').append(form("G", number(g)))
                .append('&').append(form("B", number(b)));

            if (a != null) {
                body.append('&').append(form("A", number(a)));
            }

            URI uri = URI.create(SERVICE_BASE_URL + "/GeometryModeling?handler=CreateColor");
            HttpRequest request = HttpRequest.newBuilder(uri)
                .timeout(TIMEOUT)
                .header("Content-Type", "application/x-www-form-urlencoded")
                .POST(HttpRequest.BodyPublishers.ofString(body.toString()))
                .build();

            HttpResponse<String> response =
                CLIENT.send(request, HttpResponse.BodyHandlers.ofString());

            if (response.statusCode() != 200) {
                LOG.error("gmColor: HTTP {}: {}", response.statusCode(), response.body());
                throw new ExprEvalException("gmColor: HTTP " + response.statusCode());
            }

            JsonObject json = JSON.parse(response.body()).getAsObject();
            JsonValue geometry = json.hasKey("geometry") ? json.get("geometry") : json.get("Geometry");

            if (geometry == null || !geometry.isString()) {
                LOG.error("gmColor: unexpected response: {}", response.body());
                throw new ExprEvalException("gmColor: missing or non-string 'geometry' in response");
            }

            return NodeValue.makeString(geometry.getAsString().value());
        } catch (ExprEvalException e) {
            throw e;
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new ExprEvalException("gmColor: interrupted", e);
        } catch (Exception e) {
            LOG.error("gmColor failed", e);
            throw new ExprEvalException("gmColor failed: " + e.getMessage(), e);
        }
    }

    private static void requireNumber(String name, NodeValue v) {
        if (!v.isNumber())
            throw new ExprEvalException("gmColor: '" + name + "' must be numeric, got: " + v);
    }

    private static String number(NodeValue v) {
        return String.format(Locale.ROOT, "%s", v.getDouble());
    }

    private static String form(String name, String value) {
        return URLEncoder.encode(name, StandardCharsets.UTF_8) + "="
            + URLEncoder.encode(value, StandardCharsets.UTF_8);
    }
}