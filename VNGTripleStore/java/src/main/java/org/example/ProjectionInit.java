package org.example;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class ProjectionInit implements JenaSubsystemLifecycle {

    private static final String FN_URI =
        "http://vng.nl/geometry-ext.ttl#projection";

    @Override
    public void start() {
        FunctionRegistry.get().put(FN_URI, ProjectionFunction.class);
        System.out.println("[ProjectionInit] Registered: " + FN_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 101; }
}