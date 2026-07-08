using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFCGeometry2RDF;


#if _WIN64
using int_t = System.Int64;
#else
using int_t = System.Int32;
#endif

namespace IFCGeometry2RDF
{
    public interface IInstance
    {
        #region Properties

        int_t ID { get; }
        Geometry Geometry { get; }
        _matrix4x4? Transformation { get; }
        bool Enabled { get; set; }

        #endregion // Properties
    }
}
