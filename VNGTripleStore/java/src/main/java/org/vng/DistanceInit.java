package org.vng;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class DistanceInit implements JenaSubsystemLifecycle {

    private static final String FN_URI =
        "http://vng.nl/geometry-ext.ttl#gmDistance";

    @Override
    public void start() {
        FunctionRegistry.get().put(FN_URI, DistanceFunction.class);
        System.out.println("[DistanceInit] Registered: " + FN_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 101; }
}