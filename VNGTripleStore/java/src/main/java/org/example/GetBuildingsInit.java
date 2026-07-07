package org.example;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sparql.pfunction.PropertyFunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class GetBuildingsInit implements JenaSubsystemLifecycle {

    private static final String FN_URI = "http://vng.nl/geometry-ext.ttl#getBuildings";
    private static final String PF_URI = "http://vng.nl/geometry-ext.ttl#eachBuilding";

    @Override
    public void start() {
        FunctionRegistry.get().put(FN_URI, GetBuildingsFunction.class);
        System.out.println("[GetBuildingsInit] Registered function:          " + FN_URI);

        PropertyFunctionRegistry.get().put(PF_URI, GetBuildingsPropFunction.class);
        System.out.println("[GetBuildingsInit] Registered property function: " + PF_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 102; }
}