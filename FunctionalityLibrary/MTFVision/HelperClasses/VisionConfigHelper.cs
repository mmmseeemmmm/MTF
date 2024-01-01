using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;
using MTFVision.HelperClasses;
using MTFVision.AnnotationString;
using MTFVision.AnnotationStrings;

namespace MTFVision
{
    [Flags]
    public enum MainLightFunctionLogItems
    {
        AdjustmentPoint = 1,
        StartPoint = 2,
        FallPoint = 4,
        CheckPoint = 8,
        RawPoint = 16,
        VOL = 32,
        VOR = 64,
        HorizontalLine = 128,
        VerticalLine = 256,
        HorizontalUpperLine = 1024,
        IntensityMeasurement=2048,
        Image = 4096,
        GradientMeasurement=8192
    }
    [Flags]
    public enum IntensityMeasurementLogItems
    {
        MeasuredValues = 1,
        Image = 2,
    }
    [Flags]
    public enum ColorMeasurementLogItems
    {
        CIECoordinates = 1,
        ColorMeasurementResult = 2,
        Image = 4,

    }
    [Flags]
    public enum PatternRecognitionLogItems
    {
        TypeofPattern = 1,
        NumberOfMatches = 2,
        ScoreInArea = 4,
        Position = 8,
        Image = 16,
        Result=32,
        EvaluationMethod=64,
        PatternResults=128    
    }
    class VisionConfigHelper : IParameterHelperClass
    {
        private ushort maxCameras = 30;


        //List<ValueWithName> cameras = new List<ValueWithName> { 
        //    new ValueWithName(){DisplayName="Cam001",Value="cam001"},
        //    new ValueWithName(){DisplayName="Cam002",Value="cam002"},
        //    new ValueWithName(){DisplayName="Cam003",Value="cam003"},
        //    new ValueWithName(){DisplayName="Cam004",Value="cam004"},
        //    new ValueWithName(){DisplayName="Cam005",Value="cam005"},
        //    new ValueWithName(){DisplayName="Cam006",Value="cam006"},
        //    new ValueWithName(){DisplayName="Cam007",Value="cam007"},
        //    new ValueWithName(){DisplayName="Cam008",Value="cam008"},
        //    new ValueWithName(){DisplayName="Cam009",Value="cam009"},
        //};
        //List<string> cameras = new List<string> { 
        //    "cam001",
        //    "cam002",
        //    "cam003",
        //    "cam004",
        //    "cam005",
        //    "cam006",
        //    "cam007",
        //    "cam008",
        //    "cam009",
        //    "cam010",
        //    "cam011",
        //    "cam012",
        //    "cam013",
        //    "cam014",
        //    "cam015",
        //    "cam016",
        //    "cam017",
        //    "cam018",
        //    "cam019",
        //    "cam020",
        //};
        //Dictionary<string, List<string>> 
        private readonly Dictionary<string, List<string>> logItems = new Dictionary<string, List<string>>
        {
            
            
                {AnnotationSettings.MainLightFunctionReportSetting,Enum.GetNames(typeof(MainLightFunctionLogItems)).ToList()},
                {AnnotationSettings.ColorReportSetting,Enum.GetNames(typeof(ColorMeasurementLogItems)).ToList()},
                {AnnotationSettings.IntensityReportSetting,Enum.GetNames(typeof(IntensityMeasurementLogItems)).ToList()},
                {AnnotationSettings.PatternRecognitionReportSetting,Enum.GetNames(typeof(PatternRecognitionLogItems)).ToList()},

            //{"Main Light Function", Enum.GetNames(typeof(MainLightFunctionLogItems)).ToList() }
        };
        //{
        //    Enum.GetNames(typeof(MainLightFunctionLogItems)).ToList(),
        //    Enum.GetNames(typeof(ColorMeasurementLogItems)).ToList(),
        //    Enum.GetNames(typeof(IntensityMeasurementLogItems)).ToList(),
        //    Enum.GetNames(typeof(PatternRecognitionLogItems)).ToList()
        //};
        //List<string> mlfItems = Enum.GetNames(typeof(MainLightFunctionLogItems)).ToList();
        //List<string> colorItems = Enum.GetNames(typeof(ColorMeasurementLogItems)).ToList();
        //List<string> intensityItems = Enum.GetNames(typeof(IntensityMeasurementLogItems)).ToList();
        //List<string> patternItems = Enum.GetNames(typeof(PatternRecognitionLogItems)).ToList();


        List<ValueWithName> imageFormat = new List<ValueWithName> { 
            new ValueWithName(){DisplayName=GlobalStrings.ImageFormatRaw,Value=0},
            new ValueWithName(){DisplayName=GlobalStrings.ImageFormatJpeg,Value=1},
            new ValueWithName(){DisplayName=GlobalStrings.ImageFormatPng,Value=2},
        };

        List<ValueWithName> enableImageLogging = new List<ValueWithName> { 
            new ValueWithName(){DisplayName=GlobalStrings.True,Value=true},
            new ValueWithName(){DisplayName=GlobalStrings.False,Value=false},
        };

        List<ValueWithName> enableMessageLogging = new List<ValueWithName> { 
            new ValueWithName(){DisplayName=GlobalStrings.True,Value=true},
            new ValueWithName(){DisplayName=GlobalStrings.False,Value=false},
        };
        List<ValueWithName> forceVisionInit = new List<ValueWithName> { 
            new ValueWithName(){DisplayName=GlobalStrings.True,Value=true},
            new ValueWithName(){DisplayName=GlobalStrings.False,Value=false},
        };


        //string serverUri, string wcfHostExe, List<CameraConfig> cameraConfigs, bool enableAcquireImageLog, string absoluteLogPath, int imageFormat
        public List<MTFParameterDescriptor> GetParameterDescriptors()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), @"data\logs\Vision");
            Directory.CreateDirectory(path);
            var tab = PrepareCameraConfig();
            var report = PrepareReportConfig();
            return new List<MTFParameterDescriptor> {
                new MTFParameterDescriptor{
                    ParameterName = "serverUri",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.TextInput,
                    Value = @"net.tcp://localhost:8743/Design_Time_Addresses/ElcomUniWcfService/ElcomUniService/tcp",
                    DisplayName = "Server URI",
                    Description = "Use default value",
                },
                new MTFParameterDescriptor{
                    ParameterName = "wcfHostExe",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.TextInput,
                    Value =  Path.Combine(Directory.GetCurrentDirectory(),@"ELCOM Vision\ElcomHostForWcfUni\ElcomHostForWcfUni.exe"),//@"c:\_ALCZ\ElcomHostForWcfUni\ElcomHostForWcfUni.exe",
                    DisplayName = "WCFHost Path",
                    Description = "Absolute path to WCFHost.exe",
                },
                new MTFParameterDescriptor { 
                    ParameterName = "cameraConfigs", 
                    IsEnabled = true, 
                    ControlType = MTFParameterControlType.TabControl, 
                    Value = tab,

                    //AllowedValues = cameras.ToArray(),
                    //DisplayName = "Camera configs",
                    //Description = "Absolute path to WCFHost.exe",
                },
                new MTFParameterDescriptor { 
                    ParameterName = "reportConfigs", 
                    IsEnabled = true, 
                    ControlType = MTFParameterControlType.TabControl, 
                    Value = report,

                    //AllowedValues = cameras.ToArray(),
                    //DisplayName = "Camera configs",
                    //Description = "Absolute path to WCFHost.exe",
                },
                new MTFParameterDescriptor { 
                    ParameterName = "enableAcquireImageLog", 
                    IsEnabled = true, 
                    ControlType = MTFParameterControlType.ListBox, 
                    Value=false,
                    AllowedValues =enableImageLogging.ToArray(),
                    Description = "Enable save image in AcquireImage activity to selected folder", 
                    DisplayName = "Enable Acquire Image log",
                },
                new MTFParameterDescriptor{
                    ParameterName = "absoluteLogPath",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.TextInput,
                    Value = path,
                    Description = "Folder where AcquireImage saves acquired images", 
                    DisplayName = "Absolute Acquire Image log path",
                },             
                new MTFParameterDescriptor{
                    ParameterName = "imageFormat",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.ListBox,
                    AllowedValues = imageFormat.ToArray(),
                    Value = 0,
                    Description = "Format of saved images from Acquire Image activity", 
                    DisplayName = "Acquire Image Log format"
                },
                new MTFParameterDescriptor{
                    ParameterName = "enableMessageLog",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.ListBox,
                    AllowedValues = enableMessageLogging.ToArray(),
                    Value = 0,
                    Description = "Log asynchronous responses to file", 
                    DisplayName = "Enable Logging"
                },
                new MTFParameterDescriptor{
                    ParameterName = "forceVisionInit",
                    IsEnabled = true,
                    ControlType = MTFParameterControlType.ListBox,
                    AllowedValues = forceVisionInit.ToArray(),
                    Value = 0,
                    Description = Methods.ForceVisionInitDescription, 
                    DisplayName = Methods.ForceVisionInitDisplayName
                },
            };
        }

        public void ParameterDescriptorChanged(ref List<MTFParameterDescriptor> currentParameterDescriptors)
        {
            if (currentParameterDescriptors[0].Value == null)
            {
                currentParameterDescriptors[0].Value = Directory.GetCurrentDirectory();
            }
            if (currentParameterDescriptors[2].Value != null) //camera config
            {
                List<MTFTabControl> tab = currentParameterDescriptors[2].Value as List<MTFTabControl>;
                if (tab != null)
                {
                    var table = tab[0].Content[0] as MTFDataTable;
                    if (table.TableData.Count < maxCameras)
                    {
                        for (int i = table.TableData.Count; i < maxCameras; i++)
                        {
                            List<object> line = new List<object>();
                            line.Add("");
                            line.Add("");
                            line.Add((bool?)false);
                            line.Add((bool?)false);
                            table.TableData.Add(line);
                        }
                    }
                    tab[0].Content[0] = table;
                }

            }
        }

        private List<MTFTabControl> PrepareCameraConfig()
        {
            List<ColumnDescription> columns = null;
            List<MTFTabControl> tabControls = new List<MTFTabControl>();

            columns = new List<ColumnDescription>();
            //columns.Add(new ColumnDescription { Name = "Camera", DataType = ColumnDataType.Text, ListBoxItems = cameras, ReadOnly = true });
            columns.Add(new ColumnDescription { Name = "IP Adress", DataType = ColumnDataType.Text });
            columns.Add(new ColumnDescription { Name = "Camera Name", DataType = ColumnDataType.Text });
            columns.Add(new ColumnDescription { Name = "Is Simulated", DataType = ColumnDataType.Checkbox });
            columns.Add(new ColumnDescription { Name = "Logging for Simulation", DataType = ColumnDataType.Checkbox });

            MTFTabControl tabControl = new MTFTabControl();

            MTFDataTable dataTable = new MTFDataTable();
            dataTable.CanReorderColumns = false;
            dataTable.CanResizeColumns = true;
            dataTable.CanSort = false;
            dataTable.Columns = columns;
            dataTable.CanAddRow = false;
            dataTable.HeaderVisibility = HeaderVisibility.All;

            tabControl.Header = "Camera Config";

            List<List<object>> tableData = new List<List<object>>();


            for (int i=0;i<maxCameras;i++)
            {
                List<object> line = new List<object>();
                line.Add("");
                line.Add("");
                line.Add((bool?)false);
                line.Add((bool?)false);
                tableData.Add(line);

            }

            dataTable.TableData = tableData;
            tabControl.Content = new List<object>();
            tabControl.Content.Add(dataTable);
            tabControls.Add(tabControl);


            return tabControls;
        }


        private List<MTFTabControl> PrepareReportConfig()
        {
            List<ColumnDescription> columns = null;
            List<MTFTabControl> tabControls = new List<MTFTabControl>();

            columns = new List<ColumnDescription>();
            columns.Add(new ColumnDescription { Name = "Parameter for Logging", DataType = ColumnDataType.Text, ListBoxItems = new List<string>(), ReadOnly = true });
            columns.Add(new ColumnDescription { Name = "Enable Logging", DataType = ColumnDataType.Checkbox });


            foreach (var item in logItems)
            {
                MTFDataTable dataTable = new MTFDataTable();
                dataTable.CanReorderColumns = false;
                dataTable.CanResizeColumns = true;
                dataTable.CanSort = false;
                dataTable.Columns = columns;
                dataTable.CanAddRow = false;
                dataTable.HeaderVisibility = HeaderVisibility.All;

                MTFTabControl tabControl = new MTFTabControl();
                List<List<object>> tableData = new List<List<object>>();
                tabControl.Header = item.Key;//"Main Light Function Config";
                foreach (var list in item.Value)
                {
                    List<object> line = new List<object>();
                    line.Add(list);
                    line.Add((bool?)false);
                    tableData.Add(line);
                }
                dataTable.TableData = tableData;
                tabControl.Content = new List<object>();
                tabControl.Content.Add(dataTable);
                tabControls.Add(tabControl);
            }


            return tabControls;
        }

    }
}
