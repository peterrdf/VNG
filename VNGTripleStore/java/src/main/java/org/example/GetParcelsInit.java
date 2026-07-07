package org.example;

import org.apache.jena.sparql.function.FunctionRegistry;
import org.apache.jena.sparql.pfunction.PropertyFunctionRegistry;
import org.apache.jena.sys.JenaSubsystemLifecycle;

public class GetParcelsInit implements JenaSubsystemLifecycle {

    //private static final String FN_URI = "http://vng.nl/geometry-ext.ttl#getParcels";
    private static final String PF_URI = "http://vng.nl/geometry-ext.ttl#eachParcel";

    @Override
    public void start() {
        //FunctionRegistry.get().put(FN_URI, GetParcelsFunction.class);
        //System.out.println("[GetParcelsInit] Registered function:          " + FN_URI);

        PropertyFunctionRegistry.get().put(PF_URI, GetParcelsPropFunction.class);
        System.out.println("[GetParcelsInit] Registered property function: " + PF_URI);
    }

    @Override public void stop() {}
    @Override public int level() { return 102; }
}