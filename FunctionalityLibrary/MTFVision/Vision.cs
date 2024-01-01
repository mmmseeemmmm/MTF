using AutomotiveLighting.MTFCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.ServiceModel.Configuration;
using AL.Utils.Logging;
using ElcomTypes;
using MTFVision.DoMlfDetection;
using MTFVision.HelperClasses;
using MTFVision.MtfVisionResults;
using MTFVision.AdjustMlf;
using MTFVision.Config;
using MTFVision.AnnotationString;
using MTFVision.CameraServer;
using MTFVision.FindPattern;
using MTFVision.MlfResult;
using MTFVision.AnnotationStrings;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace MTFVision
{

    [MTFClass(Icon = MTFIcons.Vision)]
    [MTFClassCategory("Vision")]
    public class Vision : IDisposable, IElcomUniServiceCallback, ICanStop
    {
        #region MTFInterface
        public IMTFSequenceRuntimeContext RuntimeContext { get; set; }
        private List<Guid> ActivityPath { get; set; }
        private readonly string _serverUri;
        private IElcomUniService _service;
        private Process _wcfApp;
        private string _WCFHostExe;
        private DuplexChannelFactory<IElcomUniService> _myChannelFactory;
        private List<CameraConfig> cameraConfigs;
        private static bool _enableAcquireImageLog;
        private static bool _enableMessageLogging;
        private static string _acquireImagePath;
        private static int _acquireImageFormat;
        //private VirtualCameraConfigurations _vcCamConfig = new VirtualCameraConfigurations();
        private GlobalRoiDictionary _globalRoiDictionary = new GlobalRoiDictionary();
        private CameraDict _cameraConfigDict;
        private readonly Dictionary<string, object> _reportConfig;
        private bool stop;
        private static bool asyncResponseCmdReceived;
        private static bool asyncResponseImageReceived;
        private static ElcomImage asyncImage;
        private static string asyncCmdData;
        private bool visionTimeout;
        private bool visionErrorCommand;
        private bool forceReinitVision;
        private bool visionSetupCanceled;

        [MTFConstructor(ParameterHelperClassName = "MTFVision.VisionConfigHelper")]
        [MTFAdditionalParameterInfo(ParameterName = "cameraConfigs", Description = Methods.CameraConfigsDescription, DisplayName = Methods.CameraConfigsDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "forceVisionInit", Description = Methods.ForceVisionInitDescription, DisplayName = Methods.ForceVisionInitDisplayName)]
        [MTFAllowedParameterValue("imageFormat", GlobalStrings.ImageFormatJpeg, 1)]
        [MTFAllowedParameterValue("imageFormat", GlobalStrings.ImageFormatRaw, 3)]
        public Vision(string serverUri, string wcfHostExe, List<MTFTabControl> cameraConfigs, List<MTFTabControl> reportConfigs, bool enableAcquireImageLog, string absoluteLogPath, int imageFormat, bool enableMessageLog, bool forceVisionInit)
        {
            _enableMessageLogging = enableMessageLog;
            _enableAcquireImageLog = enableAcquireImageLog;
            _acquireImagePath = absoluteLogPath;
            _acquireImageFormat = imageFormat;
            _serverUri = serverUri;
            this.cameraConfigs = createCamConfig(cameraConfigs);
            _reportConfig = CreateReportConfig(reportConfigs);
            _WCFHostExe = wcfHostExe;
            forceReinitVision = forceVisionInit;
        }

        public Vision(string serverUri, string wcfHostExe, List<MTFTabControl> cameraConfigs, List<MTFTabControl> reportConfigs, bool enableAcquireImageLog, string absoluteLogPath, int imageFormat, bool enableMessageLog)
            : this(serverUri, wcfHostExe, cameraConfigs, reportConfigs, enableAcquireImageLog, absoluteLogPath, imageFormat, enableMessageLog, false)
        {
        }

        [MTFMethod(Description = Methods.InitDescription)]
        public void Init()
        {
            visionTimeout = false;
            stop = false;
            asyncResponseCmdReceived = false;
            asyncResponseCmdReceived = false;
            visionSetupCanceled = false;
            asyncImage = null;
            asyncCmdData = string.Empty;
            imageCount = 0;
            InitVision();
            visionErrorCommand = false;
        }

        private void InitVision()
        {
            if (forceReinitVision)
            {
                StopElcom();
            }
            if (RuntimeContext == null)
            {
                throw new NullReferenceException(Errors.RuntimeContextMissing);
            }
            VisionSerializableData.RuntimeContext = RuntimeContext;
            _acquireImagePath = Path.Combine(_acquireImagePath, RuntimeContext.SequenceName);

            if (RuntimeContext != null)
            {
                RuntimeContext.ProgressNotification(0, InitStrings.Initializing);
            }
            bool visionRunning = IsVisionRunnable();
            if (!visionRunning)
            {
                StopElcom();
            }
            ConnectElcom(35000, visionRunning);
            if (RuntimeContext != null)
            {
                if (!stop)
                {
                    RuntimeContext.ProgressNotification(50, InitStrings.SearchingCameras);
                }
            }
            if (RuntimeContext != null)
            {
                if (!stop)
                {
                    RuntimeContext.ProgressNotification(70, InitStrings.InitializingCameras);
                }
            }

            if (RuntimeContext != null)
            {
                _cameraConfigDict = RuntimeContext.LoadData<CameraDict>(GlobalStrings.RuntimeContextCameraCfg) ?? new CameraDict { CameraConfigs = new Dictionary<string, CameraConfigParams>() };
            }
            if (!visionRunning)
            {
                List<VirtualCameraConfiguration> l;
                var cameras = GetAllPhysicalCamerasStep(true);
                PrepareVirtualCameras(cameras);

            }
            else
            {
                RuntimeContext.ProgressNotification(100);
            }
        }

        private void PrepareVirtualCameras(string[] cameras)
        {
            if (cameras == null)
            {
                throw new Exception(Errors.CameraNotConfigured);
            }
            var lData = new List<CameraConfigParams>();
            List<string> redundantKeys = new List<string>();
            foreach (var item in _cameraConfigDict.CameraConfigs) //Find removed cameras from component config
            {
                bool deleteItem = true;
                foreach (var camera in cameraConfigs)
                {
                    if (item.Value.VirtualCameraName == camera.VirtualCameraName)
                    {
                        deleteItem = false;
                    }
                }
                if (deleteItem)
                {
                    redundantKeys.Add(item.Key);
                }
            }
            foreach (var item in redundantKeys) //remove redundant cameras
            {
                _cameraConfigDict.CameraConfigs.Remove(item);
            }

            foreach (var camCfg in cameraConfigs)
            {


                ushort[] cameraParams = null;

                if (_cameraConfigDict.CameraConfigs.ContainsKey(camCfg.VirtualCameraName))
                {
                    if (camCfg.IsSimulation && _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName].SimulatedVirtualCameraParams != null)
                    {
                        cameraParams = _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName].SimulatedVirtualCameraParams;
                    }
                    if (!camCfg.IsSimulation && _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName].VirtualCameraParams != null)
                    {
                        cameraParams = _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName].VirtualCameraParams;
                    }

                }

                CameraConfigParams data;
                if (!camCfg.IsSimulation)
                {
                    var cameraStr = cameras.FirstOrDefault(c => c.EndsWith(camCfg.IPAddress));
                    if (string.IsNullOrEmpty(cameraStr))
                    {
                        throw new Exception(Errors.CameraNotFound(camCfg.IPAddress));
                    }

                    var camCfgSplit = cameraStr.Split('\\');
                    data = new CameraConfigParams
                    {
                        VirtualCameraName = camCfg.VirtualCameraName,
                        CameraIP = camCfg.IPAddress,
                        IsSimulated = camCfg.IsSimulation,
                        CameraSN = camCfgSplit[0],
                        VirtualCameraParams = cameraParams
                    };
                    _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName] = data;
                }
                else
                {
                    data = new CameraConfigParams
                    {
                        VirtualCameraName = camCfg.VirtualCameraName,
                        CameraIP = camCfg.IPAddress,
                        IsSimulated = camCfg.IsSimulation,
                        CameraSN = GlobalStrings.Simulation,
                        SimulatedVirtualCameraParams = cameraParams
                    };
                    _cameraConfigDict.CameraConfigs[camCfg.VirtualCameraName] = data;

                }

                if (stop)
                {
                    RuntimeContext.ProgressNotification(100, InitStrings.Stop);
                    return;
                }
                lData.Add(data);

            }

            CreateVirtualCameras(lData);
        }

        [MTFValueListGetterMethod]
        public List<Tuple<string, object>> CameraList()
        {
            var cameraList = new List<Tuple<string, object>>();

            foreach (var item in cameraConfigs)
            {
                cameraList.Add(new Tuple<string, object>(item.VirtualCameraName, item.VirtualCameraName));
            }

            return cameraList;
        }

        [MTFMethod(SetupModeAvailable = true, DisplayName = Methods.ConfigureCamerasDisplayName, Description = Methods.ConfigureCamerasDescription)]
        public void ConfifureCameras()
        {
            if (RuntimeContext.IsSetupMode)
            {
                ConfigureCameras(true, true);
            }
        }
        [MTFKnownClass]
        public class KeyChanger
        {
            public string OldKey { get; set; }
            public string NewKey { get; set; }
        }
        [MTFAdditionalParameterInfo(ParameterName = "ChangedItems", DisplayName = Methods.RefDetectionDataDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DetectionData)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.DetectionData }, DisplayName = "RenameDatas", Description = "RenameInnerKeyIntheDetectionDatas")]
        public void RenameDatas(List<KeyChanger> ChangedItems)
        {
            var data = RuntimeContext.LoadData<VisionConfigDictionary<string>>(AnnotationSettings.DetectionData);
            foreach (var item in ChangedItems)
            {

                if (data != null && data.Dict.ContainsKey(item.OldKey))
                {
                    var innerData = data.Dict[item.OldKey];
                    data.Dict.Remove(item.OldKey);
                    if (!string.IsNullOrEmpty(item.NewKey))
                    {
                        data.Dict[item.NewKey] = innerData;
                    }
                }
            }
            RuntimeContext.SaveData(AnnotationSettings.DetectionData, data);

        }

        [MTFAllowedParameterValue("imageFormat", GlobalStrings.ImageFormatNoImage, -1)]
        [MTFAllowedParameterValue("imageFormat", GlobalStrings.ImageFormatJpeg, 1)]
        [MTFAllowedParameterValue("imageFormat", GlobalStrings.ImageFormatRaw, 3)]
        [MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = Methods.VirtualCameraNameValueListName, DisplayName = Methods.VirtualCameraNameDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "imageFormat", DisplayName = Methods.ImageFormatDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFMethod(DisplayName = Methods.AcquireImageDisplayName, Description = Methods.AcquireImageDescription)]
        public MtfElcomImage AcquireImage(string virtualCameraName, string imageRef, int imageFormat, bool overlay)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            if (string.IsNullOrEmpty(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraEmpty);
            }
            if (!IsVirtualCameraConfigured(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraNotFound(virtualCameraName));
            }
            MtfElcomImage image = null;
            if (imageFormat == -1)
            {
                AcquireImageOnly(virtualCameraName, imageRef);
            }
            else
            {
                image = AcquireAndReturnImage(virtualCameraName, imageRef, imageFormat, overlay);
                if (image != null)
                {
                    if (imageFormat == 1)
                    {
                        NotifyImageAcquired(image.ImageData);
                        //RuntimeContext.ImageNotification(, RuntimeContext.ActivityPathIds);
                    }
                }
            }
            if (_enableAcquireImageLog)
            {
                AcquireImageLog ackImgLog = new AcquireImageLog()
                {
                    absolutePath = _acquireImagePath,
                    imageFormat = _acquireImageFormat,
                    imgRefName = imageRef,
                    overlay = overlay,
                };
                var dataToSend = JsonConvert.SerializeObject(ackImgLog);
                Command cmd = new Command()
                {
                    Text = GlobalStrings.SaveImage,
                    Data = dataToSend
                };
                string result;
                try
                {
                    result = _service.Command(JsonConvert.SerializeObject(cmd));
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.SaveImageForLoggingFailed, e);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(result, GlobalStrings.Vision, true);
                    }
                }
                else
                {
                    throw new Exception(Errors.NoResponseAfterSaveImage);
                }
                var cmdResponse = Deserialize(result);
                //.AddImage(imageRef, cmdResponse.Data);
            }
            if (image != null && image.IsLarge)
            {
                GC.Collect();
            }
            return image;
        }

        [MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = Methods.VirtualCameraNameValueListName, DisplayName = Methods.VirtualCameraNameDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "refDetectionData", DisplayName = Methods.RefDetectionDataDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DetectionData)]
        [MTFAdditionalParameterInfo(ParameterName = "refDestinationArea", DisplayName = Methods.RefDestinationAreaDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DestinationArea)]
        [MTFAdditionalParameterInfo(ParameterName = "refScrewsCfg", DisplayName = Methods.RefScrewsCfgDisplayName, Description = Methods.RefScrewsCfgDescription, DataNameExtension = AnnotationSettings.ScrewDriverConfig)]
        [MTFAdditionalParameterInfo(ParameterName = "inputCoordRef", DisplayName = Methods.InputCoordRefDisplayName, Description = Methods.InputCoordRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "resultCoordRef", DisplayName = Methods.ResultCoordRefDisplayName, Description = Methods.ResultCoordRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "tableName", DisplayName = Methods.TableNameDisplayName, Description = Methods.TableNameDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "function", DisplayName = Methods.FunctionDisplayName, Description = Methods.FunctionDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "outputEnabled", DisplayName = Methods.OutputEnabledDisplayName, Description = Methods.OutputEnabledDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "timeout", DisplayName = Methods.TimeoutDisplayName)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.DetectionData, AnnotationSettings.DestinationArea, AnnotationSettings.ScrewDriverConfig }, DisplayName = Methods.AdjustMlfDisplayName, Description = Methods.AdjustMlfDescription)]
        public MtfMlfDetectionResults AdjustMlfPosition(string virtualCameraName, string imageRef, string refDetectionData, string refDestinationArea, string refScrewsCfg, string inputCoordRef, string resultCoordRef, string tableName, string function, bool outputEnabled, ushort timeout)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            if (string.IsNullOrEmpty(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraEmpty);
            }
            if (!IsVirtualCameraConfigured(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraNotFound(virtualCameraName));
            }

            var detData = VisionSerializableData.Load<string>(AnnotationSettings.DetectionData, refDetectionData);
            if (detData == null)
            {
                throw new NullReferenceException("Vision Detection Data Empty, Run Configure Detection Data");
            }
            string destArea;
            string scrDriverCfg;
            ushort setupLvl = 0;
            if (RuntimeContext.IsTeachMode || RuntimeContext.IsSetupMode)
            {
                setupLvl = 1;
            }
            if (RuntimeContext.IsServiceMode)
            {
                setupLvl = 0;
            }




            if (RuntimeContext.IsSetupMode)
            {
                destArea = VisionSerializableData.Load<string>(AnnotationSettings.DestinationArea, refDestinationArea);
                scrDriverCfg = VisionSerializableData.Load<string>(AnnotationSettings.ScrewDriverConfig, refScrewsCfg);

                var cfgData = new ConfigureAdjustMLFDetection()
                {
                    virtualCameraName = virtualCameraName,
                    imgRefName = imageRef,
                    detectionData = detData,
                    destinationArea = destArea,
                    screwsParams = scrDriverCfg,
                    inputCoordRef = inputCoordRef,
                    resultCoordRef = resultCoordRef,
                    setupLevel = setupLvl,

                };
                var cfgDataToSend = JsonConvert.SerializeObject(cfgData);
                var cfgCmd = new Command()
                {
                    Text = GlobalStrings.ConfigureAdjustMLF,
                    Data = cfgDataToSend
                };
                string asyncData;
                try
                {
                    asyncData = StopableCmd(JsonConvert.SerializeObject(cfgCmd));
                    if (asyncData == null)
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.ConfigAdjustMLFCommandFailed, e);
                }


                if (!string.IsNullOrEmpty(asyncData))
                {
                    var configureAdjustMlfResult = JsonConvert.DeserializeObject<AdjustConfig>(asyncData);

                    if (configureAdjustMlfResult != null)
                    {
                        if (!string.IsNullOrEmpty(configureAdjustMlfResult.destinationArea))
                        {
                            destArea = configureAdjustMlfResult.destinationArea;
                            VisionSerializableData.Save(AnnotationSettings.DestinationArea, refDestinationArea, destArea);

                        }
                        if (!string.IsNullOrEmpty(configureAdjustMlfResult.screwsParams))
                        {
                            scrDriverCfg = configureAdjustMlfResult.screwsParams;
                            VisionSerializableData.Save(AnnotationSettings.ScrewDriverConfig, refScrewsCfg, scrDriverCfg);
                        }
                    }
                }
            }
            else
            {
                destArea = VisionSerializableData.Load<string>(AnnotationSettings.DestinationArea, refDestinationArea);
                scrDriverCfg = VisionSerializableData.Load<string>(AnnotationSettings.ScrewDriverConfig, refScrewsCfg);
            }


            var data = new AdjustMLFDetection()
            {
                virtualCameraName = virtualCameraName,
                imgRefName = imageRef,
                detectionData = detData,
                destinationArea = destArea,
                screwsParams = scrDriverCfg,
                inputCoordRef = inputCoordRef,
                resultCoordRef = resultCoordRef,
                timeout = timeout,

            };
            var dataToSend = JsonConvert.SerializeObject(data);
            var cmd = new Command()
            {
                Text = GlobalStrings.AdjustMLF,
                Data = dataToSend
            };
            ElcomImage res;
            try
            {
                res = StopableImageCmd(cmd);
            }
            catch (Exception e)
            {

                throw new Exception(Errors.AdjustMLFCommandFailed, e);
            }

            var visionResult = new MtfMlfDetectionResultsDetails();
            if (res != null)
            {
                if (_enableMessageLogging)
                {
                    Log.LogMessage(GlobalStrings.ImageMetaData + res.JSONData, GlobalStrings.Vision, true);
                }
                if (res.JSONData != null)
                {
                    var cmdReq = Deserialize(res.JSONData);
                    //Command cmdReq = JsonConvert.DeserializeObject<Command>(res.JSONData);
                    //if (cmdReq.ErrorText != string.Empty)
                    //{
                    //    throw new InvalidOperationException(Errors.ErrorReadingImage(cmdReq.ErrorText));
                    //}
                    if (!string.IsNullOrEmpty(cmdReq.Data))
                    {
                        if (cmdReq.Data.ToLower() == GlobalStrings.ElcomTimeout)
                        {
                            throw new TimeoutException(Errors.AdjustMLFTimeout);
                        }
                        var responseDoMlfDetection = JsonConvert.DeserializeObject<MtfMlfResult>(cmdReq.Data);
                        visionResult.DetectionType = responseDoMlfDetection.resultType;
                        visionResult.PointinArea = responseDoMlfDetection.detectionPointInArea;
                        switch ((DetectionTypes)responseDoMlfDetection.resultType)
                        {
                            case DetectionTypes.CutOffLine:
                                var cutOffLine = JsonConvert.DeserializeObject<CutofflineResult>(responseDoMlfDetection.resultData);
                                visionResult.CutOffLineResults = cutOffLine;
                                visionResult.IsoLineResults = null;
                                visionResult.IsoPercentResults = null;
                                break;
                            case DetectionTypes.IsoPercent:
                                var isoPercent = JsonConvert.DeserializeObject<IsopercentDetectionResult>(responseDoMlfDetection.resultData);
                                visionResult.IsoPercentResults = isoPercent;
                                visionResult.CutOffLineResults = null;
                                visionResult.IsoLineResults = null;
                                break;
                            case DetectionTypes.IsoLine:
                                var isoLine = JsonConvert.DeserializeObject<IsolineDetectionResult>(responseDoMlfDetection.resultData);
                                visionResult.IsoLineResults = isoLine;
                                visionResult.CutOffLineResults = null;
                                visionResult.IsoPercentResults = null;
                                break;
                        }
                    }

                }
            }
            if (res != null)
            {

                var images = GetImageArrayFromByteArray(res.Image);
                visionResult.Images = images;
                foreach (var image in images)
                {
                    NotifyImageAcquired(image.ImageData);
                }
            }
            if (!string.IsNullOrEmpty(resultCoordRef) && (visionResult.PointinArea != 2))
            {
                RuntimeContext.RaiseException(this, new Exception(Errors.CannotSetANewCoordSystem), ExceptionLevel.JustInfo); ;
            }
            var passed = FillValidationTable(visionResult, new MlfValidationTableConfig() { TableName = tableName, Function = function }, visionResult.PointinArea, visionTimeout);

            return outputEnabled ? new MtfMlfDetectionResults() { VisionResultDetails = visionResult, TestPassed = passed } : new MtfMlfDetectionResults() { VisionResultDetails = null, TestPassed = passed };
        }
        [MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = Methods.VirtualCameraNameValueListName, DisplayName = Methods.VirtualCameraNameDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "refDetectionData", DisplayName = Methods.RefDetectionDataDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DetectionData)]
        [MTFAdditionalParameterInfo(ParameterName = "goldSampleName", DisplayName = Methods.RefGoldSampleName, Description = Methods.RefDetectionDataDescription)]

        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.DetectionData }, DisplayName = Methods.ConfigMlfDetectionDataDisplayName, Description = Methods.ConfigMlfDetectionDataDisplayName)]
        public void ConfigureDetectionData(string virtualCameraName, string imageRef, string refDetectionData, string goldSampleName)
        {
            string detData;
            ushort setupLvl = 0;
            if (RuntimeContext.IsTeachMode || RuntimeContext.IsSetupMode)
            {
                setupLvl = 1;
            }
            if (RuntimeContext.IsServiceMode)
            {
                setupLvl = 0;
            }

            // GoldSampleConfigureDoMLFDetectionData
            //if (RuntimeContext.IsTeachMode)
            if (RuntimeContext.IsSetupMode /*IsTeachMode*/)
            {
                detData = VisionSerializableData.Load<string>(AnnotationSettings.DetectionData, refDetectionData);
                //destAreaData = VisionSerializableData.Load<string>(AnnotationSettings.DestinationArea, refDestinationArea);



                var cfgData = new ConfigureMLFDetectionData()
                {
                    virtualCameraName = virtualCameraName,
                    imgRefName = imageRef,
                    detectionData = detData,
                    setupLevel = setupLvl,
                    goldenSampleName = goldSampleName,
                };
                var cfgDataToSend = JsonConvert.SerializeObject(cfgData);
                var cfgCmd = new Command()
                {
                    Text = GlobalStrings.ConfigureMLFDetectionData,
                    Data = cfgDataToSend
                };
                string asyncData = null;
                try
                {
                    asyncData = StopableCmd(JsonConvert.SerializeObject(cfgCmd));
                    if (asyncData == null)
                    {
                        return;
                    }
                    //cfgResult = _service.Command(JsonConvert.SerializeObject(cfgCmd));
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.ConfigDoMLFCommandFailed, e);
                }


                if (!string.IsNullOrEmpty(asyncData))
                {
                    var configDoMlfDetectionResponse = JsonConvert.DeserializeObject<ResponseConfigDoMlfDetectionData>(asyncData);
                    if (configDoMlfDetectionResponse != null)
                    {
                        if (!string.IsNullOrEmpty(configDoMlfDetectionResponse.detectionData))
                        {
                            detData = configDoMlfDetectionResponse.detectionData;
                            VisionSerializableData.Save(AnnotationSettings.DetectionData, refDetectionData, detData);
                        }
                    }
                }
            }
        }

        [MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = Methods.VirtualCameraNameValueListName, DisplayName = Methods.VirtualCameraNameDisplayName)]
        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "refDetectionData", DisplayName = Methods.RefDetectionDataDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DetectionData)]
        [MTFAdditionalParameterInfo(ParameterName = "refDestinationArea", DisplayName = Methods.RefDestinationAreaDisplayName, Description = Methods.RefDetectionDataDescription, DataNameExtension = AnnotationSettings.DestinationArea)]
        [MTFAdditionalParameterInfo(ParameterName = "refIntensityMeasurementResults", DisplayName = Methods.RefIntensityMeasParamsDisplayName, Description = Methods.RefIntensityMeasParamsDisplayNameDescription, DataNameExtension = AnnotationSettings.IntensityMeasurementParams)]
        [MTFAdditionalParameterInfo(ParameterName = "inputCoordRef", DisplayName = Methods.InputCoordRefDisplayName, Description = Methods.InputCoordRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "resultCoordRef", DisplayName = Methods.ResultCoordRefDisplayName, Description = Methods.ResultCoordRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "ResultConfig", DisplayName = Methods.ValidationTableConfig, Description = Methods.TableNameDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "refGradientsResults", DisplayName = Methods.RefGradientDisplayName, Description = Methods.GradientDescription, DataNameExtension = AnnotationSettings.GradientMeasurementSetting)]
        //[MTFAdditionalParameterInfo(ParameterName = "function", DisplayName = Methods.FunctionDisplayName, Description = Methods.FunctionDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "outputEnabled", DisplayName = Methods.OutputEnabledDisplayName, Description = Methods.OutputEnabledDescription)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.DetectionData, AnnotationSettings.DestinationArea, AnnotationSettings.IntensityMeasurementParams, AnnotationSettings.GradientMeasurementSetting }, DisplayName = Methods.DoMlfDisplayName, Description = Methods.DoMlfDescription)]
        public MtfMlfDetectionResults DoMlfDetection(string virtualCameraName, string imageRef, string refDetectionData, string refDestinationArea, string refIntensityMeasurementResults, string refGradientsResults, string inputCoordRef, string resultCoordRef, MlfValidationTableConfig ResultConfig, bool outputEnabled)
        {

            ActivityPath = RuntimeContext.ActivityPathIds;
            if (string.IsNullOrEmpty(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraEmpty);
            }
            if (!IsVirtualCameraConfigured(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraNotFound(virtualCameraName));
            }
            var detData = VisionSerializableData.Load<string>(AnnotationSettings.DetectionData, refDetectionData);
            if (detData == null)
            {
                throw new NullReferenceException("Vision Detection Data Empty, teach data on Gold Sample");
            }
            string destAreaData;
            string intensityMeasurementParams;
            string gradientsMeas;
            ushort setupLvl = 0;
            if (RuntimeContext.IsTeachMode || RuntimeContext.IsSetupMode)
            {
                setupLvl = 1;
            }
            if (RuntimeContext.IsServiceMode)
            {
                setupLvl = 0;
            }


            //ConfigureDoMLF Detection
            if (RuntimeContext.IsSetupMode)
            {
                destAreaData = VisionSerializableData.Load<string>(AnnotationSettings.DestinationArea, refDestinationArea);
                intensityMeasurementParams = VisionSerializableData.Load<string>(AnnotationSettings.IntensityMeasurementParams, refIntensityMeasurementResults);
                gradientsMeas = VisionSerializableData.Load<string>(AnnotationSettings.GradientMeasurementSetting, refGradientsResults);


                var cfgData = new ConfigureMLFDetection()
                {
                    virtualCameraName = virtualCameraName,
                    imgRefName = imageRef,
                    detectionData = detData,
                    intensityPointsMeas = intensityMeasurementParams,
                    gradientsMeas = gradientsMeas,
                    destinationArea = destAreaData,
                    inputCoordRef = inputCoordRef,
                    resultCoordRef = resultCoordRef,

                    setupLevel = setupLvl,
                };
                var cfgDataToSend = JsonConvert.SerializeObject(cfgData);
                var cfgCmd = new Command()
                {
                    Text = GlobalStrings.ConfigureMLFDetection,
                    Data = cfgDataToSend
                };
                string asyncData = null;
                try
                {
                    asyncData = StopableCmd(JsonConvert.SerializeObject(cfgCmd));
                    if (asyncData == null)
                    {
                        return null;
                    }
                    //cfgResult = _service.Command(JsonConvert.SerializeObject(cfgCmd));
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.ConfigDoMLFCommandFailed, e);
                }


                if (!string.IsNullOrEmpty(asyncData))
                {
                    var configDoMlfDetectionResponse = JsonConvert.DeserializeObject<ResponseConfigDoMlfDetection>(asyncData);
                    if (configDoMlfDetectionResponse != null)
                    {
                        //if (!string.IsNullOrEmpty(configDoMlfDetectionResponse.detectionData))
                        //{
                        //    detData = configDoMlfDetectionResponse.detectionData;
                        //    VisionSerializableData.Save(AnnotationSettings.DetectionData, refDetectionData, detData);
                        //}
                        if (!string.IsNullOrEmpty(configDoMlfDetectionResponse.destinationArea))
                        {
                            destAreaData = configDoMlfDetectionResponse.destinationArea;
                            VisionSerializableData.Save(AnnotationSettings.DestinationArea, refDestinationArea, destAreaData);
                        }
                        if (!string.IsNullOrEmpty(configDoMlfDetectionResponse.intensityPointsMeas))
                        {
                            intensityMeasurementParams = configDoMlfDetectionResponse.intensityPointsMeas;
                            VisionSerializableData.Save(AnnotationSettings.IntensityMeasurementParams, refIntensityMeasurementResults, intensityMeasurementParams);
                        }
                        if (!string.IsNullOrEmpty(configDoMlfDetectionResponse.gradientsMeas))
                        {
                            gradientsMeas = configDoMlfDetectionResponse.gradientsMeas;
                            VisionSerializableData.Save(AnnotationSettings.GradientMeasurementSetting, refGradientsResults, gradientsMeas);
                        }

                    }
                }
            }
            else
            {
                //detData = VisionSerializableData.Load<string>(AnnotationSettings.DetectionData, refDetectionData);
                destAreaData = VisionSerializableData.Load<string>(AnnotationSettings.DestinationArea, refDestinationArea);
                intensityMeasurementParams = VisionSerializableData.Load<string>(AnnotationSettings.IntensityMeasurementParams, refIntensityMeasurementResults);
                gradientsMeas = VisionSerializableData.Load<string>(AnnotationSettings.GradientMeasurementSetting, refGradientsResults);
            }


            var data = new DoMLFDetection()
            {
                virtualCameraName = virtualCameraName,
                imgRefName = imageRef,
                detectionData = detData,
                intensityPointsMeas = intensityMeasurementParams,
                gradientsMeas = gradientsMeas,
                destinationArea = destAreaData,
                inputCoordRef = inputCoordRef,
                resultCoordRef = resultCoordRef,
                setupLevel = setupLvl,


            };
            var dataToSend = JsonConvert.SerializeObject(data);
            var cmd = new Command()
            {
                Text = GlobalStrings.DoMLFDetection,
                Data = dataToSend
            };
            ElcomImage res;
            try
            {
                res = _service.CommandImage(new ElcomImage() { JSONData = JsonConvert.SerializeObject(cmd) });
            }
            catch (Exception e)
            {

                throw new Exception(Errors.DoMLFCommandFailed, e);
            }

            var visionResult = new MtfMlfDetectionResultsDetails();
            if (res != null)
            {
                if (_enableMessageLogging)
                {
                    Log.LogMessage(GlobalStrings.ImageMetaData + res.JSONData, GlobalStrings.Vision, true);
                }
                if (res.JSONData != null)
                {
                    var cmdReq = Deserialize(res.JSONData);
                    //Command cmdReq = JsonConvert.DeserializeObject<Command>(res.JSONData);
                    //if (cmdReq.ErrorText != string.Empty)
                    //{
                    //    throw new InvalidOperationException(Errors.ErrorReadingImage(cmdReq.ErrorText));
                    //}
                    if (!string.IsNullOrEmpty(cmdReq.Data))
                    {
                        var responseDoMlfDetection = JsonConvert.DeserializeObject<MtfMlfResultResponse>(cmdReq.Data);
                        var mLFDetectionResult = JsonConvert.DeserializeObject<MtfMlfResult>(responseDoMlfDetection.MLFDetectionResult);
                        var intensityMeasurementResult = JsonConvert.DeserializeObject<IntensityMeasurementResult>(responseDoMlfDetection.IntensityMeasurementResult);
                        GradientMeasurementResult gradientResult = null;
                        if (responseDoMlfDetection.gradientMeasurementResult != null)
                        {
                            gradientResult = JsonConvert.DeserializeObject<GradientMeasurementResult>(responseDoMlfDetection.gradientMeasurementResult);
                        }
                        visionResult.DetectionType = mLFDetectionResult.resultType;
                        visionResult.PointinArea = mLFDetectionResult.detectionPointInArea;
                        visionResult.IntensityMeasurementResult = intensityMeasurementResult;
                        visionResult.GradientResults = gradientResult;

                        switch ((DetectionTypes)mLFDetectionResult.resultType)
                        {
                            case DetectionTypes.CutOffLine:
                                var cutOffLine = JsonConvert.DeserializeObject<CutofflineResult>(mLFDetectionResult.resultData);
                                visionResult.CutOffLineResults = cutOffLine;
                                visionResult.IsoLineResults = null;
                                visionResult.IsoPercentResults = null;
                                break;
                            case DetectionTypes.IsoPercent:
                                var isoPercent = JsonConvert.DeserializeObject<IsopercentDetectionResult>(mLFDetectionResult.resultData);
                                visionResult.IsoPercentResults = isoPercent;
                                visionResult.CutOffLineResults = null;
                                visionResult.IsoLineResults = null;
                                break;
                            case DetectionTypes.IsoLine:
                                var isoLine = JsonConvert.DeserializeObject<IsolineDetectionResult>(mLFDetectionResult.resultData);
                                visionResult.IsoLineResults = isoLine;
                                visionResult.CutOffLineResults = null;
                                visionResult.IsoPercentResults = null;
                                break;
                        }
                        //fill in result class
                    }
                }
            }

            if (res != null)
            {

                var images = GetImageArrayFromByteArray(res.Image);
                visionResult.Images = images;
                foreach (var image in images)
                {
                    NotifyImageAcquired(image.ImageData);
                }
            }
            if (!string.IsNullOrEmpty(resultCoordRef) && (visionResult.PointinArea != 2))
            {
                RuntimeContext.RaiseException(this, new Exception(Errors.CannotSetANewCoordSystem), ExceptionLevel.JustInfo); ;
            }
            var passed = FillValidationTable(visionResult, ResultConfig, visionResult.PointinArea);
            return outputEnabled ? new MtfMlfDetectionResults() { VisionResultDetails = visionResult, TestPassed = passed } : new MtfMlfDetectionResults() { VisionResultDetails = null, TestPassed = passed };
        }


        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "roisRef", DisplayName = Methods.RoisRefDisplayName, Description = Methods.RoisRefDescription, DataNameExtension = AnnotationSettings.SelectedIDs)]
        [MTFAdditionalParameterInfo(ParameterName = "intensitySettingRef", DisplayName = Methods.IntensitySettingRefDisplayName, Description = Methods.IntensitySettingRefDescription, DataNameExtension = AnnotationSettings.IntensitySettings)]
        [MTFAdditionalParameterInfo(ParameterName = "tableName", DisplayName = Methods.TableNameDisplayName, Description = Methods.TableNameDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "function", DisplayName = Methods.FunctionDisplayName, Description = Methods.FunctionDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "outputEnabled", DisplayName = Methods.OutputEnabledDisplayName, Description = Methods.OutputEnabledDescription)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.IntensitySettings, AnnotationSettings.SelectedIDs }, DisplayName = Methods.GetImageAndMeasIntensityDisplayName, Description = Methods.GetImageAndMeasIntensityDescription)]
        public AbsoluteMeasurementResult GetImageAndMeasAbsoluteIntensity(string imageRef, string roisRef, string intensitySettingRef, string tableName, string function, bool outputEnabled)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            var selectedRois = SelectRoIs(imageRef, roisRef);
            var res = MeasAbsoluteIntensityValuesStep(imageRef, string.Empty, intensitySettingRef, selectedRois);
            var passed = FillValidationTable(res, tableName, function);
            return outputEnabled ? new AbsoluteMeasurementResult() { IntensityDetails = res, TestPassed = passed } : new AbsoluteMeasurementResult() { IntensityDetails = null, TestPassed = passed };
        }

        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "roisRef", DisplayName = Methods.RoisRefDisplayName, Description = Methods.RoisRefDescription, DataNameExtension = AnnotationSettings.SelectedIDs)]
        [MTFAdditionalParameterInfo(ParameterName = "colorSettingRef", DisplayName = Methods.ColorSettingRefDisplayName, Description = Methods.ColorSettingRefDescription, DataNameExtension = AnnotationSettings.ColorSettings)]
        [MTFAdditionalParameterInfo(ParameterName = "tableName", DisplayName = Methods.TableNameDisplayName, Description = Methods.TableNameDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "function", DisplayName = Methods.FunctionDisplayName, Description = Methods.FunctionDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "outputEnabled", DisplayName = Methods.OutputEnabledDisplayName, Description = Methods.OutputEnabledDescription)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.ColorSettings, AnnotationSettings.SelectedIDs }, DisplayName = Methods.GetImageAndMeasColorDisplayName, Description = Methods.GetImageAndMeasColorDescription)]
        public ColorMeasurementResult GetImageandMeasColor(string imageRef, string roisRef, string colorSettingRef, string tableName, string function, bool outputEnabled)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            var selectedRois = SelectRoIs(imageRef, roisRef);
            var res = MeasColorValuesStep(imageRef, colorSettingRef, selectedRois);
            var passed = FillValidationTable(res, tableName, function);
            return outputEnabled ? new ColorMeasurementResult() { ColorDetails = res, TestPassed = passed } : new ColorMeasurementResult() { ColorDetails = null, TestPassed = passed };
        }


        [MTFAdditionalParameterInfo(ParameterName = "imageRef", DisplayName = Methods.ImageRefDisplayName, Description = Methods.ImageRefDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "patternSettingRef", DisplayName = Methods.PatternSettingRefDisplayName, Description = Methods.PatternSettingRefDescription, DataNameExtension = AnnotationSettings.PatternSettings)]
        [MTFAdditionalParameterInfo(ParameterName = "tableName", DisplayName = Methods.TableNameDisplayName, Description = Methods.TableNameDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "function", DisplayName = Methods.FunctionDisplayName, Description = Methods.FunctionDescription)]
        [MTFAdditionalParameterInfo(ParameterName = "outputEnabled", DisplayName = Methods.OutputEnabledDisplayName, Description = Methods.OutputEnabledDescription)]
        [MTFAllowedParameterValue("evaluationMethod", GlobalStrings.And, 0)]
        [MTFAllowedParameterValue("evaluationMethod", GlobalStrings.Or, 1)]
        [MTFMethod(SetupModeAvailable = true, UsedDataNames = new[] { AnnotationSettings.PatternSettings }, DisplayName = Methods.FindPatternDisplayName, Description = Methods.FindPatternDescription)]
        public PatternResult FindPattern(string imageRef, string patternSettingRef, ushort evaluationMethod, string tableName, string function, bool outputEnabled)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            PatternResultDetails res = null;
            var setting = VisionSerializableData.Load<string[]>(AnnotationSettings.PatternSettings, patternSettingRef);
            FindPatternsCMD patternCmd = new FindPatternsCMD
            {
                imageRef = imageRef,
                patternsSettings = setting,
                setupMode = RuntimeContext.IsSetupMode,
                evaluationMethod = evaluationMethod
            };


            var dataToSend = JsonConvert.SerializeObject(patternCmd);
            Command cmd = new Command()
            {
                Text = GlobalStrings.FindPatterns,
                Data = dataToSend
            };
            ElcomImage result;
            try
            {
                //result = _service.CommandImage(new ElcomImage() { JSONData = JsonConvert.SerializeObject(cmd) });
                result = StopableImageCmd(cmd);
            }
            catch (Exception e)
            {

                throw new Exception(Errors.FindPatternCommandFailed, e);
            }

            if (result != null && !string.IsNullOrEmpty(result.JSONData))
            {
                var cmdResponse = Deserialize(result.JSONData);
                //Command cmdResponse = JsonConvert.DeserializeObject<Command>(result.JSONData);
                //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
                //{
                //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText), ExceptionLevel.JustInfo);
                //}

                FindPatternResult patternResults = null;
                if (!string.IsNullOrEmpty(cmdResponse.Data))
                {
                    patternResults = JsonConvert.DeserializeObject<FindPatternResult>(cmdResponse.Data);

                    if (patternResults != null)
                    {
                        if (RuntimeContext.IsSetupMode)
                        {
                            VisionSerializableData.Save(AnnotationSettings.PatternSettings, patternSettingRef, patternResults.patternsSettings);
                        }
                    }

                }
                var images = GetImageArrayFromByteArray(result.Image);
                foreach (var image in images)
                {
                    NotifyImageAcquired(image.ImageData);
                }
                res = new PatternResultDetails()
                {
                    Images = images,
                    Result = patternResults
                };
            }
            var passed = FillValidationTable(res, tableName, function);
            return outputEnabled ? new PatternResult() { PatternDetails = res, TestPassed = passed } : new PatternResult() { PatternDetails = null, TestPassed = passed };
        }


        //[MTFAllowedParameterValue("imageFormat", "No Image", -1)]
        //[MTFAllowedParameterValue("imageFormat", "JPEG", 1)]
        //[MTFAllowedParameterValue("imageFormat", "RAW", 3)]
        //[MTFMethod(DisplayName = "Upload Raw Data Image", Description = "Upload image to Elcom Vision, it can return \nNO DATA (data are stored in elcom and accessible via Image reference), \nJPEG image \nRAW Data")]
        //[MTFAdditionalParameterInfo(ParameterName = "ImageRawData", DisplayName = "Raw Data Image")]
        public MTFImage UploadImage(MTFImage imageRawData, int imageOutputFormat, string imageRef)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            UploadImageCmd prom = new UploadImageCmd()
            {
                imgRefName = imageRef
            };

            var dataToSend = JsonConvert.SerializeObject(prom);
            Command cmd = new Command()
            {
                Text = GlobalStrings.UploadImage,
                Data = dataToSend
            };
            ElcomImage result;
            try
            {
                result = _service.CommandImage(new ElcomImage() { JSONData = JsonConvert.SerializeObject(cmd), Image = imageRawData.ImageData });
            }
            catch (Exception e)
            {

                throw new Exception(Errors.UploadImageFailed, e);
            }

            if (result != null && result.JSONData != string.Empty)
            {
                Deserialize(result.JSONData); //check answer for errors
                //Command cmdResponse = JsonConvert.DeserializeObject<Command>(result.JSONData);
                //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
                //{
                //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText), ExceptionLevel.JustInfo);
                //}
                if (_enableMessageLogging)
                {
                    Log.LogMessage(result.JSONData, GlobalStrings.Vision, true);
                }
                var imageOut = new MTFImage(GetImageCmd(imageRef, imageOutputFormat, false));
                return imageOut;
            }
            else
            {
                throw new Exception(Errors.UploadImageFailed);
            }

        }

        //[MTFMethod(DisplayName = "Start Stream", Description = "This activity will start a stream from Elcom driver to MTF")]
        //[MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = "CameraList")]
        public string StartStream(string virtualCameraName, string imageRef)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            if (!IsVirtualCameraConfigured(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraNotFound(virtualCameraName));
            }
            if (string.IsNullOrEmpty(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraEmpty);
            }
            AcquireImage i = new AcquireImage()
            {
                virtualCameraName = virtualCameraName,
                imgRefName = imageRef
            };

            Command c = new Command()
            {
                Text = GlobalStrings.StartImageStream,
                Data = JsonConvert.SerializeObject(i)
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                string res;
                try
                {
                    res = _service.Command(cmdStr);
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.StartStreamFailed, e);
                }

                return res;
            }
            return null;
        }

        //[MTFMethod(DisplayName = "Stop Stream", Description = "This activity will stop the previously started stream from Elcom driver to MTF")]
        //[MTFAdditionalParameterInfo(ParameterName = "virtualCameraName", ValueListName = "CameraList")]
        public string StopStream(string virtualCameraName)
        {
            ActivityPath = RuntimeContext.ActivityPathIds;
            if (string.IsNullOrEmpty(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraEmpty);
            }
            if (!IsVirtualCameraConfigured(virtualCameraName))
            {
                throw new Exception(Errors.VirtualCameraNotFound(virtualCameraName));
            }

            StopImageStream i = new StopImageStream()
            {
                virtualCameraName = virtualCameraName
            };

            Command c = new Command()
            {
                Text = GlobalStrings.StopImageStream,
                Data = JsonConvert.SerializeObject(i)
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                string res;
                try
                {
                    res = _service.Command(cmdStr);
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.StopStreamFailed, e);
                }

                return res;
            }
            return Errors.NoResultFromStopStream;
        }

        #endregion

        public void NotifyImageAcquired(ElcomImage image)
        {


            if (image != null && image.Image != null && image.Image.Length > 0)
            {
                Task.Run(() =>
                {

                    var img = GetImageArrayFromByteArray(image.Image);
                    if (img != null)
                    {
                        NotifyImageAcquired(img[0].ImageData);
                    }
                });


            }
        }

        public Command Deserialize(string json)
        {
            var cmd = JsonConvert.DeserializeObject<Command>(json);
            if (cmd != null)
            {
                //var result = JsonConvert.DeserializeAnonymousType(json, new { Text  = "", ErrorText = "",Data="" });

                switch (cmd.Text)
                {
                    case "Warn":
                        if (cmd.Data.Contains("Undelivered message task") || cmd.Data.Contains("AreaManager_Module"))
                        {
                            visionErrorCommand = true;
                        }
                        RuntimeContext.RaiseException(this, new Exception(string.Format("Vision Warning: {0}", cmd.Data)), ExceptionLevel.JustInfo);
                        return null;
                    //break;
                    case "Error":
                        if (cmd.Data.Contains("Undelivered message task") || cmd.Data.Contains("AreaManager_Module"))
                        {
                            visionErrorCommand = true;
                        }
                        RuntimeContext.RaiseException(this, new Exception(string.Format("Vision Warning: {0}", cmd.Data)), ExceptionLevel.CriticalAsynchronousException);
                        return null;
                    case "Cancel":
                        visionSetupCanceled = true;
                        visionErrorCommand = true;

                        RuntimeContext.RaiseException(this, new Exception("Vision Setup Canceled"), ExceptionLevel.JustInfo);
                        return null;
                    default:
                        if (cmd.Code == 0 && string.IsNullOrEmpty(cmd.ErrorText))
                        {
                            return cmd;
                        }
                        RuntimeContext.RaiseException(this, new Exception(string.Format("Vision Error {0}: {1}", cmd.Code, cmd.ErrorText)), ExceptionLevel.JustInfo);
                        if (_enableMessageLogging)
                        {
                            Log.LogMessage(json, GlobalStrings.Vision, true);
                        }
                        break;
                }

            }
            return null;
        }


        object asyncResponseLock = new object();
        public void NotifyAsyncResponse(string cmdString)
        {
            lock (asyncResponseLock)
            {
                var res = Deserialize(cmdString); //
                if (res != null)
                {
                    if (res.Data == "Timeout")
                    {
                        visionTimeout = true;
                    }
                    asyncCmdData = res.Data;
                    asyncResponseCmdReceived = true;
                }
            }
        }

        public void NotifyAsyncImageResponse(ElcomImage image)
        {
            lock (asyncResponseLock)
            {
                asyncImage = image;
                asyncResponseImageReceived = true;
            }
        }

        public void Dispose()
        {
            if (forceReinitVision)
            {

                if (_service != null)
                {

                    try
                    {
                        _service.Command(JsonConvert.SerializeObject(new Command { Text = "StopEVS" }));
                        System.Threading.Thread.Sleep(200);
                        _service.ShutDownElcomVision(15000);
                    }
                    catch (Exception e)
                    {

                        Log.LogMessage(new Exception(Errors.ShutDownElcomVisionFailed, e));
                    }
                    try
                    {
                        ShutDownWcfApp(2000);
                    }
                    catch (Exception e)
                    {

                        Log.LogMessage(new Exception(Errors.ShutDownElcomHostFailed, e));
                    }
                    StopElcom();
                }
            }
            else
            {
                //if (_service != null)
                //{
                //    _service.Command(JsonConvert.SerializeObject(new Command {Text = "StopEVS"}));
                //}

                if (_myChannelFactory != null)
                {
                    _myChannelFactory.Abort();
                }
            }

        }

        #region private

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDlgItem(int hwnd, int childID);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hwnd, int msg, int wparam, int lparam);

        const UInt32 WM_CLOSE = 0x0010;
        private const int WM_COMMAND = 0x111;
        private const int dlgCANCEL = 0x2;

        private void StopElcom()
        {

            foreach (Process myProc in Process.GetProcesses())
            {
                if (myProc.ProcessName == GlobalStrings.EVSName)
                {
                    IntPtr window1 = FindWindowByCaption(IntPtr.Zero, myProc.ProcessName);
                    myProc.CloseMainWindow();
                    
                    int i = 0;
                    while (!myProc.HasExited && !stop&&i<100)
                    {
                        try
                        {

                            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, myProc.ProcessName);
                            if (windowPtr != IntPtr.Zero && window1 != windowPtr)
                            {
                                SendMessage(windowPtr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                                PostMessage(windowPtr, WM_COMMAND, dlgCANCEL,0);
                            }                            
                            RuntimeContext.ProgressNotification(i);
                            System.Threading.Thread.Sleep(200);
                            
                        }
                        catch
                        {
                            //try kill Elcom Host
                        }
                        i++;
                    }
                    if (!myProc.HasExited && i == 100)
                    {
                        RuntimeContext.RaiseException(this, new Exception("Try close EVS with kill"), ExceptionLevel.JustInfo);
                        myProc.Kill();
                    }
                }

                if (myProc.ProcessName == GlobalStrings.ElcomHostName)
                {
                    IntPtr window1 = FindWindowByCaption(IntPtr.Zero, "Host For WCF Server");
                    SendMessage(window1, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                    //myProc.CloseMainWindow();

                    int i = 0;
                    while (!myProc.HasExited && !stop && i < 100)
                    {
                        try
                        {

                            IntPtr windowPtr = FindWindowByCaption(IntPtr.Zero, "Host For WCF Server");
                            if (windowPtr != IntPtr.Zero && window1 != windowPtr)
                            {
                                
                                PostMessage(windowPtr, WM_COMMAND, dlgCANCEL, 0);
                            }
                            RuntimeContext.ProgressNotification(i);
                            System.Threading.Thread.Sleep(200);

                        }
                        catch
                        {
                            //try kill Elcom Host
                        }
                        i++;
                    }
                    if (!myProc.HasExited && i == 100)
                    {
                        RuntimeContext.RaiseException(this, new Exception("Try close Elcom Host For WCF Server with kill"), ExceptionLevel.JustInfo);
                        myProc.Kill();
                    }
                }
            }
        }

        private bool IsVisionRunnable()
        {

            var savedCameras = RuntimeContext.LoadData<CameraDict>(GlobalStrings.RuntimeContextCameraCfg);
            if (savedCameras == null)
            {
                return false;
            }
            bool cameraConfigOk = true;
            foreach (var item in cameraConfigs)
            {
                if (!savedCameras.CameraConfigs.ContainsKey(item.VirtualCameraName))
                {
                    cameraConfigOk = false;
                    break;
                }
            }
            if (cameraConfigOk)
            {
                foreach (var item in savedCameras.CameraConfigs) //Find removed cameras from component config
                {
                    //var item = _cameraConfigDict.CameraConfigs.ElementAt(i);
                    bool missingItem = true;
                    foreach (var camera in cameraConfigs)
                    {
                        if (item.Value.VirtualCameraName == camera.VirtualCameraName)
                        {
                            missingItem = false;
                        }
                    }
                    if (missingItem)
                    {
                        cameraConfigOk = false;
                        break;
                    }
                }
            }

            bool hostRunning = false;
            bool eVSRunning = false;
            foreach (Process myProc in Process.GetProcesses())
            {
                if (myProc.ProcessName == GlobalStrings.ElcomHostName)
                {
                    hostRunning = true;
                }
                if (myProc.ProcessName == GlobalStrings.EVSName)
                {
                    eVSRunning = true;
                }
            }

            return (hostRunning && eVSRunning && cameraConfigOk);
        }

        private int imageCount;
        object imageCountLock = new object();
        private void NotifyImageAcquired(byte[] image)
        {
            if (image != null)
            {
                RuntimeContext.ImageNotification(image, ActivityPath);
                if (_enableMessageLogging)
                {
                    if (imageCountLock == null)
                    {
                        imageCountLock = new object();
                    }
                    lock (imageCountLock)
                    {
                        Log.LogMessage(string.Format("Image Notification send {0}", imageCount++));
                    }
                }

            }
        }

        private MTFImage[] GetImageArrayFromByteArray(byte[] array)
        {
            if (array != null)
            {
                try
                {
                    ushort nrBytesImageCount = 3;
                    ushort nrBytesImageLength = 10;
                    byte[] chars = new byte[nrBytesImageCount];
                    Buffer.BlockCopy(array, 0, chars, 0, nrBytesImageCount);
                    int nrImages;
                    int.TryParse(System.Text.Encoding.UTF8.GetString(chars).Trim(), out nrImages);
                    int[] imagesLength = new int[nrImages];
                    chars = new byte[nrBytesImageLength];
                    MTFImage[] images = new MTFImage[nrImages];
                    for (int i = 0; i < nrImages; i++)
                    {
                        Buffer.BlockCopy(array, (nrBytesImageCount + i * nrBytesImageLength), chars, 0,
                            nrBytesImageLength);

                        int.TryParse(System.Text.Encoding.UTF8.GetString(chars).Trim(), out imagesLength[i]);

                    }
                    int offset = (nrBytesImageCount + nrImages * nrBytesImageLength);
                    bool clear = false;
                    for (int i = 0; i < nrImages; i++)
                    {
                        if (imagesLength[i] > 10485760)
                        {
                            GC.Collect();
                            clear = true;
                        }
                        images[i] = new MTFImage { ImageData = new byte[imagesLength[i]] };
                        Buffer.BlockCopy(array, offset, images[i].ImageData, 0, imagesLength[i]);
                        offset = offset + imagesLength[i];
                    }
                    if (clear)
                    {
                        GC.Collect();
                    }
                    return images;
                }
                catch (Exception e)
                {
                    RuntimeContext.RaiseException(this, e, ExceptionLevel.JustInfo);
                    return null;
                }
            }

            RuntimeContext.RaiseException(this, new Exception(Errors.ImageElcomIsNull), ExceptionLevel.JustInfo);
            return null;

        }

        private IDS LoadRoIs(string roisRef)
        {

            try
            {
                var response = RuntimeContext.LoadData<GlobalRoiDictionary>(GlobalStrings.RuntimeContextGlobalAreas);
                if (response != null)
                {
                    _globalRoiDictionary = response;
                    if (_globalRoiDictionary.roiDict != null && _globalRoiDictionary.roiDict.Count > 0)
                    {
                        if (_enableMessageLogging)
                        {
                            Log.LogMessage(GlobalStrings.GlobalAreaDictLoaded, GlobalStrings.Vision, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {

                RuntimeContext.RaiseException(this, new Exception(Errors.MergeError(Errors.LoadGlobalAreaError, e.Message)), ExceptionLevel.JustInfo);

            }
            var ids = VisionSerializableData.Load<IDS>(AnnotationSettings.SelectedIDs, roisRef);
            return ids;
        }
        private AbsoluteMeasurementResultDetails MeasAbsoluteIntensityValuesStep(string imgRefName, string coordSysName, string intensitySettingRef, Areas rois)
        {
            if (rois == null || !rois.areas.Any())
            {
                RuntimeContext.RaiseException(this, new ArgumentNullException(Errors.RoisNull), ExceptionLevel.JustInfo);
                return null;
            }

            var setting = VisionSerializableData.Load<string[]>(AnnotationSettings.IntensitySettings, intensitySettingRef);

            MeasAbsoluteIntensityValues prom = new MeasAbsoluteIntensityValues
            {
                imgRefName = imgRefName,
                coordSysName = coordSysName,
                intensityMeasSettings = setting,
                areas = rois.areas,
                setupMode = RuntimeContext.IsSetupMode
            };

            var dataToSend = JsonConvert.SerializeObject(prom);
            Command cmd = new Command()
            {
                Text = GlobalStrings.MeasAbsoluteIntensityValues,
                Data = dataToSend
            };
            ElcomImage result;
            try
            {
                result = StopableImageCmd(cmd);
            }
            catch (Exception e)
            {

                throw new Exception(Errors.MeasIntensityFailed, e);
            }

            if (result != null && result.JSONData != string.Empty)
            {
                var cmdResponse = Deserialize(result.JSONData);
                //Command cmdResponse = JsonConvert.DeserializeObject<Command>(result.JSONData);
                //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
                //{
                //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText),
                //        ExceptionLevel.JustInfo);
                //}
                MeasAbsoluteIntensityValuesResponse intensityResults = null;
                if (!string.IsNullOrEmpty(cmdResponse.Data))
                {
                    intensityResults = JsonConvert.DeserializeObject<MeasAbsoluteIntensityValuesResponse>(cmdResponse.Data);

                    if (intensityResults != null)
                    {
                        if (RuntimeContext.IsSetupMode)
                        {
                            setting = new string[intensityResults.intensityMeasSettings.Length];
                            for (int i = 0; i < intensityResults.intensityMeasSettings.Length; i++)
                            {
                                setting[i] = JsonConvert.SerializeObject(intensityResults.intensityMeasSettings[i]);
                            }
                            VisionSerializableData.Save(AnnotationSettings.IntensitySettings, intensitySettingRef, setting);
                        }
                    }
                }

                var images = GetImageArrayFromByteArray(result.Image);
                foreach (var image in images)
                {
                    NotifyImageAcquired(image.ImageData);
                }
                return new AbsoluteMeasurementResultDetails
                {
                    Images = images,
                    Result = intensityResults
                };
            }
            return null;
        }

        private ColorMeasurementResultDetails MeasColorValuesStep(string imgRefName, string colorSettingRef, Areas rois)
        {
            if (rois == null || !rois.areas.Any())
            {
                RuntimeContext.RaiseException(this, new ArgumentNullException(Errors.RoisNull), ExceptionLevel.JustInfo);
                return null;
            }
            var setting = VisionSerializableData.Load<string[]>(AnnotationSettings.ColorSettings, colorSettingRef);

            MeasAbsoluteColorValues prom = new MeasAbsoluteColorValues
            {
                imageRef = imgRefName,
                colorMeasSettings = setting,
                areas = rois.areas,
                setupMode = RuntimeContext.IsSetupMode
            };

            var dataToSend = JsonConvert.SerializeObject(prom);
            Command cmd = new Command()
            {
                Text = GlobalStrings.MeasAbsoluteColor,
                Data = dataToSend
            };
            ElcomImage result;
            try
            {
                result = StopableImageCmd(cmd);
            }
            catch (Exception e)
            {

                throw new Exception(Errors.MeasColorFailed, e);
            }

            if (result != null && !string.IsNullOrEmpty(result.JSONData))
            {//colorSettings
                var cmdResponse = Deserialize(result.JSONData);
                //Command cmdResponse = JsonConvert.DeserializeObject<Command>(result.JSONData);
                //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
                //{
                //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText), ExceptionLevel.JustInfo);
                //}
                MeasColorValuesResponse colorResults = null;
                if (!string.IsNullOrEmpty(cmdResponse.Data))
                {
                    colorResults = JsonConvert.DeserializeObject<MeasColorValuesResponse>(cmdResponse.Data);
                    if (colorResults != null)
                    {

                        if (RuntimeContext.IsSetupMode)
                        {
                            VisionSerializableData.Save(AnnotationSettings.ColorSettings, colorSettingRef, colorResults.colorMeasSettings);
                        }
                        colorResults.colorResult = new MeasColorResults[colorResults.colorMeasSettings.Length];
                        for (int i = 0; i < colorResults.colorMeasSettings.Length; i++)
                        {
                            colorResults.colorResult[i] = JsonConvert.DeserializeObject<MeasColorResults>(colorResults.colorMeasSettings[i]);
                        }
                    }

                }


                var images = GetImageArrayFromByteArray(result.Image);
                foreach (var image in images)
                {
                    NotifyImageAcquired(image.ImageData);
                }

                return new ColorMeasurementResultDetails()
                {
                    Images = images,
                    Result = colorResults
                };
            }
            return null;
        }

        private void AcquireImageOnly(string virtualCameraName, string imgRefName)
        {
            AcquireImage i = new AcquireImage()
            {
                virtualCameraName = virtualCameraName,
                imgRefName = imgRefName,
            };

            Command c = new Command()
            {
                Text = GlobalStrings.AcquireImage,
                Data = JsonConvert.SerializeObject(i)
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                string cfgResult;
                try
                {
                    cfgResult = _service.Command(cmdStr);
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.AcquireImageFailed, e);
                }

                Deserialize(cfgResult);//check answer for errors
                //Command cmdResponse = JsonConvert.DeserializeObject<Command>(cfgResult);
                //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
                //{
                //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText), ExceptionLevel.JustInfo);
                //}
            }

        }

        private MtfElcomImage AcquireAndReturnImage(string virtualCameraName, string imgRefName, int imageFormat, bool overlay)
        {
            AcquireAndReturnImage i = new AcquireAndReturnImage()
            {
                virtualCameraName = virtualCameraName,
                imgRefName = imgRefName,
                imageFormat = imageFormat,
                overlay = overlay
            };

            Command c = new Command()
            {
                Text = GlobalStrings.AcquireAndReturnImage,
                Data = JsonConvert.SerializeObject(i)
            };
            Dictionary<string, string> dict = null;
            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                ElcomImage res;
                try
                {
                    res = _service.CommandImage(new ElcomImage() { JSONData = cmdStr });
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.AcquireAndReturnImageFailed, e);
                }

                if (res != null)
                {
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(GlobalStrings.ImageMetaData + res.JSONData, GlobalStrings.Vision, true);
                    }
                    if (imageFormat == 3)
                    {
                        if (res.JSONData != null)
                        {
                            var cmdReq = Deserialize(res.JSONData);
                            //Command cmdReq = JsonConvert.DeserializeObject<Command>(res.JSONData);
                            var setting = JsonConvert.DeserializeObject<string[][]>(cmdReq.Data);
                            if (cmdReq.ErrorText != string.Empty)
                            {
                                throw new InvalidOperationException(Errors.ErrorReadingImage(cmdReq.ErrorText));
                            }
                            dict = new Dictionary<string, string>();
                            foreach (string[] t in setting)
                            {
                                dict.Add(t[0], t[1]);
                            }
                        }
                    }

                    if (imageFormat != 3)
                    {
                        var images = GetImageArrayFromByteArray(res.Image);
                        if (images == null || images.Length == 0)
                        {
                            throw new Exception(Errors.VisionResultDidNotContainImage);
                        }
                        if (images.Length > 1)
                        {
                            RuntimeContext.RaiseException(this, new Exception(Errors.AcquireImageReturnedMoreThanOneImage), ExceptionLevel.JustInfo);
                        }

                        return new MtfElcomImage() { ImageData = images[0].ImageData, Config = dict, IsLarge = false };
                    }

                    return new MtfElcomImage() { RawData = res.Image, Config = dict, IsLarge = (res.Image.Length > 10485760) };// Before sending large byte array 10MB GC.Collect will be called
                }
                if (_enableMessageLogging)
                {
                    Log.LogMessage(Errors.NoValidImage, GlobalStrings.Vision, true);
                }

            }
            return null;
        }

        private byte[] GetImageCmd(string imgRefName, int imageFormat, bool overlay)
        {
            GetImage i = new GetImage()
            {
                imgRefName = imgRefName,
                imageFormat = imageFormat,
                overlay = overlay
            };

            Command c = new Command()
            {
                Text = GlobalStrings.GetImage,
                Data = JsonConvert.SerializeObject(i)
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                ElcomImage res;
                try
                {
                    res = _service.CommandImage(new ElcomImage() { JSONData = cmdStr });
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.GetImageFailed, e);
                }

                if (res != null)
                {
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(GlobalStrings.ImageMetaData + res.JSONData, GlobalStrings.Vision, true);
                    }
                    if (res.JSONData != null)
                    {
                        Deserialize(res.JSONData);//check and report errors to mtf
                        //Command cmdReq = JsonConvert.DeserializeObject<Command>(res.JSONData);
                        //if (cmdReq.ErrorText != string.Empty)
                        //{
                        //    throw new InvalidOperationException(Errors.ErrorReadingImage(cmdReq.ErrorText));
                        //}
                    }
                    return res.Image;
                }
                if (_enableMessageLogging)
                {
                    Log.LogMessage(Errors.NoValidImage, GlobalStrings.Vision, true);
                }
            }
            return null;
        }

        private void CreateVirtualCameras(List<CameraConfigParams> config)
        {
            VirtualCamera[] i = new VirtualCamera[config.Count];
            int j = 0;
            foreach (var item in config)
            {
                i[j] = new VirtualCamera()
                {
                    virtualCameraName = item.VirtualCameraName,
                    identifier = item.CameraSN,
                    isSimulation = item.IsSimulated,
                    virtualCameraParams = item.IsSimulated ? item.SimulatedVirtualCameraParams : item.VirtualCameraParams,
                };
                j++;
            }

            var d = JsonConvert.SerializeObject(i);

            Command c = new Command()
            {
                Text = GlobalStrings.CreateVirtualCameras,
                Data = d,
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                try
                {
                    var res = _service.Command(cmdStr);
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(res, GlobalStrings.Vision, true);
                    }
                }
                catch (Exception e)
                {
                    RuntimeContext.RaiseException(this, new Exception(Errors.CreateVirtualCamFailed, e), ExceptionLevel.CriticalAsynchronousException);
                }
                return;
            }
            if (_enableMessageLogging)
            {
                Log.LogMessage(Errors.CreateVirtualCamFailed, GlobalStrings.Vision, true);
            }

        }

        private void CreateVirtualCamera(CameraConfigParams config)
        {
            CreateVirtualCamera(config.CameraSN, config.VirtualCameraName, config.IsSimulated, config.IsSimulated ? config.SimulatedVirtualCameraParams : config.VirtualCameraParams);
        }

        private void CreateVirtualCamera(string identifier, string virtualCameraName, bool isSimulation, ushort[] virtualCameraParams)
        {
            //if (virtualCameraParams == null)
            //{
            //    throw new Exception(Errors.VirtualCameraParamsNull);
            //}
            VirtualCamera i = new VirtualCamera()
                {
                    virtualCameraName = virtualCameraName,
                    identifier = identifier,
                    isSimulation = isSimulation,
                    virtualCameraParams = virtualCameraParams,

                };
            var d = JsonConvert.SerializeObject(i);

            Command c = new Command()
            {
                Text = GlobalStrings.CreateVirtualCamera,
                Data = d,
            };

            string cmdStr = JsonConvert.SerializeObject(c);
            if (cmdStr != string.Empty)
            {
                try
                {
                    var res = _service.Command(cmdStr);
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(res, GlobalStrings.Vision, true);
                    }
                }
                catch (Exception e)
                {
                    RuntimeContext.RaiseException(this, new Exception(Errors.CreateVirtualCamFailed, e), ExceptionLevel.CriticalAsynchronousException);
                }
                return;
            }
            if (_enableMessageLogging)
            {
                Log.LogMessage(Errors.CreateVirtualCamFailed, GlobalStrings.Vision, true);
            }
        }
        private string[] GetAllPhysicalCamerasStep(bool onlyEthernet)
        {
            GetAllPhysicalCameras ga = new GetAllPhysicalCameras() { OnlyEthernetCameras = onlyEthernet };
            var dataToSend = JsonConvert.SerializeObject(ga);
            Command cmd = new Command()
            {
                Text = GlobalStrings.GetAllPhysicalCameras,
                Data = dataToSend
            };
            string result;
            try
            {
                result = _service.Command(JsonConvert.SerializeObject(cmd));
            }
            catch (Exception e)
            {

                throw new Exception(Errors.GetAllPhysicalCamerasFailed, e);
            }
            var cmdResponse = Deserialize(result);
            //Command cmdResponse = JsonConvert.DeserializeObject<Command>(result);
            //if (!string.IsNullOrEmpty(cmdResponse.ErrorText))
            //{
            //    RuntimeContext.RaiseException(this, new Exception(cmdResponse.ErrorText), ExceptionLevel.JustInfo);
            //}

            string[] kamery = JsonConvert.DeserializeObject<string[]>(cmdResponse.Data);

            return kamery;
        }

        private Areas LoadSelectedIds(string roisRef, IDS ids)
        {
            try
            {
                var selectedIds = VisionSerializableData.Load<IDS>(AnnotationSettings.SelectedIDs, roisRef);
                return GetSelectedAreas(selectedIds.IDs);
            }
            catch
            {
                throw new Exception(Errors.NoAreaFound);
            }
        }

        private Areas GetRoIs(string roisRef, string coordSysName, string imgRefName)
        {
            var ids = LoadRoIs(roisRef);
            if (IsFullTeachDisabled)
            {
                return LoadSelectedIds(roisRef, ids);
            }
            SelectSpecificAreas areas = new SelectSpecificAreas
            {
                areas = _globalRoiDictionary.roiDict != null ? _globalRoiDictionary.roiDict.Values.ToArray() : null,
                imgRefName = imgRefName,
                coordSysName = coordSysName,
                selectedAreasIDs = ids != null ? ids.IDs : null
            };

            var d = JsonConvert.SerializeObject(areas);
            Command cmdRequest = new Command()
            {
                Text = GlobalStrings.SelectSpecificAreas,
                Data = d,
                ErrorText = ""
            };

            string cmdStr = JsonConvert.SerializeObject(cmdRequest);
            if (cmdStr != string.Empty)
            {
                ElcomImage res;
                try
                {
                    res = StopableImageCmd(cmdRequest);// _service.CommandImage(new ElcomImage() { JSONData = cmdStr });
                    if (visionSetupCanceled)
                    {
                        return LoadSelectedIds(roisRef, ids);
                    }
                }
                catch (Exception e)
                {

                    throw new Exception(Errors.SelectSpecificAreasFailed, e);
                }

                if (res != null && res.JSONData != string.Empty)
                {
                    var cmd = Deserialize(res.JSONData);
                    //Command cmd = JsonConvert.DeserializeObject<Command>(res.JSONData);
                    MTFEditAreas editAreaResult = null;
                    if (cmd != null)
                    {
                        editAreaResult = JsonConvert.DeserializeObject<MTFEditAreas>(cmd.Data);
                    }
                    if (editAreaResult == null)
                    {
                        RuntimeContext.RaiseException(this, new NullReferenceException(Errors.DeserializeResponseSelectSpecificAreasFailed), ExceptionLevel.JustInfo);
                        return null;
                    }
                    Areas selectedAreas = null;


                    if (editAreaResult.areas.Length != editAreaResult.IDs.Length)
                    {
                        RuntimeContext.RaiseException(this, new Exception(Errors.AreasAndIDsCountMismatch), ExceptionLevel.JustInfo);
                    }
                    if (editAreaResult.areas.Length > 0 && editAreaResult.IDs.Length > 0)
                    {
                        _globalRoiDictionary.roiDict = new Dictionary<string, string>();
                        for (int i = 0; i < editAreaResult.areas.Length; i++)
                        {

                            _globalRoiDictionary.roiDict.Add(editAreaResult.IDs[i], editAreaResult.areas[i]);
                        }

                        RuntimeContext.SaveData(GlobalStrings.RuntimeContextGlobalAreas, new GlobalRoiDictionary() { roiDict = _globalRoiDictionary.roiDict });
                        VisionSerializableData.Save(AnnotationSettings.SelectedIDs, roisRef, new IDS() { IDs = editAreaResult.selectedAreasIDs });

                        selectedAreas = GetSelectedAreas(editAreaResult.selectedAreasIDs);
                        if (_enableMessageLogging)
                        {
                            Log.LogMessage(GlobalStrings.GlobalAreasAndSelectedIDsSaved, GlobalStrings.Vision, true);
                        }
                    }
                    RuntimeContext.ImageNotification(res.Image);
                    return selectedAreas;
                }
            }
            return null;
        }

        private Areas GetSelectedAreas(string[] ids)
        {
            var selectedAreas = new Areas { areas = new string[ids.Length] };
            for (int i = 0; i < ids.Length; i++)
            {
                if (_globalRoiDictionary.roiDict.ContainsKey(ids[i]))
                {
                    selectedAreas.areas[i] = _globalRoiDictionary.roiDict[ids[i]];
                }
            }
            return selectedAreas;
        }

        private VirtualCameraConfiguration[] GetVirtualCameraConfigurations(CameraDict cameraConfigDict)
        {
            VirtualCameraConfiguration[] config = new VirtualCameraConfiguration[cameraConfigDict.CameraConfigs.Count];
            int i = 0;
            foreach (var item in cameraConfigDict.CameraConfigs)
            {
                if (config[i] == null)
                {
                    config[i] = new VirtualCameraConfiguration();
                }
                config[i].virtualCameraName = item.Value.VirtualCameraName;
                if (item.Value.IsSimulated)
                {
                    config[i].virtualCameraParams = item.Value.SimulatedVirtualCameraParams;
                }
                else
                {
                    config[i].virtualCameraParams = item.Value.VirtualCameraParams;
                }
                i++;
            }
            return config;
        }

        private void ConfigureCameras(bool send, bool setup)
        {
            try
            {

                VirtualCameraConfigurations vcs;
                if (_cameraConfigDict != null && _cameraConfigDict.CameraConfigs != null)
                {
                    vcs = new VirtualCameraConfigurations()
                                                     {
                                                         setupMode = setup,
                                                         virtualCameraConfigurations = GetVirtualCameraConfigurations(_cameraConfigDict)// _vcCamConfig.virtualCameraConfigurations
                                                     };
                }
                else
                {
                    vcs = new VirtualCameraConfigurations()
                                                      {
                                                          setupMode = setup,
                                                          virtualCameraConfigurations = new[] { new VirtualCameraConfiguration()
                                                          {
                                                              virtualCameraParams = null,//new ushort[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                        virtualCameraName = GlobalStrings.VirtualCameraName}  }
                                                      };
                }

                var d = JsonConvert.SerializeObject(vcs);

                Command c = new Command()
                {
                    Text = GlobalStrings.ConfigureVirtualCameras,
                    Data = d,
                    ErrorText = ""
                };

                string cmdStr = JsonConvert.SerializeObject(c);
                if (cmdStr != string.Empty)
                {
                    string data;

                    try
                    {
                        data = StopableCmd(cmdStr);
                    }
                    catch (Exception e)
                    {

                        throw new Exception(Errors.ConfigureVirtualCamerasFailed, e);
                    }



                    VirtualCameraConfigurations vcsret = null;
                    if (data != null)
                    {
                        vcsret = JsonConvert.DeserializeObject<VirtualCameraConfigurations>(data);
                    }
                    else
                    {
                        if (_enableMessageLogging)
                        {
                            Log.LogMessage(Errors.CameraConfigEmpty, GlobalStrings.Vision, true);
                        }
                    }
                    if (RuntimeContext.IsSetupMode)
                    {
                        #region new data save
                        if (_cameraConfigDict == null || _cameraConfigDict.CameraConfigs == null)
                        {
                            throw new Exception(Errors.CameraConfigWasNotCreatedInInit);

                        }

                        if (vcsret != null && vcsret.virtualCameraConfigurations != null)
                            foreach (var camdata in vcsret.virtualCameraConfigurations)
                            {
                                if (_cameraConfigDict.CameraConfigs.ContainsKey(camdata.virtualCameraName))
                                {
                                    var camConfig = _cameraConfigDict.CameraConfigs[camdata.virtualCameraName];
                                    if (camConfig.IsSimulated)
                                    {
                                        camConfig.SimulatedVirtualCameraParams = camdata.virtualCameraParams;
                                    }
                                    else
                                    {
                                        camConfig.VirtualCameraParams = camdata.virtualCameraParams;
                                    }
                                    _cameraConfigDict.CameraConfigs[camdata.virtualCameraName] = camConfig;
                                }
                                else
                                {
                                    throw new Exception(Errors.CameraConfigDoesNotCorrespontToComponentConfig);
                                }
                            }
                        if (send)
                        {
                            RuntimeContext.SaveData(GlobalStrings.RuntimeContextCameraCfg, _cameraConfigDict);
                            //try
                            //{
                            //    using (StreamWriter file = File.CreateText(Directory.GetParent(Path.GetDirectoryName(_WCFHostExe)).ToString() + @"\VisionLauncher\camcfg.cfg"))
                            //    {
                            //        JsonSerializer serializer = new JsonSerializer();
                            //        serializer.Serialize(file, _cameraConfigDict);
                            //    }
                            //}
                            //catch
                            //{
                            //    if (RuntimeContext != null)
                            //    {
                            //        RuntimeContext.RaiseException(this, new Exception("Data for Vision Loader were not saved"), ExceptionLevel.JustInfo);
                            //    }
                            //}

                            if (_enableMessageLogging)
                            {
                                Log.LogMessage(GlobalStrings.CameraConfigSaved, GlobalStrings.Vision, true);
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        if (_enableMessageLogging)
                        {
                            Log.LogMessage(Errors.CameraConfigNotInSetupMode, GlobalStrings.Vision, true);
                        }
                    }


                }
                else
                {
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(Errors.CameraConfigCommandEmpty, GlobalStrings.Vision, true);
                    }
                }

            }
            catch (Exception ex)
            {
                RuntimeContext.RaiseException(this, new Exception(ex.Message + (ex.InnerException != null ? ex.InnerException.Message : "")), ExceptionLevel.JustInfo);
            }
        }
        private void ConnectElcom(int timeoutMs, bool visionIsRunning)
        {
            if (!visionIsRunning)
            {
                if (!string.IsNullOrEmpty(_WCFHostExe))
                {
                    RuntimeContext.ProgressNotification(1, InitStrings.StartingEVS);
                    if (stop)
                    {
                        RuntimeContext.ProgressNotification(100, InitStrings.Stop);
                        return;
                    }
                    //var visionPath = Directory.GetParent(Directory.GetParent(_WCFHostExe).ToString());
                    //var eVSPath = Path.Combine(visionPath.ToString(), @"EVS\builds\EVS.exe");
                    //Process.Start(eVSPath);

                    _wcfApp = Process.Start(_WCFHostExe);



                    if (timeoutMs <= 0 && !stop)
                    {
                        timeoutMs = 15000;
                    }
                    if (_wcfApp != null)
                    {
                        bool repeat = true;
                        var sw = new Stopwatch();
                        sw.Restart();
                        while (repeat)
                        {
                            if (stop)
                            {
                                RuntimeContext.ProgressNotification(100, InitStrings.Stop);
                                return;
                            }

                            if (sw.ElapsedMilliseconds > timeoutMs)
                            {
                                throw new TimeoutException(Errors.ElcomHostTimeOut);
                            }
                            repeat = !_wcfApp.WaitForInputIdle(100);
                        }
                    }
                    else
                    {
                        throw new Exception(Errors.ElcomHostNotStarted);
                    }
                    RuntimeContext.ProgressNotification(10, InitStrings.ConnectingToElcomWCF);
                }
                else
                {
                    throw new Exception(Errors.WCFHostPAthEmpty);
                }
            }

            try
            {
                if (stop)
                {
                    RuntimeContext.ProgressNotification(100, InitStrings.Stop);
                    return;
                }
                var myBinding = new NetTcpBinding
                    {
                        ReceiveTimeout = new TimeSpan(0, 20, 0),
                        SendTimeout = new TimeSpan(0, 20, 0),
                        MaxReceivedMessageSize = int.MaxValue,
                        MaxBufferSize = int.MaxValue,
                        MaxBufferPoolSize = int.MaxValue
                    };
                var myEndpoint = new EndpointAddress(_serverUri);
                _myChannelFactory = new DuplexChannelFactory<IElcomUniService>(typeof(Vision), myBinding, myEndpoint);

                _service = _myChannelFactory.CreateChannel(new InstanceContext(this));

                if (_service != null)
                {
                    RuntimeContext.ProgressNotification(20);
                    if (stop)
                    {
                        RuntimeContext.ProgressNotification(100, InitStrings.Stop);
                        return;
                    }
                    string res = _service.InitElcomVision(35000);
                    if (_enableMessageLogging)
                    {
                        Log.LogMessage(GlobalStrings.ElcomVisionStarted + res, GlobalStrings.Vision, true);
                    }
                }
                else
                {
                    throw new NullReferenceException(Errors.HostChannelWasNotCreated);
                }
            }
            catch (Exception e)
            {
                throw new Exception(Errors.CannotConnectToEVS, e);
            }
        }
        private Areas SelectRoIs(string imageRef, string roisRef)
        {
            if (!string.IsNullOrEmpty(roisRef) && !string.IsNullOrEmpty(imageRef))
            {
                //var imgRefName = string.Format("{0}:{1}", imageRef, variant);
                return GetRoIs(roisRef, string.Empty, imageRef);
            }
            return null;

        }
        private void ShutDownWcfApp(int timeout)
        {
            try
            {
                if (_myChannelFactory != null)
                {
                    if (_myChannelFactory.State == CommunicationState.Opened)
                    {
                        _myChannelFactory.Abort();
                    }
                }
                if (_wcfApp != null)
                {
                    if (!_wcfApp.HasExited)
                    {
                        // Try to send a close message to the main window.
                        if (!_wcfApp.CloseMainWindow())
                        {
                            // Close message did not get sent - Kill process.
                            _wcfApp.Kill();
                            _wcfApp.Close();
                            _wcfApp = null;
                        }
                        else
                        {
                            // Close message sent successfully; wait for X seconds
                            // for termination confirmation before resorting to Kill.
                            if (!_wcfApp.WaitForExit(timeout))
                            {
                                _wcfApp.Kill();
                                _wcfApp.Close();
                                _wcfApp = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                RuntimeContext.RaiseException(this, ex, ExceptionLevel.JustInfo);
            }
        }

        private bool IsVirtualCameraConfigured(string virtualCameraName)
        {
            foreach (var virtualCameraConfig in _cameraConfigDict.CameraConfigs.Values)
            {
                if (virtualCameraConfig.VirtualCameraName == virtualCameraName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool FillValidationTable(ColorMeasurementResultDetails result, string tableName, string rowPrefix)
        {
            bool passed = true;
            if ((int)_reportConfig[AnnotationSettings.ColorReportSetting] > 0 && string.IsNullOrEmpty(tableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {

                if (result == null)
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName("No area measured", rowPrefix), Value = ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                    RuntimeContext.AddRangeToValidationTable(tableName, tableOutput);
                    return false;
                }
                ColorMeasurementLogItems cfg = (ColorMeasurementLogItems)_reportConfig[AnnotationSettings.ColorReportSetting];
                if (result != null && !string.IsNullOrEmpty(tableName))
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    for (int i = 0; i < result.Result.CIEcoordinates.Length; i++)
                    {
                        if (cfg.HasFlag(ColorMeasurementLogItems.CIECoordinates))
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CIECoordinates, rowPrefix, result.Result.colorResult[i].alias), Value = string.Format("({0},{1})", result.Result.CIEcoordinates[i].x, result.Result.CIEcoordinates[i].y), ValidationColumns = null });
                        }
                        if (cfg.HasFlag(ColorMeasurementLogItems.ColorMeasurementResult))
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.ColorMeasRes, rowPrefix, result.Result.colorResult[i].alias), Value = result.Result.results[i] ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                            passed = passed & result.Result.results[i];
                        }
                    }
                    if (cfg.HasFlag(ColorMeasurementLogItems.Image))
                    {
                        if (result.Images.Length > 1)
                        {
                            for (int i = 0; i < result.Images.Length; i++)
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.ColorMeasurementImage, rowPrefix, i), Value = result.Images[i], ValidationColumns = null });
                            }
                        }
                        else if (result.Images.Length == 1)
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.ColorMeasurementImage, rowPrefix), Value = result.Images[0], ValidationColumns = null });
                        }
                    }
                    RuntimeContext.AddRangeToValidationTable(tableName, tableOutput);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0}, {1}", e, Errors.ColumnMissing));
            }
            return passed;
        }

        private bool FillValidationTable(AbsoluteMeasurementResultDetails result, string tableName, string rowPrefix)
        {
            bool passed = true;
            if ((int)_reportConfig[AnnotationSettings.IntensityReportSetting] > 0 && string.IsNullOrEmpty(tableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {
                if (result == null)
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName("No area measured", rowPrefix), Value = ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                    RuntimeContext.AddRangeToValidationTable(tableName, tableOutput);
                    return false;
                }

                IntensityMeasurementLogItems cfg = (IntensityMeasurementLogItems)_reportConfig[AnnotationSettings.IntensityReportSetting];
                if (result != null && result.Result.MeasuredValues != null)
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    if (cfg.HasFlag(IntensityMeasurementLogItems.MeasuredValues))
                    {
                        for (int i = 0; i < result.Result.MeasuredValues.Length; i++)
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MeasValues, rowPrefix, result.Result.intensityMeasSettings[i].alias), Value = result.Result.MeasuredValues[i], ValidationColumns = CreateMinMaxColumn(result.Result.intensityMeasSettings[i].intensityLimit.min, result.Result.intensityMeasSettings[i].intensityLimit.max) });
                            var r = result.Result.MeasuredValues[i];
                            passed = passed & (r >= result.Result.intensityMeasSettings[i].intensityLimit.min) & (r <= result.Result.intensityMeasSettings[i].intensityLimit.max);
                        }
                    }

                    if (cfg.HasFlag(IntensityMeasurementLogItems.Image))
                    {
                        if (result.Images.Length > 1)
                        {
                            for (int i = 0; i < result.Images.Length; i++)
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMeasurementImage, rowPrefix, i), Value = result.Images[i], ValidationColumns = null });
                            }
                        }
                        else if (result.Images.Length == 1)
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMeasurementImage, rowPrefix), Value = result.Images[0], ValidationColumns = null });
                        }
                    }
                    RuntimeContext.AddRangeToValidationTable(tableName, tableOutput);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0}, {1}", e, Errors.ColumnMissing));
            }
            return passed;
        }

        private bool FillValidationTable(PatternResultDetails result, string tableName, string rowPrefix)
        {
            bool passed = true;
            if ((int)_reportConfig[AnnotationSettings.PatternRecognitionReportSetting] > 0 && string.IsNullOrEmpty(tableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {
                PatternRecognitionLogItems cfg = (PatternRecognitionLogItems)_reportConfig[AnnotationSettings.PatternRecognitionReportSetting];
                if (result != null && !string.IsNullOrEmpty(tableName))
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    if (cfg.HasFlag(PatternRecognitionLogItems.Result))
                    {
                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PatternMatchingResults, rowPrefix), Value = result.Result.result ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                        passed = result.Result.result;
                    }
                    if (cfg.HasFlag(PatternRecognitionLogItems.EvaluationMethod))
                    {
                        string evaluationType;
                        switch (result.Result.evaluationMethod)
                        {
                            case 0:
                                evaluationType = GlobalStrings.And;
                                break;
                            case 1:
                                evaluationType = GlobalStrings.Or;
                                break;
                            default:
                                evaluationType = ValidationTableResults.NotDefined;
                                break;
                        }
                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EvaluationMethod, rowPrefix), Value = evaluationType, ValidationColumns = null });
                    }

                    for (int i = 0; i < result.Result.aliasses.Length; i++)
                    {
                        if (cfg.HasFlag(PatternRecognitionLogItems.TypeofPattern))
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.TypeOfPatternInArea, rowPrefix, result.Result.aliasses[i]), Value = result.Result.typesOfPatterns[i], ValidationColumns = null });
                        }
                        if (cfg.HasFlag(PatternRecognitionLogItems.NumberOfMatches))
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.NrOfMatchesExpectedMatches, rowPrefix, result.Result.aliasses[i]), Value = string.Format("{0}/{1}", result.Result.numberOfMatches[i], result.Result.expectedMatches[i]), ValidationColumns = null });
                        }
                        if (cfg.HasFlag(PatternRecognitionLogItems.PatternResults))
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PatternResult, rowPrefix, result.Result.aliasses[i]), Value = result.Result.patternsResults[i] ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = null });

                        }
                        for (int j = 0; j < result.Result.numberOfMatches[i]; j++)
                        {
                            if (cfg.HasFlag(PatternRecognitionLogItems.ScoreInArea))
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.ScoreInArea, rowPrefix, j), Value = result.Result.scores[i][j] / 10, ValidationColumns = null });
                            }
                            if (cfg.HasFlag(PatternRecognitionLogItems.Position))
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PositionXInArea, rowPrefix, j), Value = result.Result.positions[i][j].x, ValidationColumns = null });

                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PositionYInArea, rowPrefix, j), Value = result.Result.positions[i][j].y, ValidationColumns = null });
                            }
                        }
                    }
                    if (cfg.HasFlag(PatternRecognitionLogItems.Image))
                    {
                        if (result.Images.Length > 1)
                        {
                            for (int i = 0; i < result.Images.Length; i++)
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PatternMatchingImage, rowPrefix, i), Value = result.Images[i], ValidationColumns = null });
                            }
                        }
                        else if (result.Images.Length == 1)
                        {
                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.PatternMatchingImage, rowPrefix), Value = result.Images[0], ValidationColumns = null });
                        }
                    }
                    RuntimeContext.AddRangeToValidationTable(tableName, tableOutput);
                }
                else
                {
                    passed = false; //not tested or no result
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0}, {1}", e, Errors.ColumnMissing));
            }
            return passed;
        }

        private bool FillValidationTable(MtfMlfDetectionResultsDetails result, MlfValidationTableConfig mlfValidationConfig, ushort detectionPointInArea, bool timeout = false)
        {
            bool passed = true;
            if ((int)_reportConfig[AnnotationSettings.MainLightFunctionReportSetting] > 0 && string.IsNullOrEmpty(mlfValidationConfig.TableName))
            {
                throw new Exception("Vision: Please create validation table with Min, Max, Required value and add it to the activity or remove test check in component config");
            }
            try
            {
                MainLightFunctionLogItems cfg = (MainLightFunctionLogItems)_reportConfig[AnnotationSettings.MainLightFunctionReportSetting];
                if (result != null)
                {
                    var tableOutput = new List<ValidationRowContainer>();
                    if (!timeout)
                    {
                        if (result.CutOffLineResults != null)
                        {
                            if (result.CutOffLineResults.algorithmValid)
                            {
                                if (cfg.HasFlag(MainLightFunctionLogItems.AdjustmentPoint))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionX, mlfValidationConfig.Function), Value = result.CutOffLineResults.resultPointPosition.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionY, mlfValidationConfig.Function), Value = result.CutOffLineResults.resultPointPosition.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPointInPosition, mlfValidationConfig.Function), Value = (detectionPointInArea == 2) ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                                    passed = (detectionPointInArea == 2);
                                }

                                if (cfg.HasFlag(MainLightFunctionLogItems.StartPoint))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.startPoint.x, ValidationColumns = null });

                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.startPoint.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.FallPoint))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FallPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.fallPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FallPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.fallPoint.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.CheckPoint))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CheckPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.checkPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CheckPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.checkPoint.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.RawPoint))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.RawPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.rawPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.RawPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.rawPoint.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.VOL))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VOLPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.vOL.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VOLPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.vOL.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.VOR))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VORPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.vOR.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VORPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.points.vOR.y, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.HorizontalLine))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLStartPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.horizontalCOL.line.startPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLStartPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.horizontalCOL.line.startPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLEndPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.horizontalCOL.line.endPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLEndPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.horizontalCOL.line.endPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLAngle, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.horizontalCOL.angle, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.VerticalLine))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalCOLStartPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.verticalCOL.line.startPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalCOLStartPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.verticalCOL.line.startPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalCOLEndPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.verticalCOL.line.endPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalCOLEndPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.verticalCOL.line.endPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalCOLAngle, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.verticalCOL.angle, ValidationColumns = null });
                                }

                                if (cfg.HasFlag(MainLightFunctionLogItems.HorizontalUpperLine))
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.UpperHorizontalCOLStartPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.upperHorizontalCOL.line.startPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.UpperHorizontalCOLStartPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.upperHorizontalCOL.line.startPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.UpperHorizontalCOLEndPointX, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.upperHorizontalCOL.line.endPoint.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.UpperHorizontalCOLEndPointY, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.upperHorizontalCOL.line.endPoint.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.UpperHorizontalCOLAngle, mlfValidationConfig.Function), Value = result.CutOffLineResults.lines.upperHorizontalCOL.angle, ValidationColumns = null });
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.IntensityMeasurement))
                                {
                                    int count = 1;

                                    if (result.IntensityMeasurementResult != null && (MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result != MLFIntensityMeasurementResult.NotMeasured)
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                        passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result == MLFIntensityMeasurementResult.Passed);
                                        if (result.IntensityMeasurementResult.pointsResults != null && result.IntensityMeasurementResult.pointsResults.Length > 0)
                                        {


                                            if (result.IntensityMeasurementResult.pointsResults.Length > 1)
                                            {
                                                for (int i = 0; i < result.IntensityMeasurementResult.pointsResults.Length; i++)
                                                {
                                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].pointName, ValidationColumns = null });
                                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].meanIntensity, ValidationColumns = null });
                                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                                    passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result == MLFIntensityMeasurementResult.Passed);
                                                    count++;
                                                }

                                            }
                                            else if (result.IntensityMeasurementResult.pointsResults.Length == 1)
                                            {
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].pointName, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].meanIntensity, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                                passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result == MLFIntensityMeasurementResult.Passed);
                                                count++;
                                            }
                                            if (mlfValidationConfig.ComputeRatios != null && mlfValidationConfig.ComputeRatios.Length > 0)
                                            {
                                                for (int i = 0; i < mlfValidationConfig.ComputeRatios.Length; i++)
                                                {
                                                    if (mlfValidationConfig.ComputeRatios[i] != null)
                                                    {
                                                        if (!mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && mlfValidationConfig.ComputeRatios[i].DenominatorIndex <= result.IntensityMeasurementResult.pointsResults.Length)
                                                        {
                                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                            var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity;
                                                            passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                        }
                                                        else if (mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && result.IntensityMeasurementResult.maximumPoint != null)
                                                        {
                                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                            var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity;
                                                            passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                        }
                                                    }

                                                }
                                            }

                                        }
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = ValidationTableResults.IntensityMaxPoint, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionX, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.x, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionY, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.y, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                        passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result == MLFIntensityMeasurementResult.Passed);
                                    }
                                    else
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ValidationTableResults.IntensityResultNotMeasured, ValidationColumns = null });
                                    }
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.Image))
                                {
                                    if (result.Images.Length > 1)
                                    {
                                        for (int i = 0; i < result.Images.Length; i++)
                                        {
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CutOffLineDetectionImage, mlfValidationConfig.Function, i), Value = result.Images[i], ValidationColumns = null });
                                        }
                                    }
                                    else if (result.Images.Length == 1)
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CutOffLineDetectionImage, mlfValidationConfig.Function), Value = result.Images[0], ValidationColumns = null });
                                    }
                                }
                                if (cfg.HasFlag(MainLightFunctionLogItems.GradientMeasurement))
                                {
                                    if (result.GradientResults != null && (MLFIntensityMeasurementResult)result.GradientResults.result != MLFIntensityMeasurementResult.NotMeasured)
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.result).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                        if (result.GradientResults.gradientMeasurementResult != null)
                                        {
                                            for (int i = 0; i < result.GradientResults.gradientMeasurementResult.Length; i++)
                                            {
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CutName, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].cutName, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].gradientInLimit).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalPositionInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].verticalPositionInLimit).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.x, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.y, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.x, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.y, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientValue, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientValue, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.X, ValidationColumns = null });
                                                //tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.Value, ValidationColumns = null });

                                            }
                                        }
                                    }
                                    if (result.GradientResults.flattnessResult != null)
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FlatnessDirection, mlfValidationConfig.Function), Value = result.GradientResults.flattnessResult.direction.GetName(), ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxDifference, mlfValidationConfig.Function), Value = result.GradientResults.flattnessResult.maxDifference, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FlatnessResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.flattnessResult.result).GetName(), ValidationColumns = null });
                                    }

                                }
                            }
                            else
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AlgorithmValid, mlfValidationConfig.Function), Value = ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                                passed = false;
                            }
                            //result.CutOffLineResults.adjustmentPointPosition
                        } //CutOffLine Results
                        if (result.IsoLineResults != null)
                        {
                            if (cfg.HasFlag(MainLightFunctionLogItems.IntensityMeasurement))
                            {
                                int count = 1;

                                if (result.IntensityMeasurementResult != null && (MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result != MLFIntensityMeasurementResult.NotMeasured)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                    passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result == MLFIntensityMeasurementResult.Passed);
                                    if (result.IntensityMeasurementResult.pointsResults != null && result.IntensityMeasurementResult.pointsResults.Length > 0)
                                    {
                                        if (result.IntensityMeasurementResult.pointsResults.Length > 1)
                                        {
                                            for (int i = 0; i < result.IntensityMeasurementResult.pointsResults.Length; i++)
                                            {
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].pointName, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].meanIntensity, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                                passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result == MLFIntensityMeasurementResult.Passed);
                                                count++;
                                            }
                                        }
                                        else if (result.IntensityMeasurementResult.pointsResults.Length == 1)
                                        {
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].pointName, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].meanIntensity, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                            passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result == MLFIntensityMeasurementResult.Passed);
                                            count++;
                                        }
                                        if (mlfValidationConfig.ComputeRatios != null && mlfValidationConfig.ComputeRatios.Length > 0)
                                        {
                                            for (int i = 0; i < mlfValidationConfig.ComputeRatios.Length; i++)
                                            {
                                                if (mlfValidationConfig.ComputeRatios[i] != null)
                                                {
                                                    if (!mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && mlfValidationConfig.ComputeRatios[i].DenominatorIndex <= result.IntensityMeasurementResult.pointsResults.Length)
                                                    {
                                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                        var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity;
                                                        passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                    }
                                                    else if (mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && result.IntensityMeasurementResult.maximumPoint != null)
                                                    {
                                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                        var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity;
                                                        passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                    }
                                                }

                                            }
                                        }

                                    }
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = ValidationTableResults.IntensityMaxPoint, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionX, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionY, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                    passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result == MLFIntensityMeasurementResult.Passed);
                                }
                                else
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ValidationTableResults.IntensityResultNotMeasured, ValidationColumns = null });
                                }
                            }

                            if (cfg.HasFlag(MainLightFunctionLogItems.AdjustmentPoint))
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionX, mlfValidationConfig.Function), Value = result.IsoLineResults.adjustmentPointPosition.x, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionY, mlfValidationConfig.Function), Value = result.IsoLineResults.adjustmentPointPosition.y, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPointInPosition, mlfValidationConfig.Function), Value = (detectionPointInArea == 2) ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                                passed = passed & (detectionPointInArea == 2);
                            }

                            if (result.Images.Length > 1)
                            {
                                for (int i = 0; i < result.Images.Length; i++)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IsoLineDetectionImage, mlfValidationConfig.Function, i), Value = result.Images[i], ValidationColumns = null });
                                }
                            }
                            else if (result.Images.Length == 1)
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IsoLineDetectionImage, mlfValidationConfig.Function), Value = result.Images[0], ValidationColumns = null });
                            }
                            if (cfg.HasFlag(MainLightFunctionLogItems.GradientMeasurement))
                            {
                                if (result.GradientResults != null && (MLFIntensityMeasurementResult)result.GradientResults.result != MLFIntensityMeasurementResult.NotMeasured)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.result).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                    if (result.GradientResults.gradientMeasurementResult != null)
                                    {
                                        for (int i = 0; i < result.GradientResults.gradientMeasurementResult.Length; i++)
                                        {
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CutName, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].cutName, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].gradientInLimit).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalPositionInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].verticalPositionInLimit).ToString(), ValidationColumns = FillInRequiredTableValue() });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.x, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.y, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.x, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.y, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientValue, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientValue, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.X, ValidationColumns = null });
                                            //tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.Value, ValidationColumns = null });

                                        }
                                    }
                                }

                            }
                            //result.IsoLineResults.adjustmentPointPosition
                        } //IsoLine Results

                        if (result.IsoPercentResults != null)
                        {
                            if (cfg.HasFlag(MainLightFunctionLogItems.IntensityMeasurement))
                            {
                                int count = 1;

                                if (result.IntensityMeasurementResult != null && (MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result != MLFIntensityMeasurementResult.NotMeasured)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                    passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.result == MLFIntensityMeasurementResult.Passed);
                                    if (result.IntensityMeasurementResult.pointsResults != null && result.IntensityMeasurementResult.pointsResults.Length > 0)
                                    {
                                        if (result.IntensityMeasurementResult.pointsResults.Length > 1)
                                        {
                                            for (int i = 0; i < result.IntensityMeasurementResult.pointsResults.Length; i++)
                                            {
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].pointName, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[i].meanIntensity, ValidationColumns = null });
                                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                                passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[i].result == MLFIntensityMeasurementResult.Passed);
                                                count++;
                                            }
                                        }
                                        else if (result.IntensityMeasurementResult.pointsResults.Length == 1)
                                        {
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].pointName, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.pointsResults[0].meanIntensity, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                            passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.pointsResults[0].result == MLFIntensityMeasurementResult.Passed);
                                            count++;
                                        }
                                        if (mlfValidationConfig.ComputeRatios != null && mlfValidationConfig.ComputeRatios.Length > 0)
                                        {
                                            for (int i = 0; i < mlfValidationConfig.ComputeRatios.Length; i++)
                                            {
                                                if (mlfValidationConfig.ComputeRatios[i] != null)
                                                {
                                                    if (!mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && mlfValidationConfig.ComputeRatios[i].DenominatorIndex <= result.IntensityMeasurementResult.pointsResults.Length)
                                                    {
                                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                        var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].DenominatorIndex].meanIntensity;
                                                        passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                    }
                                                    else if (mlfValidationConfig.ComputeRatios[i].UseMaximusAsDenominator && mlfValidationConfig.ComputeRatios[i].NumeratorIndex <= result.IntensityMeasurementResult.pointsResults.Length && result.IntensityMeasurementResult.maximumPoint != null)
                                                    {
                                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(mlfValidationConfig.ComputeRatios[i].Description, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = CreateMinMaxColumn(mlfValidationConfig.ComputeRatios[i].Min, mlfValidationConfig.ComputeRatios[i].Max) });
                                                        var r = result.IntensityMeasurementResult.pointsResults[mlfValidationConfig.ComputeRatios[i].NumeratorIndex].meanIntensity / result.IntensityMeasurementResult.maximumPoint.maximumIntensity;
                                                        passed = passed & (r >= mlfValidationConfig.ComputeRatios[i].Min) & (r <= mlfValidationConfig.ComputeRatios[i].Max);
                                                    }
                                                }

                                            }
                                        }

                                    }
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointName, mlfValidationConfig.Function, count), Value = ValidationTableResults.IntensityMaxPoint, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointMeanIntensity, mlfValidationConfig.Function, count), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensity, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionX, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.x, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityMaxPointPositionY, mlfValidationConfig.Function), Value = result.IntensityMeasurementResult.maximumPoint.maximumIntensityPosition.y, ValidationColumns = null });
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityPointResult, mlfValidationConfig.Function, count), Value = ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                    passed = passed & ((MLFIntensityMeasurementResult)result.IntensityMeasurementResult.maximumPoint.result == MLFIntensityMeasurementResult.Passed);
                                }
                                else
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IntensityResult, mlfValidationConfig.Function), Value = ValidationTableResults.IntensityResultNotMeasured, ValidationColumns = null });
                                }
                            }
                            if (cfg.HasFlag(MainLightFunctionLogItems.AdjustmentPoint))
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionX, mlfValidationConfig.Function), Value = result.IsoPercentResults.adjustmentPointPosition.x, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPositionY, mlfValidationConfig.Function), Value = result.IsoPercentResults.adjustmentPointPosition.y, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustedPointInPosition, mlfValidationConfig.Function), Value = (detectionPointInArea == 2) ? ValidationTableResults.Passed : ValidationTableResults.NotPass, ValidationColumns = FillInRequiredTableValue() });
                                passed = passed & (detectionPointInArea == 2);
                            }

                            if (cfg.HasFlag(MainLightFunctionLogItems.HorizontalLine))
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLStartPointX, mlfValidationConfig.Function), Value = result.IsoPercentResults.line.line.startPoint.x, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLStartPointY, mlfValidationConfig.Function), Value = result.IsoPercentResults.line.line.startPoint.y, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLEndPointX, mlfValidationConfig.Function), Value = result.IsoPercentResults.line.line.endPoint.x, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLEndPointY, mlfValidationConfig.Function), Value = result.IsoPercentResults.line.line.endPoint.y, ValidationColumns = null });
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.HorizontalCOLAngle, mlfValidationConfig.Function), Value = result.IsoPercentResults.line.angle, ValidationColumns = null });
                            }

                            if (result.Images.Length > 1)
                            {
                                for (int i = 0; i < result.Images.Length; i++)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IsoPercentDetectionImage, mlfValidationConfig.Function, i), Value = result.Images[i], ValidationColumns = null });
                                }
                            }
                            if (cfg.HasFlag(MainLightFunctionLogItems.GradientMeasurement))
                            {
                                if (result.GradientResults != null && (MLFIntensityMeasurementResult)result.GradientResults.result != MLFIntensityMeasurementResult.NotMeasured)
                                {
                                    tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientResult, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.result).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                    if (result.GradientResults.gradientMeasurementResult != null)
                                    {
                                        for (int i = 0; i < result.GradientResults.gradientMeasurementResult.Length; i++)
                                        {
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.CutName, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].cutName, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.GradientInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].gradientInLimit).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.VerticalPositionInLimit, mlfValidationConfig.Function), Value = ((MLFIntensityMeasurementResult)result.GradientResults.gradientMeasurementResult[i].verticalPositionInLimit).GetName(), ValidationColumns = FillInRequiredTableValue() });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.x, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.StartPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].startPoint.y, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.x, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.EndPointY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].endPoint.y, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientValue, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientValue, ValidationColumns = null });
                                            tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosX, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.X, ValidationColumns = null });
                                            //tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxGradientPosY, mlfValidationConfig.Function), Value = result.GradientResults.gradientMeasurementResult[i].maxGradientPosition.Value, ValidationColumns = null });

                                        }
                                    }
                                    if(result.GradientResults.flattnessResult != null)
                                    {
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FlatnessDirection, mlfValidationConfig.Function), Value = result.GradientResults.flattnessResult.direction.GetName(), ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.MaxDifference, mlfValidationConfig.Function), Value = result.GradientResults.flattnessResult.maxDifference, ValidationColumns = null });
                                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.FlatnessResult, mlfValidationConfig.Function), Value =( (MLFIntensityMeasurementResult)result.GradientResults.flattnessResult.result).GetName(), ValidationColumns = null });
                                    }
                                }

                            }
                            else if (result.Images.Length == 1)
                            {
                                tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.IsoPercentDetectionImage, mlfValidationConfig.Function), Value = result.Images[0], ValidationColumns = null });
                            }
                            //result.IsoPercentResults.adjustmentPointPosition
                        } //IsoPercent Results
                    }
                    else
                    {
                        passed = false;
                        tableOutput.Add(new ValidationRowContainer() { RowName = CreateRowName(ValidationTableResults.AdjustingFailed, mlfValidationConfig.Function), Value = ValidationTableResults.Timeout, ValidationColumns = FillInRequiredTableValue() });
                    }
                    RuntimeContext.AddRangeToValidationTable(mlfValidationConfig.TableName, tableOutput);
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("{0}, {1}", e, Errors.ColumnMissing));
            }
            return passed;
        }

        private string CreateRowName(string name, int postfix)
        {
            return string.Format("{0} {1}", name, postfix);
        }

        private string CreateRowName(string name, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ? name : string.Format("{0} {1}", prefix, name);
        }
        private string CreateRowName(string name, string prefix, int postfix)
        {
            return string.IsNullOrEmpty(prefix) ? CreateRowName(name, postfix) : string.Format("{0} {1} {2}", prefix, name, postfix);
        }
        private string CreateRowName(string name, string prefix, string postfix)
        {
            return string.IsNullOrEmpty(prefix) ? CreateRowName(name, postfix) : string.Format("{0} {1} {2}", prefix, name, postfix);
        }
        private List<ValidationColumn> CreateMinMaxColumn(double min, double max)
        {
            return new List<ValidationColumn> { new ValidationColumn { Name = ValidationTableResults.Min, Value = min }, new ValidationColumn { Name = ValidationTableResults.Max, Value = max } };
        }

        private List<ValidationColumn> FillInRequiredTableValue()
        {
            return new List<ValidationColumn> { new ValidationColumn { Name = ValidationTableResults.Required, Value = new List<string> { ValidationTableResults.Passed } } };
        }
        private static Dictionary<string, object> CreateReportConfig(List<MTFTabControl> reportConfig)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            foreach (var item in reportConfig)
            {


                if (!res.ContainsKey(item.Header))
                {
                    res.Add(item.Header, 0);
                }
                foreach (var data in item.Content)
                {
                    var table = data as MTFDataTable;
                    if (table != null)
                    {
                        foreach (var line in table.TableData)
                        {
                            if ((bool)line[1])
                            {
                                int locItem;
                                switch (item.Header)
                                {

                                    case AnnotationSettings.MainLightFunctionReportSetting:
                                        //MainLightFunctionLogItems locItem;
                                        locItem = (int)Enum.Parse(typeof(MainLightFunctionLogItems), (string)line[0]);
                                        res[item.Header] = (int)res[item.Header] + locItem;
                                        break;
                                    case AnnotationSettings.IntensityReportSetting:
                                        //MainLightFunctionLogItems locItem;
                                        locItem = (int)Enum.Parse(typeof(IntensityMeasurementLogItems), (string)line[0]);
                                        res[item.Header] = (int)res[item.Header] + locItem;
                                        break;
                                    case AnnotationSettings.ColorReportSetting:
                                        //MainLightFunctionLogItems locItem;
                                        locItem = (int)Enum.Parse(typeof(ColorMeasurementLogItems), (string)line[0]);
                                        res[item.Header] = (int)res[item.Header] + locItem;
                                        break;
                                    case AnnotationSettings.PatternRecognitionReportSetting:
                                        //MainLightFunctionLogItems locItem;
                                        locItem = (int)Enum.Parse(typeof(PatternRecognitionLogItems), (string)line[0]);
                                        res[item.Header] = (int)res[item.Header] + locItem;
                                        break;
                                }
                                //int locItem;

                                //result = result | locItem;
                            }
                        }
                    }
                }
            }

            return res;
        }

        private List<CameraConfig> createCamConfig(List<MTFTabControl> cameras)
        {
            List<CameraConfig> config = new List<CameraConfig>();
            foreach (var camera in cameras)
            {
                foreach (var data in camera.Content)
                {
                    var table = data as MTFDataTable;

                    if (table != null)
                    {
                        foreach (var line in table.TableData)
                        {
                            var cameraName = line[1] as string;
                            var ipAdress = line[0] as string;
                            bool? isVirtual = line[2] as bool?;
                            bool? isSimulationLogEnabled = line[3] as bool?;
                            if (!string.IsNullOrEmpty(ipAdress) && !string.IsNullOrEmpty(cameraName) && isVirtual != null && isSimulationLogEnabled != null && !string.IsNullOrEmpty(cameraName))
                            {
                                config.Add(new CameraConfig() { VirtualCameraName = cameraName, IPAddress = ipAdress, IsSimulation = (bool)isVirtual, IsSimulationLogEnabled = (bool)isSimulationLogEnabled });
                            }
                        }
                    }
                }
            }
            return config;
        }

        private bool IsFullTeachDisabled
        {
            get
            {
                var ST = RuntimeContext.IsSetupMode;
                var SR = RuntimeContext.IsServiceMode;
                var T = RuntimeContext.IsTeachMode;
                return (!ST && !SR && !T) || (ST && SR && !T); //Enable full config for teach and setup only, diable for service
            }
        }
        private string StopableCmd(string cmd)
        {
            lock (asyncResponseLock)
            {
                asyncResponseCmdReceived = false;
            }
            visionErrorCommand = false;
            visionSetupCanceled = false;
            //asyncReponseData = string.Empty;
            //string res=string.Empty;
            var resp = _service.Command(cmd);//cmdFunc.Invoke(cmd);
            if (string.IsNullOrEmpty(resp))
            {
                return null;
            }
            var cmdResponse = Deserialize(resp);//JsonConvert.DeserializeObject<Command>(resp);
            if (cmdResponse == null)
            {
                return null;
            }
            if (cmdResponse.Text != "WaitForSetup")
            {
                return cmdResponse.Data;
            }
            //bool responseReceived = false;

            while (!asyncResponseCmdReceived && !stop && !visionErrorCommand)
            {
                System.Threading.Thread.Sleep(200);
            }

            if (asyncResponseCmdReceived)
            {
                return asyncCmdData; //JsonConvert.DeserializeObject<Command>(asyncImage.JSONData).Data;
            }

            if (stop)
            {
                _service.Command(JsonConvert.SerializeObject(new Command { Text = "StopEVS" }));
            }
            return null;
        }

        private ElcomImage StopableImageCmd(Command cmd)
        {
            visionTimeout = false;
            visionErrorCommand = false;
            lock (asyncResponseLock)
            {
                asyncResponseImageReceived = false;
            }
            asyncImage = new ElcomImage() { JSONData = JsonConvert.SerializeObject(cmd) };

            var resp = _service.CommandImage(asyncImage);
            if (resp == null)
            {
                return null;
            }
            var cmdResponse = Deserialize(resp.JSONData);
            if (cmdResponse == null)
            {
                return null;
            }
            if (cmdResponse.Text != "WaitForSetup")
            {
                return resp;
            }
            //bool responseReceived = false;

            while (!asyncResponseImageReceived && !stop && !visionTimeout && !visionErrorCommand)
            {
                System.Threading.Thread.Sleep(200);
            }

            if (visionTimeout)
            {
                RuntimeContext.RaiseException(this, new TimeoutException("Vision Timeout"), ExceptionLevel.JustInfo);
            }

            if (asyncResponseImageReceived)
            {
                return asyncImage;
            }

            if (stop)
            {

                _service.Command(JsonConvert.SerializeObject(new Command { Text = "StopEVS" }));
            }
            return null;
        }


        //public static void LogSavedData(string runtimeContextKey, string internalKey, object data, IMTFSequenceRuntimeContext runtimeContext)
        //{
        //    if (runtimeContext != null)
        //    {
        //        string variant = string.Empty;
        //        foreach (var item in runtimeContext.SequenceVariantGroups)
        //        {
        //            var value = string.Join(",", runtimeContext.SequenceVariantValue(item));
        //            if (string.IsNullOrEmpty(variant))
        //            {
        //                variant = string.Format("[{0}]", value);
        //            }
        //            else
        //            {
        //                variant = string.Format("{0} [{1}]", variant, value);
        //            }

        //        }
        //        var result = string.Format("*SAVE* RuntimeContextKey: {1}, InternalKey {3},SavedDataVariant: {4}, LoadedDataVariant:{5}, ActualVariant: {0}, Data {2}", variant, runtimeContextKey, JsonConvert.SerializeObject(data), internalKey, runtimeContext.GetVariantForSaveData(runtimeContextKey), runtimeContext.GetVariantForLoadData(runtimeContextKey));
        //        Log.LogMessage(result, "RuntimeContext", true);
        //    }
        //}

        //public static void LogLoadedData(string runtimeContextKey, string internalKey, object data, IMTFSequenceRuntimeContext runtimeContext)
        //{
        //    if (runtimeContext != null)
        //    {
        //        string variant = string.Empty;
        //        foreach (var item in runtimeContext.SequenceVariantGroups)
        //        {
        //            var value = string.Join(",", runtimeContext.SequenceVariantValue(item));
        //            if (string.IsNullOrEmpty(variant))
        //            {
        //                variant = string.Format("[{0}]", value);
        //            }
        //            else
        //            {
        //                variant = string.Format("{0} [{1}]", variant, value);
        //            }

        //        }
        //        var result = string.Format("*LOAD* RuntimeContextKey: {1}, InternalKey {3}, ActualVariant: {0}, Data {2}", variant, runtimeContextKey, JsonConvert.SerializeObject(data), internalKey);
        //        Log.LogMessage(result, "RuntimeContext", true);
        //    }
        //}

        //public static void LogSavedData(string runtimeContextKey, object data, IMTFSequenceRuntimeContext runtimeContext)
        //{
        //    if (runtimeContext != null)
        //    {
        //        string variant = string.Empty;
        //        foreach (var item in runtimeContext.SequenceVariantGroups)
        //        {
        //            var value = string.Join(",", runtimeContext.SequenceVariantValue(item));
        //            if (string.IsNullOrEmpty(variant))
        //            {
        //                variant = string.Format("[{0}]", value);
        //            }
        //            else
        //            {
        //                variant = string.Format("{0} [{1}]", variant, value);
        //            }

        //        }
        //        var result = string.Format("*SAVE* Key: {1}, SavedDataVariant: {0}, LoadedDataVariant:{3}, ActualVariant: {4}, Data {2}", runtimeContext.GetVariantForSaveData(runtimeContextKey), runtimeContextKey, JsonConvert.SerializeObject(data), runtimeContext.GetVariantForLoadData(runtimeContextKey), variant);
        //        Log.LogMessage(result, "RuntimeContext", true);
        //    }
        //}

        //public static void LogLoadedData(string runtimeContextKey, object data, IMTFSequenceRuntimeContext runtimeContext)
        //{
        //    if (runtimeContext != null)
        //    {
        //        string variant = string.Empty;
        //        foreach (var item in runtimeContext.SequenceVariantGroups)
        //        {
        //            var value = string.Join(",", runtimeContext.SequenceVariantValue(item));
        //            if (string.IsNullOrEmpty(variant))
        //            {
        //                variant = string.Format("[{0}]", value);
        //            }
        //            else
        //            {
        //                variant = string.Format("{0} [{1}]", variant, value);
        //            }

        //        }
        //        var result = string.Format("*LOAD* Key: {1}, LoadedDataVariant: {0}, Actual Variant {3}, Data {2}", runtimeContext.GetVariantForLoadData(runtimeContextKey), runtimeContextKey, JsonConvert.SerializeObject(data), variant);
        //        Log.LogMessage(result, "RuntimeContext", true);
        //    }
        //}
        #endregion //private



        public bool Stop
        {
            set
            {
                stop = true;
            }
        }



    }
}
