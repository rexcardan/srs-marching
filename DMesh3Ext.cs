using System;
using System.Collections.Generic;
using System.Text;

namespace srs_marching
{
    using g3;

    public static class DMesh3Ext
    {
        public static double Volume(this DMesh3 mesh)
        {
            return MeshMeasurements.VolumeArea(mesh, mesh.TriangleIndices(), (i) => mesh.GetVertex(i)).x;
        }
    }
}
