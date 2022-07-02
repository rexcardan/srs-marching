using System;
using System.Collections.Generic;
using System.Text;

namespace srs_marching
{
    using System.Linq;

    using g3;

    public class FloodFillOp
    {
        public static (DenseGrid3f Grid, AxisAlignedBox3i Bounds) FloodSearch(DenseGrid3f grid, Vector3i start, Func<float, bool> searchCritiera)
        {
            var searchResult = grid.EmptyClone();

            var bounds = grid.BoundsInclusive;
            if (!bounds.Contains(start)) { return (searchResult, AxisAlignedBox3i.Zero); }

            //The stack to search outward - this grows when new values are found
            Stack<Vector3i> stack = new Stack<Vector3i>();
            //A list of previously searched voxels
            HashSet<Vector3i> searched = new HashSet<Vector3i>();
            stack.Push(start);
            searched.Add(start);

            while (stack.Count > 0)
            {
                Vector3i p = stack.Pop();

                if (!bounds.Contains(p)) { continue; }
                var val = grid[p];
                if (searchCritiera(val)) //Grow
                {
                    searchResult[p] = val;
                    Vector3i p1, p2, p3, p4, p5, p6;
                    if (!searched.Contains((p1 = new Vector3i(p.x + 1, p.y, p.z))))
                    {
                        searched.Add(p1);
                        stack.Push(p1);
                    }
                    if (!searched.Contains((p2 = new Vector3i(p.x - 1, p.y, p.z))))
                    {
                        searched.Add(p2);
                        stack.Push(p2);
                    }
                    if (!searched.Contains((p3 = new Vector3i(p.x, p.y - 1, p.z))))
                    {
                        searched.Add(p3);
                        stack.Push(p3);
                    }
                    if (!searched.Contains((p4 = new Vector3i(p.x, p.y + 1, p.z))))
                    {
                        searched.Add(p4);
                        stack.Push(p4);
                    }
                    if (!searched.Contains((p5 = new Vector3i(p.x, p.y, p.z - 1))))
                    {
                        searched.Add(p5);
                        stack.Push(p5);
                    }
                    if (!searched.Contains((p6 = new Vector3i(p.x, p.y, p.z + 1))))
                    {
                        searched.Add(p6);
                        stack.Push(p6);
                    }
                }
                else
                {
                    searchResult[p] = val; //Add but do not grow. This allows a marching cubes algroithmm to find the more accurate boundary
                }
            }

            var min = new Vector3i(int.MaxValue);
            var max = new Vector3i(int.MinValue);
            foreach (var pt in searched)
            {
                min.x = Math.Min(min.x, pt.x);
                min.y = Math.Min(min.y, pt.y);
                min.z = Math.Min(min.z, pt.z);

                max.x = Math.Max(max.x, pt.x);
                max.y = Math.Max(max.y, pt.y);
                max.z = Math.Max(max.z, pt.z);
            }

            return (searchResult, new AxisAlignedBox3i(min, max));
        }

        public static (DenseGrid3f Grid, AxisAlignedBox3i Bounds) PaddedFloodSearch(DenseGrid3f grid, Vector3i start, Func<float, bool> searchCritiera)
        {
            var searchResult = grid.EmptyClone();

            var bounds = grid.CellBoundsInclusive();
            if (!bounds.Contains(start)) { return (searchResult, AxisAlignedBox3i.Zero); }

            //The stack to search outward - this grows when new values are found
            Stack<Vector3i> stack = new Stack<Vector3i>();
            //A list of previously searched voxels
            HashSet<Vector3i> searched = new HashSet<Vector3i>();
            stack.Push(start);
            searched.Add(start);

            while (stack.Count > 0)
            {
                Vector3i p = stack.Pop();

                if (!bounds.Contains(p)) { continue; }
                var val = grid[p];
                if (searchCritiera(val)) //Grow
                {
                    searchResult[p] = val;
                    Vector3i p1, p2, p3, p4, p5, p6;
                    if (!searched.Contains((p1 = new Vector3i(p.x + 1, p.y, p.z))))
                    {
                        searched.Add(p1);
                        stack.Push(p1);
                    }
                    if (!searched.Contains((p2 = new Vector3i(p.x - 1, p.y, p.z))))
                    {
                        searched.Add(p2);
                        stack.Push(p2);
                    }
                    if (!searched.Contains((p3 = new Vector3i(p.x, p.y - 1, p.z))))
                    {
                        searched.Add(p3);
                        stack.Push(p3);
                    }
                    if (!searched.Contains((p4 = new Vector3i(p.x, p.y + 1, p.z))))
                    {
                        searched.Add(p4);
                        stack.Push(p4);
                    }
                    if (!searched.Contains((p5 = new Vector3i(p.x, p.y, p.z - 1))))
                    {
                        searched.Add(p5);
                        stack.Push(p5);
                    }
                    if (!searched.Contains((p6 = new Vector3i(p.x, p.y, p.z + 1))))
                    {
                        searched.Add(p6);
                        stack.Push(p6);
                    }
                }
                else
                {
                    searchResult[p] = val; //Add but do not grow. This allows a marching cubes algroithmm to find the more accurate boundary
                }
            }

            //Grow 1 more voxel layer out
            foreach (var p in searched.ToList())
            {
                Vector3i p1, p2, p3, p4, p5, p6;
                if (!searched.Contains((p1 = new Vector3i(p.x + 1, p.y, p.z))))
                {
                    var index = grid.to_linear(p1);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p1);
                        searchResult[p1] = grid[p1];
                    }
                }
                if (!searched.Contains((p2 = new Vector3i(p.x - 1, p.y, p.z))))
                {
                    var index = grid.to_linear(p2);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p2);
                        searchResult[p2] = grid[p2];
                    }
                }
                if (!searched.Contains((p3 = new Vector3i(p.x, p.y - 1, p.z))))
                {
                    var index = grid.to_linear(p3);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p3);
                        searchResult[p3] = grid[p3];
                    }
                }
                if (!searched.Contains((p4 = new Vector3i(p.x, p.y + 1, p.z))))
                {
                    var index = grid.to_linear(p4);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p4);
                        searchResult[p4] = grid[p4];
                    }
                }
                if (!searched.Contains((p5 = new Vector3i(p.x, p.y, p.z - 1))))
                {
                    var index = grid.to_linear(p5);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p5);
                        searchResult[p5] = grid[p5];
                    }
                }
                if (!searched.Contains((p6 = new Vector3i(p.x, p.y, p.z + 1))))
                {
                    var index = grid.to_linear(p6);
                    if (index > 0 && index < grid.Buffer.Length)
                    {
                        searched.Add(p6);
                        searchResult[p6] = grid[p6];
                    }

                }
            }

            var min = new Vector3i(int.MaxValue);
            var max = new Vector3i(int.MinValue);
            foreach (var p in searched)
            {

                min.x = Math.Min(min.x, p.x);
                min.y = Math.Min(min.y, p.y);
                min.z = Math.Min(min.z, p.z);

                max.x = Math.Max(max.x, p.x);
                max.y = Math.Max(max.y, p.y);
                max.z = Math.Max(max.z, p.z);
            }

            return (searchResult, new AxisAlignedBox3i(min, max));
        }
    }
}
