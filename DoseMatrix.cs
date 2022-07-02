using System;
using System.Collections.Generic;
using System.Text;

namespace srs_marching
{
    using g3;

    public class OrientedTrilinearGrid3f : DenseGrid3f, IImplicitFunction3d
    {
        public OrientedTrilinearGrid3f() : base() { }
        public OrientedTrilinearGrid3f(DenseGrid3f grid) : base(grid.ni, grid.nj, grid.nk, 0)
        {
            Array.Copy(grid.Buffer, this.Buffer, grid.Buffer.Length);
        }

        public OrientedTrilinearGrid3f(int ni, int nj, int nk, float initialValue) : base(ni, nj, nk, initialValue)
        {
        }

        public float BuildSRSDose(double diameter)
        {
            var radius = diameter / 2;
            var center = CellBounds.Center;
            var centerPos = IndexToPosition(center);
            var s1 = 0.249f;
            var s2 = 7.019f;
            var s3 = 0.029f;
            var s4 = 1.927f;

            this.VoxelWiseApply(v =>
                {
                    var r = IndexToPosition(v).Distance(centerPos);

                    if (r <= radius)
                    {
                        return (float)(1 - s1 * Math.Exp(-s2 * (radius - r))) * 100f;
                    }
                    else
                    {
                        return (float)(s3 + (1 - s1 - s3) * Math.Exp(-s4 * (r - radius))) * 100f;
                    }
                });

            return 75;
        }

        public Vector3i PositionToIndex(Vector3f pos)
        {
            var x = (int)((pos.x - Bounds.Min.x) / CellSize.x);
            var y = (int)((pos.y - Bounds.Min.y) / CellSize.y);
            var z = (int)((pos.z - Bounds.Min.z) / CellSize.z);
            return new Vector3i(x, y, z);
        }

        public Vector3f CellSize { get; set; } = Vector3f.One;
        public Frame3f Orientation { get; set; }

        /// <summary>
        /// Trilinearly interpolates from the root voxel to a distance away
        /// </summary>
        /// <param name="nx">root voxel x index</param>
        /// <param name="ny">root voxel y index</param>
        /// <param name="nz">root voxel z index</param>
        /// <param name="dx">distance from center of root voxel in x direction</param>
        /// <param name="dy">distance from center of root voxel in y direction</param>
        /// <param name="dz">distance from center of root voxel in z direction</param>
        /// <returns>the value at dx,dy,dz from the center of the root voxel</returns>
        public double Trilinear(int nx, int ny, int nz, double dx, double dy, double dz)
        {
            return this[nx, ny, nz] * ((1.0 - dx) * (1.0 - dy) * (1.0 - dz))
                         + (this[nx + 1, ny, nz] * (dx * (1.0 - dy) * (1.0 - dz)))
                         + (this[nx, ny + 1, nz] * ((1.0 - dx) * dy * (1.0 - dz)))
                         + (this[nx + 1, ny + 1, nz] * (dx * dy * (1.0 - dz)))
                         + (this[nx, ny, nz + 1] * ((1.0 - dx) * (1.0 - dy) * dz))
                         + (this[nx + 1, ny, nz + 1] * (dx * (1.0 - dy) * dz))
                         + (this[nx, ny + 1, nz + 1] * ((1.0 - dx) * dy * dz))
                         + (this[nx + 1, ny + 1, nz + 1] * (dx * dy * dz));
        }

        // value to return if query point is outside grid (in an SDF
        // outside is usually positive). Need to do math with this value,
        // so don't use double.MaxValue or square will overflow
        public double Outside = Math.Sqrt(Math.Sqrt(double.MaxValue));

        public new OrientedTrilinearGrid3f EmptyClone()
        {
            var clone = new OrientedTrilinearGrid3f(ni, nj, nk, 0)
            {
                Orientation = new Frame3f(Orientation),
                CellSize = new Vector3f(CellSize)
            };
            return clone;
        }

        public OrientedTrilinearGrid3f Get3DSlice(int slice_i, int dimension)
        {
            OrientedTrilinearGrid3f slice;
            if (dimension == 0)
            {
                slice = new OrientedTrilinearGrid3f(nj, nk, 1, 0);
                for (int k = 0; k < nk; ++k)
                    for (int j = 0; j < nj; ++j)
                        slice[j, k, 0] = Buffer[slice_i + ni * (j + nj * k)];
            }
            else if (dimension == 1)
            {
                slice = new OrientedTrilinearGrid3f(ni, nk, 1, 0);
                for (int k = 0; k < nk; ++k)
                    for (int i = 0; i < ni; ++i)
                        slice[i, k, 0] = Buffer[i + ni * (slice_i + nj * k)];
            }
            else
            {
                slice = new OrientedTrilinearGrid3f(ni, nj, 1, 0);
                for (int j = 0; j < nj; ++j)
                    for (int i = 0; i < ni; ++i)
                        slice[i, j, 0] = Buffer[i + ni * (j + nj * slice_i)];
            }
            return slice;
        }

        public DMesh3 MarchingCubes(float isoLevel)
        {
            return MarchingCubesOp.Calculate(this, isoLevel, MarchingCubesOp.MarchResolution.HIGH);
        }

        public DMesh3 MarchingCubes(float isoLevel, AxisAlignedBox3f bounds)
        {
            return MarchingCubesOp.Calculate(this, isoLevel, bounds);
        }

        public double FindLocalIsoVolume(Vector3f localCenter, float isoLevel, bool doSmallVolumeCorrection = true)
        {
            var copy = EmptyClone();
            Array.Copy(Buffer, copy.Buffer, Buffer.Length);
            // Starting at center, search outward and keep only values where value is 100% or higher - zero everything else
            //Going to store the bounds of this shape
            var (localDose, localBounds) = copy.PaddedFloodSearch(localCenter, (v) => v >= isoLevel, 3);

            // Invert direction so negative values are outside iso surface and positive inside - also normalize so surface is zero
            localDose.VoxelWiseApply(v => -v * 100 / isoLevel + 100); //Invert
            var mesh = localDose.MarchingCubes(0, localBounds);
            var triangleCount = mesh.TriangleCount;
            //Correction factor
            var correction = doSmallVolumeCorrection ? 1 + (11.62891 * Math.Pow(triangleCount, -0.5878701)) : 1;
            return mesh.Volume() * correction;
        }

        public (OrientedTrilinearGrid3f Grid, AxisAlignedBox3f Bounds) PaddedFloodSearch(Vector3f start, Func<float, bool> searchCritiera, int numPad = 1)
        {
            Vector3f gridPt = new Vector3f(
                ((start.x - Orientation.Origin.x) / CellSize.x),
                ((start.y - Orientation.Origin.y) / CellSize.y),
                ((start.z - Orientation.Origin.z) / CellSize.z));

            // compute integer coordinates
            var starti = new Vector3i((int)gridPt.x, (int)gridPt.y, (int)gridPt.z);
            var searchResult = EmptyClone();
            var (grid, aabox) = FloodFillOp.PaddedFloodSearch(this, starti, searchCritiera);
            Array.Copy(grid.Buffer, searchResult.Buffer, grid.Buffer.Length);
            var min = IndexToPosition(aabox.Min);
            for (int i = 0; i < numPad; i++)
            {
                if ((min.x - CellSize.x) >= Bounds.Min.x && (min.y - CellSize.y) >= Bounds.Min.y && (min.z - CellSize.z) >= Bounds.Min.z)
                {
                    min = min - CellSize;
                }
            }
            var max = IndexToPosition(aabox.Max);
            for (int i = 0; i < numPad; i++)
            {
                if ((max.x + CellSize.x) <= Bounds.Max.x && (max.y + CellSize.y) <= Bounds.Max.y && (max.z + CellSize.z) <= Bounds.Max.z)
                {
                    max = max + CellSize;
                }
            }
            return (searchResult, new AxisAlignedBox3f(min, max));
        }

        public AxisAlignedBox3f Bounds
        {
            get
            {
                var realWorldBound = IndexToPositionAllowOutOfBounds(Dimensions + new Vector3i(1, 1, 1));

                var max = new Vector3f(Math.Max(realWorldBound.x, Orientation.Origin.x), Math.Max(realWorldBound.y, Orientation.Origin.y), Math.Max(realWorldBound.z, Orientation.Origin.z));
                var min = new Vector3f(Math.Min(realWorldBound.x, Orientation.Origin.x), Math.Min(realWorldBound.y, Orientation.Origin.y), Math.Min(realWorldBound.z, Orientation.Origin.z));

                return new AxisAlignedBox3f(min, max);
            }
        }

        /// <summary>
        /// Takes a 3D cell index and converts it to real world coordinantes
        /// </summary>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public Vector3f IndexToPosition(Vector3i index)
        {
            if (!CellBounds.Contains(index)) { return new Vector3f(float.NaN); }

            var realX = Orientation.Origin.x + (CellSize.x * (index.x - 1)) * Orientation.X.x +
               (CellSize.y * (index.y - 1)) * Orientation.Y.x +
               (CellSize.z * (index.z - 1)) * Orientation.Z.x;

            var realY = Orientation.Origin.y + (CellSize.x * (index.x - 1)) * Orientation.X.y +
                (CellSize.y * (index.y - 1)) * Orientation.Y.y +
                (CellSize.z * (index.z - 1)) * Orientation.Z.y;

            var realZ = Orientation.Origin.z + (CellSize.x * (index.x - 1)) * Orientation.X.z +
                (CellSize.y * (index.y - 1)) * Orientation.Y.z +
                (CellSize.z * (index.z - 1)) * Orientation.Z.z;
            return new Vector3f(realX, realY, realZ);
        }

        /// <summary>
        /// Takes a 3D cell index and converts it to real world coordinantes. But allows out of bounds
        /// </summary>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public Vector3f IndexToPositionAllowOutOfBounds(Vector3i index)
        {
            var realX = Orientation.Origin.x + (CellSize.x * (index.x - 1)) * Orientation.X.x +
               (CellSize.y * (index.y - 1)) * Orientation.Y.x +
               (CellSize.z * (index.z - 1)) * Orientation.Z.x;

            var realY = Orientation.Origin.y + (CellSize.x * (index.x - 1)) * Orientation.X.y +
                (CellSize.y * (index.y - 1)) * Orientation.Y.y +
                (CellSize.z * (index.z - 1)) * Orientation.Z.y;

            var realZ = Orientation.Origin.z + (CellSize.x * (index.x - 1)) * Orientation.X.z +
                (CellSize.y * (index.y - 1)) * Orientation.Y.z +
                (CellSize.z * (index.z - 1)) * Orientation.Z.z;
            return new Vector3f(realX, realY, realZ);
        }

        public double Value(ref Vector3d pt)
        {
            var gridPt = GetGridPoint(pt);
            // compute integer coordinates
            int x0 = (int)gridPt.x;
            int y0 = (int)gridPt.y, y1 = y0 + 1;
            int z0 = (int)gridPt.z, z1 = z0 + 1;

            // clamp to grid
            if (x0 < 0 || (x0 + 1) >= ni ||
                y0 < 0 || y1 >= nj ||
                z0 < 0 || z1 >= nk)
                return Outside;

            // convert double coords to [0,1] range
            double fAx = gridPt.x - (double)x0;
            double fAy = gridPt.y - (double)y0;
            double fAz = gridPt.z - (double)z0;
            return Trilinear(x0, y0, z0, fAx, fAy, fAz);
        }

        /// <summary>
        /// Returns the input real space position as a position in grid coordinates.
        /// Helps find the indices of the point
        /// </summary>
        /// <param name="pt">a point in real world coordinates</param>
        /// <returns></returns>
        public Vector3d GetGridPoint(Vector3d pt)
        {
            return new Vector3d(
                ((pt.x - Orientation.Origin.x) / CellSize.x),
                ((pt.y - Orientation.Origin.y) / CellSize.y),
                ((pt.z - Orientation.Origin.z) / CellSize.z));
        }



        public AxisAlignedBox3i CellBounds
        {
            get { return new AxisAlignedBox3i(0, 0, 0, ni, nj, nk); }
        }
        public AxisAlignedBox3i CellBoundsInclusive
        {
            get { return new AxisAlignedBox3i(0, 0, 0, ni - 1, nj - 1, nk - 1); }
        }

        public Vector3i Dimensions => new Vector3i(ni, nj, nk);
    }
}
