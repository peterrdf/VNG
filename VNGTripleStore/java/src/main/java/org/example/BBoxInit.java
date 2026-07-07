package org.example;

import org.apache.jena.sparql.pfunction.PropertyFunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class BBoxInit implements JenaSubsystemLifecycle {

    private static final String PF_URI =
        "http://vng.nl/geometry-ext.ttl#bbox";

    @Override
    public void start() {
        PropertyFunctionRegistry.get().put(PF_URI, BBoxPropFunction.class);
        System.out.println("[BBoxInit] Registered property function: " + PF_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 101; }
}