using System;
using System.Collections.Generic;
using System.Text;

namespace srs_marching
{
    using g3;

    /// Minimalist implicit function interface
    /// </summary>
    public interface IImplicitFunction3d
    {
        double Value(ref Vector3d pt);
    }
}
