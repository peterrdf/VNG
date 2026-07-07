using System.Text;
using System.Runtime.InteropServices;
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

namespace VNGPortal.IFC2RDF.IFC
{
    public class IFCModel : Model
    {
        #region Constants

        public static int_t DEFAULT_CIRCLE_SEGMENTS = 36;

        #endregion // Constants

        #region Fields

        // Entities
        int_t _ifcSpaceEntity;
        int_t _ifcOpeningElementEntity;
        int_t _ifcDistributionElementEntity;
        int_t _ifcElectricalElementEntity;
        int_t _ifcElementAssemblyEntity;
        int_t _ifcElementComponentEntity;
        int_t _ifcEquipmentElementEntity;
        int_t _ifcFeatureElementEntity;
        int_t _ifcFeatureElementSubtractionEntity;
        int_t _ifcFurnishingElementEntity;
        int_t _ifcReinforcingElementEntity;
        int_t _ifcTransportElementEntity;
        int_t _ifcVirtualElementEntity;

        #endregion // Fields

        #region Methods

        public IFCModel()
            : base(ModelType.IFC)
        {
        }

        protected override bool LoadCore(string strFilePath)
        {
            if (!File.Exists(strFilePath))
            {
                return false;
            }

            // Add null terminator to byte arrays for P/Invoke marshaling
            var filePathBytes = Encoding.UTF8.GetBytes(strFilePath + "\0");
            var schemaBytes = Encoding.UTF8.GetBytes("\0");
            Instance = ifcengine.sdaiOpenModelBN(0, filePathBytes, schemaBytes);
            if (Instance == 0)
            {
                return false;
            }

            ifcengine.GetSPFFHeaderItem(Instance, 9, 0, ifcengine.sdaiSTRING, out IntPtr outputValue);

            string? strVersion = Marshal.PtrToStringAnsi(outputValue);
            if (strVersion == null || !strVersion.Contains("IFC"))
            {
                return false;
            }

            ifcengine.setBRepProperties(Instance, 7, 0.9, 0.0, 20000);

            _ifcSpaceEntity = ifcengine.sdaiGetEntity(Instance, "IFCSPACE");
            _ifcOpeningElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCOPENINGELEMENT");
            _ifcDistributionElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCDISTRIBUTIONELEMENT");
            _ifcElectricalElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCELECTRICALELEMENT");
            _ifcElementAssemblyEntity = ifcengine.sdaiGetEntity(Instance, "IFCELEMENTASSEMBLY");
            _ifcElementComponentEntity = ifcengine.sdaiGetEntity(Instance, "IFCELEMENTCOMPONENT");
            _ifcEquipmentElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCEQUIPMENTELEMENT");
            _ifcFeatureElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCFEATUREELEMENT");
            _ifcFeatureElementSubtractionEntity = ifcengine.sdaiGetEntity(Instance, "IFCFEATUREELEMENTSUBTRACTION");
            _ifcFurnishingElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCFURNISHINGELEMENT");
            _ifcReinforcingElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCREINFORCINGELEMENT");
            _ifcTransportElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCTRANSPORTELEMENT");
            _ifcVirtualElementEntity = ifcengine.sdaiGetEntity(Instance, "IFCVIRTUALELEMENT");

            RetrieveObjectsRecursively(ifcengine.sdaiGetEntity(Instance, "IFCOBJECT"), DEFAULT_CIRCLE_SEGMENTS);
            RetrieveObjects("IFCPROJECT", DEFAULT_CIRCLE_SEGMENTS);
            RetrieveObjects("IFCRELSPACEBOUNDARY", DEFAULT_CIRCLE_SEGMENTS);

            FireModelLoaded();

            return true;
        }

        private void RetrieveObjects(string strEntityName, int_t iCircleSegments)
        {
            int_t iInstances = ifcengine.sdaiGetEntityExtentBN(Instance, strEntityName);
            int_t iInstancesCount = ifcengine.sdaiGetMemberCount(iInstances);

            for (int_t i = 0; i < iInstancesCount; ++i)
            {
                int_t iInstance = 0;
                ifcengine.engiGetAggrElement(iInstances, i, ifcengine.sdaiINSTANCE, out iInstance);
                Debug.Assert(iInstance != 0);

                PreLoadInstance(iInstance);

                if (iCircleSegments != DEFAULT_CIRCLE_SEGMENTS)
                {
                    ifcengine.circleSegments(iCircleSegments, 5);
                }

                IFCInstance ifcInstance = new IFCInstance(_iInstanceID++, iInstance, strEntityName);

                if (iCircleSegments != DEFAULT_CIRCLE_SEGMENTS)
                {
                    ifcengine.circleSegments(DEFAULT_CIRCLE_SEGMENTS, 5);
                }

                ifcInstance.Enabled =
                    strEntityName.ToUpper() == "IFCSPACE" ||
                    strEntityName.ToUpper() == "IFCRELSPACEBOUNDARY" ||
                    strEntityName.ToUpper() == "IFCOPENINGELEMENT" ||
                    strEntityName.ToUpper() == "IFCALIGNMENTVERTICAL" ||
                    strEntityName.ToUpper() == "IFCALIGNMENTHORIZONTAL" ||
                    strEntityName.ToUpper() == "IFCALIGNMENTSEGMENT" ||
                    strEntityName.ToUpper() == "IFCALIGNMENTCANT" ? false : true;

                if (!IFCInstances.ContainsKey(ifcInstance.Instance))
                {
                    IFCInstances.Add(ifcInstance.Instance, ifcInstance);
                    ID2IFCInstance[ifcInstance.ID] = ifcInstance;
                }

                if (!Geometries.ContainsKey(ifcInstance.Instance))
                {
                    ifcInstance.Instances.Add(ifcInstance); // Geometry

                    Geometries[ifcInstance.Instance] = ifcInstance;
                    Instances[ifcInstance.ID] = ifcInstance;
                }                
            }
        }

        private void RetrieveObjectsRecursively(int_t iParentEntity, int_t iCircleSegments)
        {
            if (iParentEntity == _ifcDistributionElementEntity ||
                iParentEntity == _ifcElectricalElementEntity ||
                iParentEntity == _ifcElementAssemblyEntity ||
                iParentEntity == _ifcElementComponentEntity ||
                iParentEntity == _ifcEquipmentElementEntity ||
                iParentEntity == _ifcFeatureElementEntity ||
                iParentEntity == _ifcFurnishingElementEntity ||
                iParentEntity == _ifcTransportElementEntity ||
                iParentEntity == _ifcVirtualElementEntity)
            {
                iCircleSegments = 12;
            }

            if (iParentEntity == _ifcReinforcingElementEntity)
            {
                iCircleSegments = 6;
            }

            int_t iInstances = ifcengine.sdaiGetEntityExtent(Instance, iParentEntity);
            int_t iIntancesCount = ifcengine.sdaiGetMemberCount(iInstances);

            if (iIntancesCount != 0)
            {
                ifcengine.engiGetEntityName(iParentEntity, ifcengine.sdaiSTRING, out IntPtr name);

                string? strParentEntityName = Marshal.PtrToStringAnsi(name);
                if (strParentEntityName != null)
                {
                    RetrieveObjects(strParentEntityName, iCircleSegments);
                }
            }

            iIntancesCount = ifcengine.engiGetEntityCount(Instance);
            for (int_t i = 0; i < iIntancesCount; i++)
            {
                int_t iEntity = ifcengine.engiGetEntityElement(Instance, i);
                if (ifcengine.engiGetEntityParent(iEntity) == iParentEntity)
                {
                    RetrieveObjectsRecursively(iEntity, iCircleSegments);
                }
            }
        }

        protected override void Reset()
        {
            base.Reset();

            IFCInstances = new Dictionary<int_t, IFCInstance>();
            ID2IFCInstance = new Dictionary<int_t, IFCInstance>();
        }

        #endregion // Methods

        #region Properties

        // SdaiInstance : IFCInstance
        public Dictionary<int_t, IFCInstance> IFCInstances { get; private set; } = new Dictionary<int_t, IFCInstance>(); //#todo - use Instances?
        // ID : IFCInstance
        public Dictionary<int_t, IFCInstance> ID2IFCInstance { get; private set; } = new Dictionary<int_t, IFCInstance>(); //#todo - use Instances?

        #endregion // Properties
    }
}
