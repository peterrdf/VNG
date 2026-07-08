using IFCGeometry2RDF;
using RDF;

#if _IFCENGINE
using stepengine = RDF.ifcengine;
#endif

#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

namespace IFCGeometry2RDF.IFC
{
    public class IFCInstance
        : Geometry
        , IInstance
    {
        #region Fields

        private int_t _iID;

        #endregion // Fields

        #region Methods

        public IFCInstance(int_t iID, int_t iInstance, string strEntity)
            : base(GeometryType.IFC)
        {
            // Geometry
            Instance = iInstance;
            Entity = strEntity;

            // IInstance
            _iID = iID;

            Calculate();
        }

        protected override void SetFormat(int_t iModel)
        {
            base.SetFormat(iModel);

            ifcengine.setFilter(iModel, ifcengine.flagbit1, ifcengine.flagbit1);
        }

        #endregion // Methods

        #region Properties

        #region IInstance

        public int_t ID => _iID;
        public Geometry Geometry => this;
        public _matrix4x4? Transformation { get; set; } = null;
        public bool Enabled { get; set; } = false;

        #endregion // IInstance

        #endregion // Properties
    }
}
