using System.Diagnostics;
using RDF;

#if _IFCENGINE
using stepengine = RDF.ifcengine;
#endif

#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

namespace VNGPortal.IFC2RDF
{
    public enum GeometryType
    {
        AP242,
        IFC
    }

    public class Geometry : _geometry
    {
        #region Methods

        public Geometry(GeometryType type)
        {
            Type = type;
        }

        protected void Calculate()
        {
            int_t iModel = GetModel();
            uint iVertexLength = GetVertexLength();

            // Format
            SetFormat(iModel);

            /* Geometry */

            int_t iSdaiModel = ifcengine.sdaiGetInstanceModel(Instance);

            long owlInstance = 0;
            ifcengine.owlBuildInstance(iSdaiModel, Instance, out owlInstance);

            OwlInstance = owlInstance;
        }

        #endregion // Methods

        #region Properties

        public GeometryType Type { get; private set; }
        public List<IInstance> Instances = new();

        #endregion // Properties
    }
}
