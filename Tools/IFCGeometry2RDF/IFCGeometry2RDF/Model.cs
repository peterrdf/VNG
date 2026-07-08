using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDF;

#if _IFCENGINE
using stepengine = RDF.ifcengine;
#endif

#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

namespace IFCGeometry2RDF
{
    public enum ModelType
    {
        AP242,
        IFC
    }

    public abstract class Model
    {
        #region Fields

        // Unique ID
        protected int_t _iInstanceID = 1;

        #endregion // Fields

        #region Methods

        public Model(ModelType type)
        {
            Type = type;
        }

        public bool Load(string strFilePath)
        {
            Reset();

            FilePath = strFilePath;

            return LoadCore(strFilePath);
        }

        protected abstract bool LoadCore(string strFilePath);

        protected void Scale()
        {
            // Reset
            Xmin = -1f;
            Xmax = 1f;
            Ymin = -1f;
            Ymax = 1f;
            Zmin = -1f;
            Zmax = 1f;
            OriginalBoundingSphereDiameter = 2f;
            BoundingSphereDiameter = 2f;
            XTranslation = 0f;
            YTranslation = 0f;
            ZTranslation = 0f;

            // Calculate Min/Max
            float fXmin = float.MaxValue;
            float fXmax = -float.MaxValue;
            float fYmin = float.MaxValue;
            float fYmax = -float.MaxValue;
            float fZmin = float.MaxValue;
            float fZmax = -float.MaxValue;
            foreach (var prGeometry in Geometries)
            {
                var geometry = prGeometry.Value;
                if (!geometry.HasGeometry)
                {
                    continue;
                }

                foreach (var instance in geometry.Instances)
                {
                    geometry.CalculateMinMax(
                        instance.Transformation,
                        ref fXmin, ref fXmax,
                        ref fYmin, ref fYmax,
                        ref fZmin, ref fZmax);
                }
            } // foreach (var prGeometry ...

            if ((fXmin == float.MaxValue) ||
                (fXmax == -float.MaxValue) ||
                (fYmin == float.MaxValue) ||
                (fYmax == -float.MaxValue) ||
                (fZmin == float.MaxValue) ||
                (fZmax == -float.MaxValue))
            {
                Console.WriteLine("Invalid model.");
                return;
            }

            // World
            float fOriginalBoundingSphereDiameter = fXmax - fXmin;
            fOriginalBoundingSphereDiameter = float.Max(fOriginalBoundingSphereDiameter, fYmax - fYmin);
            fOriginalBoundingSphereDiameter = float.Max(fOriginalBoundingSphereDiameter, fZmax - fZmin);

            // Scale
            foreach (var prGeometry in Geometries)
            {
                var geometry = prGeometry.Value;
                if (!geometry.HasGeometry)
                {
                    continue;
                }

                geometry.Scale(fOriginalBoundingSphereDiameter / 2f);
            } // foreach (var prGeometry ...

            // Post-processing
            foreach (var prGeometry in Geometries)
            {
                var geometry = prGeometry.Value;
                if (!geometry.HasGeometry)
                {
                    continue;
                }

                //#todo : BuildCohorts
                //geometry.BuildCohorts();
            } // foreach (var prGeometry ...

            // Calculate Min/Max
            fXmin = float.MaxValue;
            fXmax = -float.MaxValue;
            fYmin = float.MaxValue;
            fYmax = -float.MaxValue;
            fZmin = float.MaxValue;
            fZmax = -float.MaxValue;

            foreach (var prGeometry in Geometries)
            {
                var geometry = prGeometry.Value;
                if (!geometry.HasGeometry)
                {
                    continue;
                }

                foreach (var instance in geometry.Instances)
                {
                    geometry.CalculateMinMax(
                        instance.Transformation,
                        ref fXmin, ref fXmax,
                        ref fYmin, ref fYmax,
                        ref fZmin, ref fZmax);
                }
            } // foreach (var prGeometry ...

            if ((fXmin == float.MaxValue) ||
                (fXmax == -float.MaxValue) ||
                (fYmin == float.MaxValue) ||
                (fYmax == -float.MaxValue) ||
                (fZmin == float.MaxValue) ||
                (fZmax == -float.MaxValue))
            {
                Console.WriteLine("Invalid model.");
                return;
            }

            // Min/Max
            Xmin = fXmin;
            Xmax = fXmax;
            Ymin = fYmin;
            Ymax = fYmax;
            Zmin = fZmin;
            Zmax = fZmax;

            // World
            OriginalBoundingSphereDiameter = fOriginalBoundingSphereDiameter;
            BoundingSphereDiameter = fXmax - fXmin;
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fYmax - fYmin);
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fZmax - fZmin);

            // [0.0 -> X/Y/Zmin + X/Y/Zmax]
            XTranslation -= Xmin;
            YTranslation -= Ymin;
            ZTranslation -= Zmin;

            // center
            XTranslation -= (Xmax - Xmin) / 2.0f;
            YTranslation -= (Ymax - Ymin) / 2.0f;
            ZTranslation -= (Zmax - Zmin) / 2.0f;

            // [-1.0 -> 1.0]
            XTranslation /= BoundingSphereDiameter / 2.0f;
            YTranslation /= BoundingSphereDiameter / 2.0f;
            ZTranslation /= BoundingSphereDiameter / 2.0f;
        }

        public void ZoomTo(IInstance instance)
        {
            // Reset
            Xmin = -1f;
            Xmax = 1f;
            Ymin = -1f;
            Ymax = 1f;
            Zmin = -1f;
            Zmax = 1f;
            BoundingSphereDiameter = 2f;
            XTranslation = 0f;
            YTranslation = 0f;
            ZTranslation = 0f;

            // Calculate Min/Max
            float fXmin = float.MaxValue;
            float fXmax = -float.MaxValue;
            float fYmin = float.MaxValue;
            float fYmax = -float.MaxValue;
            float fZmin = float.MaxValue;
            float fZmax = -float.MaxValue;

            instance.Geometry.CalculateMinMax(
                instance.Transformation,
                ref fXmin, ref fXmax,
                ref fYmin, ref fYmax,
                ref fZmin, ref fZmax);

            if ((fXmin == float.MaxValue) ||
                (fXmax == -float.MaxValue) ||
                (fYmin == float.MaxValue) ||
                (fYmax == -float.MaxValue) ||
                (fZmin == float.MaxValue) ||
                (fZmax == -float.MaxValue))
            {
                fXmin = -1f;
                fXmax = 1f;
                fYmin = -1f;
                fYmax = 1f;
                fZmin = -1f;
                fZmax = 1f;
            }

            // Min/Max
            Xmin = fXmin;
            Xmax = fXmax;
            Ymin = fYmin;
            Ymax = fYmax;
            Zmin = fZmin;
            Zmax = fZmax;

            // World
            BoundingSphereDiameter = fXmax - fXmin;
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fYmax - fYmin);
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fZmax - fZmin);

            // [0.0 -> X/Y/Zmin + X/Y/Zmax]
            XTranslation -= Xmin;
            YTranslation -= Ymin;
            ZTranslation -= Zmin;

            // center
            XTranslation -= (Xmax - Xmin) / 2.0f;
            YTranslation -= (Ymax - Ymin) / 2.0f;
            ZTranslation -= (Zmax - Zmin) / 2.0f;

            // [-1.0 -> 1.0]
            XTranslation /= BoundingSphereDiameter / 2.0f;
            YTranslation /= BoundingSphereDiameter / 2.0f;
            ZTranslation /= BoundingSphereDiameter / 2.0f;
        }

        public void ZoomExtent()
        {
            // Reset
            Xmin = -1f;
            Xmax = 1f;
            Ymin = -1f;
            Ymax = 1f;
            Zmin = -1f;
            Zmax = 1f;
            BoundingSphereDiameter = 2f;
            XTranslation = 0f;
            YTranslation = 0f;
            ZTranslation = 0f;

            // Calculate Min/Max
            float fXmin = float.MaxValue;
            float fXmax = -float.MaxValue;
            float fYmin = float.MaxValue;
            float fYmax = -float.MaxValue;
            float fZmin = float.MaxValue;
            float fZmax = -float.MaxValue;
            foreach (var prGeometry in Geometries)
            {
                prGeometry.Value.CalculateMinMax(
                    ref fXmin, ref fXmax,
                    ref fYmin, ref fYmax,
                    ref fZmin, ref fZmax);
            }

            if ((fXmin == float.MaxValue) ||
                (fXmax == -float.MaxValue) ||
                (fYmin == float.MaxValue) ||
                (fYmax == -float.MaxValue) ||
                (fZmin == float.MaxValue) ||
                (fZmax == -float.MaxValue))
            {
                fXmin = -1f;
                fXmax = 1f;
                fYmin = -1f;
                fYmax = 1f;
                fZmin = -1f;
                fZmax = 1f;
            }

            // Min/Max
            Xmin = fXmin;
            Xmax = fXmax;
            Ymin = fYmin;
            Ymax = fYmax;
            Zmin = fZmin;
            Zmax = fZmax;

            // World
            BoundingSphereDiameter = fXmax - fXmin;
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fYmax - fYmin);
            BoundingSphereDiameter = float.Max(BoundingSphereDiameter, fZmax - fZmin);

            // [0.0 -> X/Y/Zmin + X/Y/Zmax]
            XTranslation -= Xmin;
            YTranslation -= Ymin;
            ZTranslation -= Zmin;

            // center
            XTranslation -= (Xmax - Xmin) / 2.0f;
            YTranslation -= (Ymax - Ymin) / 2.0f;
            ZTranslation -= (Zmax - Zmin) / 2.0f;

            // [-1.0 -> 1.0]
            XTranslation /= BoundingSphereDiameter / 2.0f;
            YTranslation /= BoundingSphereDiameter / 2.0f;
            ZTranslation /= BoundingSphereDiameter / 2.0f;
        }

        protected void PreLoadInstance(int_t iInstance)
        {
            if (UpdteVertexBuffers)
            {
                long iClassInstance = RDF.engine.GetInstanceGeometryClass(iInstance);
                if (iClassInstance == 0)
                {
                    return;
                }

                double[] transformationMatrix = null;
                double[] startVector = new double[3];
                double[] endVector = new double[3];
                if (RDF.engine.GetBoundingBox(iInstance, transformationMatrix, startVector, endVector) == 0)
                {
                    return;
                }

                RDF.engine.SetVertexBufferOffset(
                    Instance,
                    -(startVector[0] + endVector[0]) / 2.0,
                    -(startVector[1] + endVector[1]) / 2.0,
                    -(startVector[2] + endVector[2]) / 2.0);

                RDF.engine.ClearedExternalBuffers(Instance);

                UpdteVertexBuffers = false;
            }
        }

        public uint GetVertexLength()
        {
            return (uint)(RDF.engine.SetFormat(Instance, 0, 0) / sizeof(float));
        }

        protected virtual void Reset()
        {
            _iInstanceID = 1;
            FilePath = null;
            if (Instance != 0)
            {
                ifcengine.sdaiCloseModel(Instance);
                Instance = 0;
            }
            UpdteVertexBuffers = true;

            Xmin = -1f;
            Xmax = 1f;
            Ymin = -1f;
            Ymax = 1f;
            Zmin = -1f;
            Zmax = 1f;
            OriginalBoundingSphereDiameter = 2f;
            BoundingSphereDiameter = 2f;
            XTranslation = 0f;
            YTranslation = 0f;
            ZTranslation = 0f;

            Geometries = new Dictionary<int_t, Geometry>();
            Instances = new Dictionary<int_t, IInstance>();
        }

        protected void FireModelLoaded()
        {
            if (ModelLoaded != null)
            {
                ModelLoaded(this, new EventArgs());
            }
        }

        public static int_t GetObjectProperty(long iInstance, string strProperty)
        {
            long iModel = RDF.engine.GetModel(iInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetObjectProperty(
                iInstance,
                RDF.engine.GetPropertyByName(iModel, strProperty),
                out values,
                out iCard);

            if (iCard == 1)
            {
                unsafe
                {
                    return ((int_t*)values.ToPointer())[0];
                }
            }

            return 0;
        }

        public static double GetDoubleProperty(int_t iInstance, string strProperty)
        {
            long iModel = RDF.engine.GetModel(iInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetDatatypeProperty(
                iInstance,
                RDF.engine.GetPropertyByName(iModel, strProperty),
                out values,
                out iCard);

            if (iCard == 1)
            {
                unsafe
                {
                    return ((double*)values.ToPointer())[0];
                }
            }

            return 0;
        }

        #endregion // Methods

        #region Events

        public event EventHandler ModelLoaded;

        #endregion // Events

        #region Properties

        public string FilePath { get; protected set; }
        public int_t Instance { get; protected set; }
        public ModelType Type { get; protected set; }
        protected bool UpdteVertexBuffers { get; set; } = true;
        public float Xmin { get; protected set; } = -1f;
        public float Xmax { get; protected set; } = 1f;
        public float Ymin { get; protected set; } = -1f;
        public float Ymax { get; protected set; } = 1f;
        public float Zmin { get; protected set; } = -1f;
        public float Zmax { get; protected set; } = 1f;
        public float OriginalBoundingSphereDiameter { get; protected set; } = 2f;
        public float BoundingSphereDiameter { get; protected set; } = 2f;
        public float XTranslation { get; protected set; } = 0f;
        public float YTranslation { get; protected set; } = 0f;
        public float ZTranslation { get; protected set; } = 0f;

        // SdaiInstance : Geometry
        public Dictionary<int_t, Geometry> Geometries { get; private set; } = new Dictionary<int_t, Geometry>();
        // ID : 
        public Dictionary<int_t, IInstance> Instances { get; private set; } = new Dictionary<int_t, IInstance>();

        #endregion // Properties
    }
}
