package org.vng;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class Projection2DInit implements JenaSubsystemLifecycle {

    private static final String FN_URI = "http://vng.nl/geometry-ext.ttl#gmProjection";

    @Override
    public void start() {
        FunctionRegistry.get().put(FN_URI, Projection2DFunction.class);
        System.out.println("[Projection2DInit] Registered: " + FN_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 101; }
}