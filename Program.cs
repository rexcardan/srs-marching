using System;
using System.Linq;

namespace srs_marching
{
    using System.Reflection.Metadata.Ecma335;
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
                //Build a 3D cell matrix
                var grid = new DoseMatrix(125, 125, 125, 0);
                grid.CellSize = new Vector3f(1, 1, 1);

                //Build a dose sphere of size r
                var evalLevel = grid.BuildSRSDose(r * 2);
                //Raw marching cubes volume (known to have greater error the smaller the volume)
                var volUnCorrected = grid.FindLocalIsoVolume(grid.Bounds.Center, evalLevel);
                //Proposed correction factor (function of raw volume)
                var cf = new Func<double, double>(vol => 0.9574 * Math.Pow(vol, -0.4103));
                //New volume corrected for small volume triangulation loss
                var volCorrected = volUnCorrected * (1 + cf(volUnCorrected));
                //Calculated volume based on size of sphere
                var trueVol = (4 * Math.PI * Math.Pow(r, 3)) / 3;
                //Gradient calc
                sb.AppendLine($"{r}, {trueVol}, {volUnCorrected}, {volCorrected}");
            }
            Console.WriteLine(sb.ToString());
            Console.ReadLine();
        }
    }
}
