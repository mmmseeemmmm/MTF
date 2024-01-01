using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.AnnotationStrings
{
    static class GlobalStrings
    {
        public const string Vision = "Vision";
        public const string ImageFormatNoImage = "No Image";
        public const string ImageFormatJpeg = "JPEG";
        public const string ImageFormatRaw = "RAW";
        public const string ImageFormatPng = "PNG";

        public const string True = "True";
        public const string False = "False";

        public const string And = "AND";
        public const string Or = "OR";

        //RuntimeContext
        public const string RuntimeContextCameraCfg = "CameraCfgs";
        public const string RuntimeContextGlobalAreas = "GlobalAreas";

        //Elcom const
        public const string Simulation = "simulation";
        public const string ElcomTimeout = "timeout";
        public const string ElcomHostName = "ElcomHostForWcfUni";
        public const string EVSName = "EVS";
        public const string VirtualCameraName = "cam000";

        //ElcomCmds
        public const string SaveImage = "SaveImage";
        public const string ConfigureAdjustMLF = "ConfigureAdjustMLF";
        public const string AdjustMLF = "AdjustMLF";
        public const string ConfigureMLFDetection = "ConfigureMLFDetection";
        public const string ConfigureMLFDetectionData = "ConfigureMLFDetectionData";
        public const string DoMLFDetection = "DoMLFDetection";
        public const string FindPatterns = "FindPatterns";
        public const string UploadImage = "UploadImage";
        public const string StartImageStream = "StartImageStream";
        public const string StopImageStream = "StopImageStream";
        public const string MeasAbsoluteIntensityValues = "MeasAbsoluteIntensityValues";
        public const string MeasAbsoluteColor = "MeasAbsoluteColor";
        public const string AcquireImage = "AcquireImage";
        public const string AcquireAndReturnImage = "AcquireAndReturnImage";
        public const string CreateVirtualCamera = "CreateVirtualCamera";
        public const string CreateVirtualCameras = "CreateVirtualCameras";
        public const string GetAllPhysicalCameras = "GetAllPhysicalCameras";
        public const string GetImage = "GetImage";
        public const string SelectSpecificAreas = "SelectSpecificAreas";
        public const string ConfigureVirtualCameras = "ConfigureVirtualCameras";

        //Other (Log)
        public const string ImageMetaData = "Image meta data:";
        public const string GlobalAreaDictLoaded="Global Area dictionary loaded:";
        public const string GlobalAreasAndSelectedIDsSaved = "Global areas and Selected IDs Saved";
        public const string CameraConfigSaved = "Camera Config Saved";
        public const string ElcomVisionStarted = "ELCOM Vision System started as:";

    }
}
