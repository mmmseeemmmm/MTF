using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.Common.VisionBasicObjects;
using MTFVision.MtfVisionResults;

namespace MTFVision.DoMlfDetection
{
    public class GradientMeasurementResult
    {
        public short result { get; set; } //-1 not tested, 1 nok, 2 ok, 3 error
        public GradientCuts[] gradientMeasurementResult { get; set; }
        public FlatnessResult flattnessResult { get; set; }
    }

    public class GradientCuts
    {
        public string cutName { get; set; }
        public short result { get; set; } //-1 not tested, 1 nok, 2 ok, 3 error
        public short gradientInLimit { get; set; } //-1 not tested, 1 nok, 2 ok, 3 error
        public short verticalPositionInLimit { get; set; } //-1 not tested, 1 nok, 2 ok, 3 error
        public Point startPoint { get; set; }
        public Point endPoint { get; set; }
        public MaxGradientPosition maxGradientPosition { get; set; }
        public double maxGradientValue { get; set; }
        public double verticalPositionRelative { get; set; }
    }

    public class FlatnessResult
    {
        public short result { get; set; } //-1 not tested, 1 nok, 2 ok, 3 error
        public double maxDifference { get; set; }
        public GradientDirection direction { get; set; } //0 vertical 1-horizontal
    }

    //public class LineProfile
    //{
    //    public double[] X { get; set; }
    //    public double[] E { get; set; }
    //}
    //public class GradientProfile
    //{
    //    public double[] X { get; set; }
    //    public double[] Gradient { get; set; }
    //}

    public class MaxGradientPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

}
