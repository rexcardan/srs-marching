using System;

namespace srs_marching
{
    using System.Text;

    using g3;

    internal class Program
    {
        static void Main(string[] args)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"R, Golden Volume, Uncorrected MC, Corrected MC");
            foreach (var r in new double[] { 1.5, 2.5, 3.5, 5, 7.5, 10, 15, 25, 50 })
            {
                var grid = new OrientedTrilinearGrid3f(125, 125, 125, 0);
                grid.CellSize = new Vector3f(1, 1, 1);
                var evalLevel = grid.BuildSRSDose(r * 2);
                var volCorrected = grid.FindLocalIsoVolume(grid.Bounds.Center, evalLevel);
                var volUnCorrected = grid.FindLocalIsoVolume(grid.Bounds.Center, evalLevel, false);
                var trueVol = (4 * Math.PI * Math.Pow(r, 3)) / 3;
                //Gradient calc
                sb.AppendLine($"{r}, {trueVol}, {volUnCorrected}, {volCorrected}");
            }
        }
    }
}
