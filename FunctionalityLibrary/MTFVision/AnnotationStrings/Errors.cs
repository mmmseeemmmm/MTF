using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTFVision.AnnotationStrings
{
    static class Errors
    {


        public const string ColumnMissing = "if validation column is missing check in sequence editor table has column " + ValidationTableResults.Min + ", " + ValidationTableResults.Max + " and " + ValidationTableResults.Required;
        public const string RuntimeContextMissing = "RuntimeContext is null";
        public const string CameraNotConfigured = "Camera is not configured";
        public const string SaveImageForLoggingFailed = "Save Image for Logging failed";
        public const string NoResponseAfterSaveImage = "No response after SaveImage command";
        public const string ConfigAdjustMLFCommandFailed = "Config " + AdjustMLFCommandFailed;
        public const string AdjustMLFCommandFailed = "Adjust MLF "+CommandFailed;
        public const string AdjustMLFTimeout = "Adjust MLF timeout";
        public const string CannotSetANewCoordSystem="Cannot set a new coordinate system. Detection point is out of selected area. Please run set up mode";
        public const string ConfigDoMLFCommandFailed = "Config " + DoMLFCommandFailed;
        public const string DoMLFCommandFailed = "Do MLF "+CommandFailed;
        public const string FindPatternCommandFailed = "Find Pattern "+CommandFailed;
        public const string UploadImageFailed = "Upload Image Failed";
        public const string VirtualCameraEmpty = "Virtual Camera cannot be empty";
        public const string StartStreamFailed = "Start Stream "+CommandFailed;
        public const string StopStreamFailed = "Stop Stream "+CommandFailed;
        public const string NoResultFromStopStream = "No result from StopStream";
        public const string VisionResultDidNotContainImage = "Vision Result did not contain image";
        public const string ShutDownElcomVisionFailed = "Shutdown Elcom Vision failed";
        public const string ShutDownElcomHostFailed = "Shutdown Elcom Host failed";
        public const string ImageElcomIsNull = "Image from Elcom is null!";
        public const string LoadGlobalAreaError = "Load Global Area Exception:";
        public const string RoisNull = "Rois are null";
        public const string MeasIntensityFailed = "Meas Intensity " + CommandFailed;
        public const string MeasColorFailed = "Meas Color " + CommandFailed;
        public const string AcquireImageFailed = "Acquire Image " + CommandFailed;
        public const string AcquireAndReturnImageFailed = "Acquire And Return Image " + CommandFailed;
        public const string AcquireImageReturnedMoreThanOneImage = "Acuire Images return more than one image";
        public const string NoValidImage = "!!! - NO valid image - !!!";
        public const string GetImageFailed = "Get Image " + CommandFailed;
        public const string VirtualCameraParamsNull = "Virtual camera parameters cannot be null";
        public const string CreateVirtualCamFailed = "Create Virtual Camera failed";
        public const string GetAllPhysicalCamerasFailed = "Get All Physical Cameras " + CommandFailed;
        public const string NoAreaFound = "No area found. "+Methods.RunSetUpMode;
        public const string SelectSpecificAreasFailed = "Select Specific Areas " + CommandFailed;
        public const string DeserializeResponseSelectSpecificAreasFailed = "Deserialize response from SelectSpecificAreas " + CommandFailed;
        public const string AreasAndIDsCountMismatch = "Areas Count differs from IDs Count";
        public const string ConfigureVirtualCamerasFailed = "Create Virtual Cameras " + CommandFailed;
        public const string CameraConfigWasNotCreatedInInit = "Camera Config was not created in Init";
        public const string CameraConfigDoesNotCorrespontToComponentConfig = "Camera Config does not correspond to component config";
        public const string CameraConfigNotInSetupMode = "Camera Config is not in setup mode, is not saved to sequence";
        public const string CameraConfigEmpty = "Result of ConfigureVirtualCameras is empty";
        public const string CameraConfigCommandEmpty = "Create Configure Virtual Command failed. Command string is empty";
        public const string ElcomHostTimeOut = "Elcom host connection timout";
        public const string ElcomHostNotStarted = "ERROR - Elcom Host NOT started";
        public const string WCFHostPAthEmpty = "WCF host path cannot be empty";
        public const string CannotConnectToEVS = "Can't connect to ELCOM Vision System";
        public const string HostChannelWasNotCreated = "Elcom Host Create Channel Failed";
        public const string CamerasNotConfigured = "No fysical cameras configured";
        
        public const string CommandFailed = "Command Failed";


        public static string CameraNotFound(string ipAdress)
        {
            return string.Format("Camera with IP adress {0} not found. Check configuration!", ipAdress);
        }
        public static string VirtualCameraNotFound(string VCName)
        {
            return string.Format("Virtual Camera {0} not found. Check configuration!. If Camera was added when Elcom Vision was running please close EVS and Elcom Host", VCName);
        }
        public static string ErrorInResponse(string Method, string ErrorText)
        {
            return string.Format("{0} returned error: {1}", Method, ErrorText);
        }
        public static string ErrorReadingImage(string ErrorText)
        {
            return string.Format("Error reading Image: {0}",  ErrorText);
        }
        public static string MergeError(string Error, params string[] OtherErrors)
        {
            if (OtherErrors != null && OtherErrors.Length > 0)
            {
                return string.Format("{0}, {1}", Error, string.Join(",", OtherErrors));

            }
            return Error;
        }

    }
}
