using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTFVision.AnnotationStrings;
using MTFVision.DoMlfDetection;

namespace MTFVision.MtfVisionResults
{
    public static class EnumNamesHelper
    {
        public static string GetName(this MLFIntensityMeasurementResult input)
        {
            switch (input)
            {
                case MLFIntensityMeasurementResult.Passed:
                    return ValidationTableResults.Passed;
                case MLFIntensityMeasurementResult.Error:
                    return ValidationTableResults.Error;
                case MLFIntensityMeasurementResult.NotMeasured:
                    return ValidationTableResults.NotMeasured;
                case MLFIntensityMeasurementResult.NotPassed:
                    return ValidationTableResults.NotPass;
                default:
                    return input.ToString();
            };
        }


        public static string GetName(this GradientDirection input)
        {
            switch (input)
            {
                case GradientDirection.vertical:
                    return ValidationTableResults.Vertical;
                case GradientDirection.horizontal:
                    return ValidationTableResults.Horizontal;
                default:
                    return input.ToString();
            };
        }
    }
}
