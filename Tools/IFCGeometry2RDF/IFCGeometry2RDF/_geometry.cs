using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
    public class _vector3
    {
        public float X;
        public float Y;
        public float Z;

        public _vector3()
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
        }

        public _vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static void Transform(_vector3 v, _matrix4x4 m, _vector3 result)
        {
            _vector3 tmp = new _vector3();
            tmp.X = (float)(v.X * m._11 + v.Y * m._21 + v.Z * m._31 + m._41);
            tmp.Y = (float)(v.X * m._12 + v.Y * m._22 + v.Z * m._32 + m._42);
            tmp.Z = (float)(v.X * m._13 + v.Y * m._23 + v.Z * m._33 + m._43);

            result.X = tmp.X;
            result.Y = tmp.Y;
            result.Z = tmp.Z;
        }
    }

    public class _matrix4x3
    {
        #region Methods

        public _matrix4x3()
        {
        }

        public static void Multiply(_matrix4x3 m1, _matrix4x3 m2, _matrix4x3 result)
        {
            var temp = new _matrix4x3();
            temp._11 = m1._11 * m2._11 + m1._12 * m2._21 + m1._13 * m2._31;
            temp._12 = m1._11 * m2._12 + m1._12 * m2._22 + m1._13 * m2._32;
            temp._13 = m1._11 * m2._13 + m1._12 * m2._23 + m1._13 * m2._33;

            temp._21 = m1._21 * m2._11 + m1._22 * m2._21 + m1._23 * m2._31;
            temp._22 = m1._21 * m2._12 + m1._22 * m2._22 + m1._23 * m2._32;
            temp._23 = m1._21 * m2._13 + m1._22 * m2._23 + m1._23 * m2._33;

            temp._31 = m1._31 * m2._11 + m1._32 * m2._21 + m1._33 * m2._31;
            temp._32 = m1._31 * m2._12 + m1._32 * m2._22 + m1._33 * m2._32;
            temp._33 = m1._31 * m2._13 + m1._32 * m2._23 + m1._33 * m2._33;

            temp._41 = m1._41 * m2._11 + m1._42 * m2._21 + m1._43 * m2._31 + m2._41;
            temp._42 = m1._41 * m2._12 + m1._42 * m2._22 + m1._43 * m2._32 + m2._42;
            temp._43 = m1._41 * m2._13 + m1._42 * m2._23 + m1._43 * m2._33 + m2._43;

            result._11 = temp._11;
            result._12 = temp._12;
            result._13 = temp._13;

            result._21 = temp._21;
            result._22 = temp._22;
            result._23 = temp._23;

            result._31 = temp._31;
            result._32 = temp._32;
            result._33 = temp._33;

            result._41 = temp._41;
            result._42 = temp._42;
            result._43 = temp._43;
        }

        public static _matrix4x3 Copy(_matrix4x3 m)
        {
            _matrix4x3 result = new _matrix4x3();
            result._11 = m._11;
            result._12 = m._12;
            result._13 = m._13;

            result._21 = m._21;
            result._22 = m._22;
            result._23 = m._23;

            result._31 = m._31;
            result._32 = m._32;
            result._33 = m._33;

            result._41 = m._41;
            result._42 = m._42;
            result._43 = m._43;

            return result;
        }

        #endregion // Methods

        #region Properties

        public double _11 { get; set; } = 1.0;
        public double _12 { get; set; } = 0.0;
        public double _13 { get; set; } = 0.0;

        public double _21 { get; set; } = 0.0;
        public double _22 { get; set; } = 1.0;
        public double _23 { get; set; } = 0.0;

        public double _31 { get; set; } = 0.0;
        public double _32 { get; set; } = 0.0;
        public double _33 { get; set; } = 1.0;

        public double _41 { get; set; } = 0.0;
        public double _42 { get; set; } = 0.0;
        public double _43 { get; set; } = 0.0;

        #endregion // Properties
    }

    public class _matrix4x4
    {
        #region Methods

        public _matrix4x4()
        {
        }

        public static _matrix4x4 To4x4(_matrix4x3 m)
        {
            var result = new _matrix4x4();
            result._11 = m._11;
            result._12 = m._12;
            result._13 = m._13;

            result._21 = m._21;
            result._22 = m._22;
            result._23 = m._23;

            result._31 = m._31;
            result._32 = m._32;
            result._33 = m._33;

            result._41 = m._41;
            result._42 = m._42;
            result._43 = m._43;

            return result;
        }

        #endregion // Methods

        #region Properties

        public double _11 { get; set; } = 1.0;
        public double _12 { get; set; } = 0.0;
        public double _13 { get; set; } = 0.0;
        public double _14 { get; set; } = 0.0;

        public double _21 { get; set; } = 0.0;
        public double _22 { get; set; } = 1.0;
        public double _23 { get; set; } = 0.0;
        public double _24 { get; set; } = 0.0;

        public double _31 { get; set; } = 0.0;
        public double _32 { get; set; } = 0.0;
        public double _33 { get; set; } = 1.0;
        public double _34 { get; set; } = 0.0;

        public double _41 { get; set; } = 0.0;
        public double _42 { get; set; } = 0.0;
        public double _43 { get; set; } = 0.0;
        public double _44 { get; set; } = 1.0;

        #endregion // Properties
    }

    public static class _oglUtils
    {
        public static uint GetVerticesCountLimit(uint iVertexLengthBytes)
        {
            return uint.MaxValue / iVertexLengthBytes;
        }

        public static uint GetIndicesCountLimit()
        {
            return 64800;
        }
    }

    public class _primitives
    {
        public _primitives(long iStartIndex, long iIndicesCount)
        {
            StartIndex = iStartIndex;
            IndicesCount = iIndicesCount;
        }

        public long StartIndex { get; set; }
        public long IndicesCount { get; private set; }
    };

    public class _cohort
    {
        public _cohort()
        { }

        public List<uint> Indices { get; set; } = new List<uint>();

        public uint IBO { get; set; }
        public uint IBOOffset { get; set; }
    }

    public class _face : _primitives
    {
        public _face(long iIndex, long iStartIndex, long iIndicesCount)
            : base(iStartIndex, iIndicesCount)
        {
            Index = iIndex;
        }
        public long Index { get; private set; }
    };

    public class _cohortWithMaterial : _cohort
    {
        public _cohortWithMaterial(_material material)
            : base()
        {
            this.Material = material;
        }

        public List<_face> Faces { get; set; } = new List<_face>();
        public _material Material { get; private set; }
    };

    public class _geometry
    {
        #region Fields

        // Cache
        protected Dictionary<_material, List<_face>> _dicMaterial2ConcFaces = new Dictionary<_material, List<_face>>();
        protected Dictionary<_material, List<_face>> _dicMaterial2ConcFaceLines = new Dictionary<_material, List<_face>>();
        protected Dictionary<_material, List<_face>> _dicMaterial2ConcFacePoints = new Dictionary<_material, List<_face>>();

        #endregion // Fields

        #region Methods

        public _geometry() 
        { 
        }

        public void CalculateMinMax(
            ref float fXmin, ref float fXmax,
            ref float fYmin, ref float fYmax,
            ref float fZmin, ref float fZmax)
        {
            if (!HasGeometry)
            {
                return;
            }

            if (Vertices == null || Indices == null)
            {
                return;
            }

            uint VERTEX_LENGTH = GetVertexLength();

            if (Triangles.Count > 0)
            {
                foreach (var primitive in Triangles)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        fXmin = float.Min(fXmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmin);
                        fXmax = float.Max(fXmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmax);
                        fYmin = float.Min(fYmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmin);
                        fYmax = float.Max(fYmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmax);
                        fZmin = float.Min(fZmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmin);
                        fZmax = float.Max(fZmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmax);
                    }
                }
            }

            if (ConcFacePolygons.Count > 0)
            {
                foreach (var primitive in ConcFacePolygons)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        fXmin = float.Min(fXmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmin);
                        fXmax = float.Max(fXmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmax);
                        fYmin = float.Min(fYmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmin);
                        fYmax = float.Max(fYmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmax);
                        fZmin = float.Min(fZmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmin);
                        fZmax = float.Max(fZmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmax);
                    }
                }
            }

            if (Lines.Count > 0)
            {
                foreach (var primitive in Lines)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        fXmin = float.Min(fXmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmin);
                        fXmax = float.Max(fXmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmax);
                        fYmin = float.Min(fYmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmin);
                        fYmax = float.Max(fYmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmax);
                        fZmin = float.Min(fZmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmin);
                        fZmax = float.Max(fZmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmax);
                    }
                }
            }

            if (Points.Count > 0)
            {
                foreach (var primitive in Points)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        fXmin = float.Min(fXmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmin);
                        fXmax = float.Max(fXmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? fXmax);
                        fYmin = float.Min(fYmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmin);
                        fYmax = float.Max(fYmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? fYmax);
                        fZmin = float.Min(fZmin, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmin);
                        fZmax = float.Max(fZmax, Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? fZmax);
                    }
                }
            }
        }

        public void CalculateMinMax(
            _matrix4x4? transformation,
            ref float fXmin, ref float fXmax,
            ref float fYmin, ref float fYmax,
            ref float fZmin, ref float fZmax)
        {
            if (transformation == null)
            {
                CalculateMinMax(
                    ref fXmin, ref fXmax,
                    ref fYmin, ref fYmax,
                    ref fZmin, ref fZmax);

                return; 
            }

            if (!HasGeometry)
            {
                return;
            }

            if (Vertices == null || Indices == null)
            {
                return;
            }

            uint VERTEX_LENGTH = GetVertexLength();

            if (Triangles.Count > 0)
            {
                foreach (var primitive in Triangles)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        _vector3 point = new _vector3(
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? 0);
                        _vector3.Transform(point, transformation, point);

                        fXmin = float.Min(fXmin, point.X);
                        fXmax = float.Max(fXmax, point.X);
                        fYmin = float.Min(fYmin, point.Y);
                        fYmax = float.Max(fYmax, point.Y);
                        fZmin = float.Min(fZmin, point.Z);
                        fZmax = float.Max(fZmax, point.Z);
                    }
                }
            }

            if (ConcFacePolygons.Count > 0)
            {
                foreach (var primitive in ConcFacePolygons)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        _vector3 point = new _vector3(
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? 0);
                        _vector3.Transform(point, transformation, point);

                        fXmin = float.Min(fXmin, point.X);
                        fXmax = float.Max(fXmax, point.X);
                        fYmin = float.Min(fYmin, point.Y);
                        fYmax = float.Max(fYmax, point.Y);
                        fZmin = float.Min(fZmin, point.Z);
                        fZmax = float.Max(fZmax, point.Z);
                    }
                }
            }

            if (Lines.Count > 0)
            {
                foreach (var primitive in Lines)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        _vector3 point = new _vector3(
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? 0);
                        _vector3.Transform(point, transformation, point);

                        fXmin = float.Min(fXmin, point.X);
                        fXmax = float.Max(fXmax, point.X);
                        fYmin = float.Min(fYmin, point.Y);
                        fYmax = float.Max(fYmax, point.Y);
                        fZmin = float.Min(fZmin, point.Z);
                        fZmax = float.Max(fZmax, point.Z);
                    }
                }
            }

            if (Points.Count > 0)
            {
                foreach (var primitive in Points)
                {
                    for (var iIndex = primitive.StartIndex;
                        iIndex < primitive.StartIndex + primitive.IndicesCount;
                        iIndex++)
                    {
                        _vector3 point = new _vector3(
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 0] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 1] ?? 0,
                            Vertices?[(Indices[iIndex] * VERTEX_LENGTH) + 2] ?? 0);
                        _vector3.Transform(point, transformation, point);

                        fXmin = float.Min(fXmin, point.X);
                        fXmax = float.Max(fXmax, point.X);
                        fYmin = float.Min(fYmin, point.Y);
                        fYmax = float.Max(fYmax, point.Y);
                        fZmin = float.Min(fZmin, point.Z);
                        fZmax = float.Max(fZmax, point.Z);
                    }
                }
            }
        }

        public virtual void Scale(float fScaleFactor)
        {
            if (!HasGeometry)
            {
                return;
            }

            if (Vertices != null)
            {
                uint VERTEX_LENGTH = GetVertexLength();

                for (var iVertex = 0; iVertex < Vertices.Length / VERTEX_LENGTH; iVertex++)
                {
                    Vertices[(iVertex * VERTEX_LENGTH) + 0] /= fScaleFactor;
                    Vertices[(iVertex * VERTEX_LENGTH) + 1] /= fScaleFactor;
                    Vertices[(iVertex * VERTEX_LENGTH) + 2] /= fScaleFactor;
                }
            }
        }

        protected virtual void SetFormat(int_t iModel)
        {
            int_t setting = 0, mask = 0;
            mask += ifcengine.flagbit2;       //    PRECISION (32/64 bit)
            mask += ifcengine.flagbit3;       //	INDEX ARRAY (32/64 bit)
            mask += ifcengine.flagbit5;       //    NORMALS
            mask += ifcengine.flagbit8;       //    TRIANGLES
            mask += ifcengine.flagbit9;       //    LINES
            mask += ifcengine.flagbit10;      //    POINTS
            mask += ifcengine.flagbit13;      //    CONCEPTUAL FACE POLYGONS
            mask += ifcengine.flagbit15;      //    ADVANCED NORMALS

            setting += 0;                         //    SINGLE PRECISION (float)
            setting += 0;                         //    32 BIT INDEX ARRAY (Int32)
            setting += ifcengine.flagbit5;    //    NORMALS ON
            setting += ifcengine.flagbit8;    //    TRIANGLES ON
            setting += ifcengine.flagbit9;    //    LINES ON
            setting += ifcengine.flagbit10;   //    POINTS ON
            setting += ifcengine.flagbit13;   //    CONCEPTUAL FACE POLYGONS ON
            setting += ifcengine.flagbit15;   //    ADVANCED NORMALS

            RDF.engine.SetFormat(iModel, (ulong)setting, (ulong)mask);
            RDF.engine.SetBehavior(iModel, 2048 + 4096, 2048 + 4096);
            ifcengine.setSegmentation(iModel, 16, 0.0);
        }

        protected int_t GetModel()
        {
            return ifcengine.sdaiGetInstanceModel(Instance);
        }

        protected uint GetVertexLength()
        {
            return (uint)(RDF.engine.SetFormat(GetModel(), 0, 0) / sizeof(float));
        }

        #endregion // Methods

        #region Properties

        // Metadata
        public int_t Instance { get; protected set; }
        public long OwlInstance { get; protected set; }
        public string? Entity { get; protected set; }
        public string Name { get; protected set; } = "NA";
        public string UniqueName { get; protected set; } = string.Empty;
        
        // Geometry
        public float[]? Vertices { get; protected set; }
        public int[]? Indices { get; protected set; }
        public long ConceptualFacesCount { get; protected set; }
        public bool HasGeometry => (Vertices != null && Vertices.Length > 0) && (Indices != null && Indices.Length > 0);

        // Primitives (Cache)
        protected List<_primitives> Triangles { get; set; } = new List<_primitives>();
        protected List<_primitives> ConcFacePolygons { get; set; } = new List<_primitives>();
        protected List<_primitives> Lines { get; set; } = new List<_primitives>();
        protected List<_primitives> Points { get; set; } = new List<_primitives>();

        #endregion // Properties
    }
}
