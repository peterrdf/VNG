using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNGService.Models
{
    internal class Material
    {
        public Material(long iMaterialInstance)
        {
        }

        public static long GetPlanDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.95;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Color"));
            RDF.engine.SetObjectProperty(
               owlColorInstance,
               RDF.engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.engine.SetObjectProperty(
              owlColorInstance,
              RDF.engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);
            //double transparency = 0.75;
            //RDF.engine.SetDatatypeProperty(
            //    owlColorInstance,
            //    RDF.engine.GetPropertyByName(owlModel, "transparency"),
            //    ref transparency,
            //    1);

            long owlMaterialInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Material"));
            RDF.engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetParcelDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.85;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);
            
            long owlColorInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Color"));
            RDF.engine.SetObjectProperty(
               owlColorInstance,
               RDF.engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.engine.SetObjectProperty(
              owlColorInstance,
              RDF.engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Material"));
            RDF.engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetBuildingDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 1.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            value = 0.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            value = 0.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Color"));
            RDF.engine.SetObjectProperty(
               owlColorInstance,
               RDF.engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.engine.SetObjectProperty(
              owlColorInstance,
              RDF.engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Material"));
            RDF.engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetWaterDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            value = 0.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            value = 1.0;
            RDF.engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Color"));
            RDF.engine.SetObjectProperty(
               owlColorInstance,
               RDF.engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.engine.SetObjectProperty(
              owlColorInstance,
              RDF.engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.engine.CreateInstance(RDF.engine.GetClassByName(owlModel, "Material"));
            RDF.engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }
    }
}
