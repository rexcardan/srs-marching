using System;
using System.Collections.Generic;
using System.Text;

namespace srs_marching
{
    using g3;

    public static class GridExt
    {
        public static void VoxelWiseApply(this DenseGrid3f grid, Func<Vector3i, float> f)
        {
            for (int k = 0; k < grid.nk; k++)
            {
                for (int j = 0; j < grid.nj; j++)
                {
                    for (int i = 0; i < grid.ni; i++)
                    {
                        int idx = i + grid.ni * (j + grid.nj * k);
                        grid.Buffer[idx] = f(new Vector3i(i, j, k));
                    }
                }
            }
        }

        public static void VoxelWiseApply(this DenseGrid3f grid, Func<float, float> f)
        {
            for (int k = 0; k < grid.nk; k++)
            {
                for (int j = 0; j < grid.nj; j++)
                {
                    for (int i = 0; i < grid.ni; i++)
                    {
                        int idx = i + grid.ni * (j + grid.nj * k);
                        grid.Buffer[idx] = f(grid.Buffer[idx]);
                    }
                }
            }
        }

        public static AxisAlignedBox3i CellBoundsInclusive(this DenseGrid3f grid)
        {
            return new AxisAlignedBox3i(0, 0, 0, grid.ni - 1, grid.nj - 1, grid.nk - 1);
        }

        public static DenseGrid3f EmptyClone(this DenseGrid3f grid)
        {
            return new DenseGrid3f(grid.ni, grid.nj, grid.nk, 0);
        }
    }
}
