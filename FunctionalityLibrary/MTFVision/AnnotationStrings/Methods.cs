using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MTFVision.AnnotationStrings
{
    static class Methods
    {       
        public const string DebugOnlyInitDescription = "Fast Init Vision Driver For Debugging Only";
        public const string InitDescription = "Init Vision Driver";
        public const string CameraConfigsDescription = "Vision Configurations";
        public const string CameraConfigsDisplayName = "Vision Configs";
        public const string ForceVisionInitDisplayName = "Restart Vision";
        public const string ForceVisionInitDescription = "True: Stop Elcom Vision when sequence stop. False: Vision is On all the time";

        public const string ConfigureCamerasDisplayName = "Configure Cameras";
        public const string ConfigureCamerasDescription = "Only in setup mode! this method shows Elcom Camera Manager.\n Use empty variant selector!";

        public const string VirtualCameraNameValueListName = "CameraList";
        public const string VirtualCameraNameDisplayName = "Virtual Camera Name";
        public const string ImageFormatDisplayName = "Image Format";
        public const string ImageRefDisplayName = "Image Reference";
        public const string ImageRefDescription = "Image Reference in Elcom Vision System. This reference links image from Acquire Image with activity which consumes image (i.e. Get Image And Measure Intensity)";
        public const string AcquireImageDisplayName = "Acquire Image";
        public const string AcquireImageDescription = AcquireImageDisplayName + " can return \nNO DATA (data are stored in elcom and accessible via Image reference), \nJPEG image \nRAW Data\n";
        //AdjustMLF
        public const string RefDetectionDataDisplayName = "Detection data ref";
        public const string RefGoldSampleName = "Gold Sample Name ref";
        public const string RefGoldSampleNameDescription = "This is the name of the gold sample reference name";
        public const string RefDetectionDataDescription = "This reference store detection data. The same data can be used with other activities.";
        public const string RefIntensityMeasParamsDisplayNameDescription = "This reference store intensity measurement parameters for main light function detection. The same data can be used with other activities.";
        public const string RefDestinationAreaDisplayName = "Destination area ref";
        public const string RefIntensityMeasParamsDisplayName = "Intensity Measurement Parameters ref";
        public const string RefDestinationAreaDescription = "This parameter set the destination area. The same data can be used with other activities.";
        public const string RefScrewsCfgDisplayName = "ScrewDriver config ref";
        public const string RefScrewsCfgDescription = "This reference stores screw driver config data. The same data can be used with other activities.";
        public const string InputCoordRefDisplayName = "Input coordinate ref";
        public const string InputCoordRefDescription = "This parameter set the zero reference point according previous MLF detection. The reference is set in Elcom Vision";
        public const string ResultCoordRefDisplayName = "Result Coordinate Reference";
        public const string ResultCoordRefDescription = "This parameter store new zero reference point from the destination area for adjustment next light function";
        
        public const string TableNameDisplayName = "Table Name";
        public const string ValidationTableConfig = "ValidationTableConfig";
        public const string TableNameDescription = "Validation table config contains Name of the table\ntable must contains column: " + ValidationTableResults.Min + ", " + ValidationTableResults.Max + " and " + ValidationTableResults.Required+".\nFunction is the prefix for row name.\nIt can contains config for computing ratios between two measure intensities including ratio name and min, max limits";
        
        public const string FunctionDisplayName = "Light Function";
        public const string FunctionDescription = "Light Function Prefix in Row Name in the validation table";
        public const string OutputEnabledDisplayName = "Vision Details Output Enabled";
        public const string OutputEnabledDescription = "(True) Enable activity to send output to MTF, (False) output of the activity is null";
        public const string TimeoutDisplayName = "Timeout [ms]";
        public const string AdjustMlfDisplayName = "Adjust MLF Position";
        public const string AdjustMlfDescription = "Method for an adjusting a Main Light Function using screwdrivers. "+RunSetUpMode;
        //Configure MLT Detection Data
        public const string ConfigMlfDetectionDataDisplayName = "Configure MLF Detection Data";
        public const string ConfigMlfDetectionDataDescription = "This methods configure detection data and goldsample creates gold sample reference";
        
        //Do MLF
        public const string DoMlfDisplayName = "Do MLF Detection";
        public const string GradientDescription = "This reference holds data for gradient settings";
        public const string DoMlfDescription = "Method for a detection if a Main Light Function is in defined area. " +RunSetUpMode;
        public const string RefGradientDisplayName = "Gradients ref";
        //GetImageAndMeasureIntensity
        public const string RoisRefDisplayName = "Actual ROIs reference";
        public const string RoisRefDescription = "Reference for accessing ROIs";
        public const string IntensitySettingRefDisplayName = "Intensity Setting Ref";
        public const string IntensitySettingRefDescription = "Reference for accessing previously created Intensity Setting";
        public const string GetImageAndMeasIntensityDisplayName = "Get Image and Meas Intensity";
        public const string GetImageAndMeasIntensityDescription = "This method gets image via reference from Acquire Image activity and defines areas in which measure intensities. "+RunSetUpMode;
        //GetImageAndMeasureColor
        public const string ColorSettingRefDisplayName = "Color Setting Ref";
        public const string ColorSettingRefDescription = "Reference for accessing previously created Color Setting";
        public const string GetImageAndMeasColorDisplayName = "Get Image and Meas Color";
        public const string GetImageAndMeasColorDescription = "This method gets image via reference from Acquire Image activity and defines areas in which measure color. "+RunSetUpMode;
        //PatternRecognition
        public const string PatternSettingRefDisplayName = "Pattern Setting Ref";
        public const string PatternSettingRefDescription = "Reference for accessing previously created Pattern Setting";
        public const string FindPatternDisplayName = "Get Image and Find Patterns";
        public const string FindPatternDescription = "This method gets image via reference from Acquire Image activity and and recognize a pattern in it. "+RunSetUpMode;

        public const string RunSetUpMode = "For config  run setup mode.";
    }
}
