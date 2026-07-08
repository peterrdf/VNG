using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

namespace IFCGeometry2RDF
{
    public class _color
    {
        #region Fields

        private static _color _facesDefaultColor;
        private static _color _pointedInstanceColor;
        private static _color _selectedInstanceColor;
        private static _color _linesDefaultColor;

        #endregion // Fields

        public _color()
        { }

        public _color(float fR, float fG, float fB, float fW)
        {
            R = fR;
            G = fG;
            B = fB;
            W = fW;
        }

        public static _color Create(long iColorComponentInstance)
        {
            double dR = GetColorComponentInstanceProperty(iColorComponentInstance, "R");
            double dG = GetColorComponentInstanceProperty(iColorComponentInstance, "G");
            double dB = GetColorComponentInstanceProperty(iColorComponentInstance, "B");
            double dW = GetColorComponentInstanceProperty(iColorComponentInstance, "W");

            return new _color((float)dR, (float)dG, (float)dB, (float)dW);
        }

        public static _color GetFacesDefaultColor()
        {
            if (_facesDefaultColor == null)
            {
                _facesDefaultColor = new _color(0.3f, 0.7f, 0.3f, 1.0f);
            }

            return _facesDefaultColor;
        }

        public static _color GetPointedInstanceColor()
        {
            if (_pointedInstanceColor == null)
            {
                _pointedInstanceColor = new _color(.33f, .33f, .33f, 0.66f);
            }

            return _pointedInstanceColor;
        }

        public static _color GetSelectedInstanceColor()
        {
            if (_selectedInstanceColor == null)
            {
                _selectedInstanceColor = new _color(1f, 0f, 0f, 1f);
            }

            return _selectedInstanceColor;
        }

        public static _color GetLinesDefaultColor()
        {
            if (_linesDefaultColor == null)
            {
                _linesDefaultColor = new _color(0f, 0f, 0f, 1.0f);
            }

            return _linesDefaultColor;
        }

        public static long GetColorInstance(long iMaterialInstance)
        {
            long iModel = RDF.engine.GetModel(iMaterialInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetObjectProperty(
                iMaterialInstance,
                RDF.engine.GetPropertyByName(iModel, "color"),
                out values,
                out iCard);

            if (iCard == 1)
            {
                unsafe
                {
                    return ((long*)values.ToPointer())[0];
                }
            }

            return 0;
        }

        public static long GetColorComponentInstance(long iColorInstance, string strColorComponent)
        {
            long iModel = RDF.engine.GetModel(iColorInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetObjectProperty(
                iColorInstance,
                RDF.engine.GetPropertyByName(iModel, strColorComponent),
                out values,
                out iCard);

            if (iCard == 1)
            {
                unsafe
                {
                    return ((long*)values.ToPointer())[0];
                }
            }

            return 0;
        }

        public static double GetColorComponentInstanceProperty(long iColorComponentInstance, string strProperty)
        {
            long iModel = RDF.engine.GetModel(iColorComponentInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetDatatypeProperty(
                iColorComponentInstance,
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

        public static double GetTransparency(long iColorInstance)
        {
            long iModel = RDF.engine.GetModel(iColorInstance);

            IntPtr values;
            long iCard = 0;

            RDF.engine.GetDatatypeProperty(
                iColorInstance,
                RDF.engine.GetPropertyByName(iModel, "transparency"),
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

        public static int Compare(_color c1, _color c2)
        {
            // R
            if (c1.R > c2.R)
            {
                return 1;
            }
            else if (c1.R < c2.R)
            {
                return -1;
            }

            // G
            if (c1.G > c2.G)
            {
                return 1;
            }
            else if (c1.G < c2.G)
            {
                return -1;
            }

            // B
            if (c1.B > c2.B)
            {
                return 1;
            }
            else if (c1.B < c2.B)
            {
                return -1;
            }

            // W
            if (c1.W > c2.W)
            {
                return 1;
            }
            else if (c1.W < c2.W)
            {
                return -1;
            }

            return 0;
        }

        public static int GetHashCode(_color c)
        {
            return c.R.GetHashCode() + c.G.GetHashCode() + c.B.GetHashCode() + c.W.GetHashCode();
        }

        public int GetHashCode()
        {
            return GetHashCode(this);
        }

        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }
        public float W { get; private set; }
    };

    public class _material
    {
        #region Fields

        private static _material _facesDefaultMaterial;
        private static _material _pointedInstanceMaterial;
        private static _material _selectedInstanceMaterial;
        private static _material _linesDefaultMaterial;

        #endregion // Fields

        #region Methods

        public _material()
        {
        }

        public static _material Create(long iConceptualFaceInstance)
        {
            long iMaterialInstance = RDF.engine.GetConceptualFaceMaterial(iConceptualFaceInstance);
            if (iMaterialInstance != 0)
            {
                long iColorInstance = _color.GetColorInstance(iMaterialInstance);
                if (iColorInstance != 0)
                {
                    _material material = new _material();

                    long iAmbientColorComponentInstance = _color.GetColorComponentInstance(iColorInstance, "ambient");
                    if (iAmbientColorComponentInstance != 0)
                    {
                        material.Ambient = _color.Create(iAmbientColorComponentInstance);
                        material.A = material.Ambient.W;
                    }

                    long iDiffuseColorComponentInstance = _color.GetColorComponentInstance(iColorInstance, "diffuse");
                    if (iDiffuseColorComponentInstance != 0)
                    {
                        material.Diffuse = _color.Create(iDiffuseColorComponentInstance);
                    }

                    long iEmissiveColorComponentInstance = _color.GetColorComponentInstance(iColorInstance, "emissive");
                    if (iEmissiveColorComponentInstance != 0)
                    {
                        material.Emissive = _color.Create(iEmissiveColorComponentInstance);
                    }

                    long iSpecularColorComponentInstance = _color.GetColorComponentInstance(iColorInstance, "specular");
                    if (iSpecularColorComponentInstance != 0)
                    {
                        material.Specular = _color.Create(iSpecularColorComponentInstance);
                    }

                    return material;
                }
            }

            return null;
        }

        public static _material GetFacesDefaultMaterial()
        {
            if (_facesDefaultMaterial == null)
            {
                _facesDefaultMaterial = CreateFacesDefaultMaterial();
            }

            return _facesDefaultMaterial;
        }

        public static _material GetPointedInstanceMaterial()
        {
            if (_pointedInstanceMaterial == null)
            {
                _pointedInstanceMaterial = CreatePointedInstanceMaterial();
            }

            return _pointedInstanceMaterial;
        }

        public static _material GetSelectedInstanceMaterial()
        {
            if (_selectedInstanceMaterial == null)
            {
                _selectedInstanceMaterial = CreateSelectedInstanceMaterial();
            }

            return _selectedInstanceMaterial;
        }

        public static _material GetLinesDefaultMaterial()
        {
            if (_linesDefaultMaterial == null)
            {
                _linesDefaultMaterial = CreateLinesDefaultMaterial();
            }

            return _linesDefaultMaterial;
        }

        private static _material CreateFacesDefaultMaterial()
        {
            var material = new _material();

            material.Ambient = _color.GetFacesDefaultColor();
            material.A = 1;
            material.Diffuse = _color.GetFacesDefaultColor();
            material.Emissive = _color.GetFacesDefaultColor();
            material.Specular = _color.GetFacesDefaultColor();

            return material;
        }

        private static _material CreatePointedInstanceMaterial()
        {
            var material = new _material();

            material.Ambient = _color.GetPointedInstanceColor();
            material.A = 0.70f;
            material.Diffuse = _color.GetPointedInstanceColor();
            material.Emissive = _color.GetPointedInstanceColor();
            material.Specular = _color.GetPointedInstanceColor();

            return material;
        }

        private static _material CreateSelectedInstanceMaterial()
        {
            var material = new _material();

            material.Ambient = _color.GetSelectedInstanceColor();
            material.A = 1f;
            material.Diffuse = _color.GetSelectedInstanceColor();
            material.Emissive = _color.GetSelectedInstanceColor();
            material.Specular = _color.GetSelectedInstanceColor();

            return material;
        }

        private static _material CreateLinesDefaultMaterial()
        {
            var material = new _material();

            material.Ambient = _color.GetLinesDefaultColor();
            material.A = 1;
            material.Diffuse = _color.GetLinesDefaultColor();
            material.Emissive = _color.GetLinesDefaultColor();
            material.Specular = _color.GetLinesDefaultColor();

            return material;
        }

        public bool Equals(_material other)
        {
            int iResult = _color.Compare(Ambient, other.Ambient);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(Diffuse, other.Diffuse);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(Emissive, other.Emissive);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(Specular, other.Specular);
            if (iResult != 0)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as _material);
        }

        public override int GetHashCode()
        {
            return Ambient.GetHashCode() + Diffuse.GetHashCode() + Emissive.GetHashCode() + Specular.GetHashCode();
        }

        #endregion // Methods

        #region Properties

        public _color Ambient { get; set; }
        public _color Diffuse { get; set; }
        public _color Emissive { get; set; }
        public _color Specular { get; set; }
        public float A { get; set; } = 1.0f;
        public string Texture { get; set; }

        #endregion // Properties
    }

    public class _materialComparer : IComparer<_material>
    {
        public int Compare(_material m1, _material m2)
        {
            int iResult = _color.Compare(m1.Ambient, m2.Ambient);
            if (iResult != 0)
            {
                return iResult;
            }

            iResult = _color.Compare(m1.Diffuse, m2.Diffuse);
            if (iResult != 0)
            {
                return iResult;
            }

            iResult = _color.Compare(m1.Emissive, m2.Emissive);
            if (iResult != 0)
            {
                return iResult;
            }

            iResult = _color.Compare(m1.Specular, m2.Specular);
            if (iResult != 0)
            {
                return iResult;
            }

            return 0;
        }
    }

    public class _materialEqualityComparer : IEqualityComparer<_material>
    {
        public bool Equals(_material m1, _material m2)
        {
            int iResult = _color.Compare(m1.Ambient, m2.Ambient);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(m1.Diffuse, m2.Diffuse);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(m1.Emissive, m2.Emissive);
            if (iResult != 0)
            {
                return false;
            }

            iResult = _color.Compare(m1.Specular, m2.Specular);
            if (iResult != 0)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(_material m)
        {
            return m.Ambient.GetHashCode() + m.Diffuse.GetHashCode() + m.Emissive.GetHashCode() + m.Specular.GetHashCode();
        }
    }
    public static class _i64RGBCoder
    {
        public static void encode(int_t i, out float fR, out float fG, out float fB)
        {
            const float STEP = 1f / 255f;

            fR = 0f;
            fG = 0f;
            fB = 0f;

            // R
            if (i >= (255 * 255))
            {
                int_t iR = i / (255 * 255);
                fR = iR * STEP;

                i -= iR * (255 * 255);
            }

            // G
            if (i >= 255)
            {
                int_t iG = i / 255;
                fG = iG * STEP;

                i -= iG * 255;
            }

            // B		
            fB = i * STEP;
        }

        public static int_t decode(byte R, byte G, byte B)
        {
            int_t i = 0;

            // R
            i += R * (255 * 255);

            // G
            i += G * 255;

            // B
            i += B;

            return i;
        }
    }
}
