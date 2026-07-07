package org.example;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sparql.pfunction.PropertyFunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class GetSpatialPlansDestinationAreasInit implements JenaSubsystemLifecycle {

    private static final String PF_URI = "http://vng.nl/geometry-ext.ttl#eachSpatialPlansDestinationArea";

    @Override
    public void start() {
        PropertyFunctionRegistry.get().put(PF_URI, GetSpatialPlansDestinationAreasPropFunction.class);
        System.out.println("[GetSpatialPlansDestinationAreasInit] Registered property function: " + PF_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 102; }
}