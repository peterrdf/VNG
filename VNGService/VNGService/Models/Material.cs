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
            long owlColorComponentInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.95;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Color"));
            RDF.Engine.SetObjectProperty(
               owlColorInstance,
               RDF.Engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.Engine.SetObjectProperty(
              owlColorInstance,
              RDF.Engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);
            //double transparency = 0.75;
            //RDF.Engine.SetDatatypeProperty(
            //    owlColorInstance,
            //    RDF.Engine.GetPropertyByName(owlModel, "transparency"),
            //    ref transparency,
            //    1);

            long owlMaterialInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Material"));
            RDF.Engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.Engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetParcelDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.85;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);
            
            long owlColorInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Color"));
            RDF.Engine.SetObjectProperty(
               owlColorInstance,
               RDF.Engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.Engine.SetObjectProperty(
              owlColorInstance,
              RDF.Engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Material"));
            RDF.Engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.Engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetBuildingDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 1.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            value = 0.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            value = 0.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Color"));
            RDF.Engine.SetObjectProperty(
               owlColorInstance,
               RDF.Engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.Engine.SetObjectProperty(
              owlColorInstance,
              RDF.Engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Material"));
            RDF.Engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.Engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }

        public static long GetWaterDefaultMaterial(long owlModel)
        {
            long owlColorComponentInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "ColorComponent"));

            double value = 0.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "R"),
                ref value,
                1);

            value = 0.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "G"),
                ref value,
                1);

            value = 1.0;
            RDF.Engine.SetDatatypeProperty(
                owlColorComponentInstance,
                RDF.Engine.GetPropertyByName(owlModel, "B"),
                ref value,
                1);

            long owlColorInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Color"));
            RDF.Engine.SetObjectProperty(
               owlColorInstance,
               RDF.Engine.GetPropertyByName(owlModel, "ambient"),
               ref owlColorComponentInstance,
               1);
            RDF.Engine.SetObjectProperty(
              owlColorInstance,
              RDF.Engine.GetPropertyByName(owlModel, "diffuse"),
              ref owlColorComponentInstance,
              1);

            long owlMaterialInstance = RDF.Engine.CreateInstance(RDF.Engine.GetClassByName(owlModel, "Material"));
            RDF.Engine.SetObjectProperty(
                owlMaterialInstance,
                RDF.Engine.GetPropertyByName(owlModel, "color"),
                ref owlColorInstance,
                1);

            return owlMaterialInstance;
        }
    }
}
