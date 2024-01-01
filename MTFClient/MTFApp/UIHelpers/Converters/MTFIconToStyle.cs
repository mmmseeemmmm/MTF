using AutomotiveLighting.MTFCommon;
using System;
using System.Windows;
using System.Windows.Data;

namespace MTFApp.UIHelpers.Converters
{
    public class MTFIconToStyle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();

            if (value is MTFIcons)
            {
                switch ((MTFIcons)value)
                {
                    case MTFIcons.NewFile: return element.FindResource("IconNewFile");
                    case MTFIcons.OpenFile: return element.FindResource("IconOpenFile");
                    case MTFIcons.AddNew: return element.FindResource("IconAddNew");
                    case MTFIcons.AddExisting: return element.FindResource("IconAddExisting");
                    case MTFIcons.SaveFile: return element.FindResource("IconSaveFile");
                    case MTFIcons.SaveFileAs: return element.FindResource("IconSaveFileAs");
                    case MTFIcons.Export: return element.FindResource("IconExport");
                    case MTFIcons.Import: return element.FindResource("IconImport");
                    case MTFIcons.Copy: return element.FindResource("IconCopy");
                    case MTFIcons.Paste: return element.FindResource("IconPaste");
                    case MTFIcons.JumpToActivity: return element.FindResource("IconJumpToActivity");
                    case MTFIcons.Wizard: return element.FindResource("IconWizard");
                    case MTFIcons.ZipFile: return element.FindResource("IconZipFile");
                    case MTFIcons.SaveAll: return element.FindResource("IconSaveAll");
                    case MTFIcons.TreeView: return element.FindResource("IconTreeView");
                    case MTFIcons.TimeView: return element.FindResource("IconTimeView");
                    case MTFIcons.TableView: return element.FindResource("IconTableView");
                        

                    case MTFIcons.StartSequence: return element.FindResource("IconSequenceStart");
                    case MTFIcons.StopSequence: return element.FindResource("IconSequenceStop");
                    case MTFIcons.PauseSequence: return element.FindResource("IconSequencePause");

                    case MTFIcons.TermEqual: return element.FindResource("TermEqualIcon");
                    case MTFIcons.TermAdd: return element.FindResource("TermAddIcon");
                    case MTFIcons.TermAnd: return element.FindResource("TermAndIcon");
                    case MTFIcons.TermAct: return element.FindResource("TermActIcon");
                    case MTFIcons.TermBitAnd: return element.FindResource("TermBitAndIcon");
                    case MTFIcons.TermBitOr: return element.FindResource("TermBitOrIcon");
                    case MTFIcons.TermGreaterOrEqual: return element.FindResource("TermGreaterOrEqualIcon");
                    case MTFIcons.TermGreater: return element.FindResource("TermGreaterIcon");
                    case MTFIcons.TermIsInList: return element.FindResource("TermIsInListIcon");
                    case MTFIcons.TermNotIsInList: return element.FindResource("TermNotIsInListIcon");
                    case MTFIcons.TermLess: return element.FindResource("TermLessIcon");
                    case MTFIcons.TermLessOrEqual: return element.FindResource("TermLessOrEqualIcon");
                    case MTFIcons.TermNot: return element.FindResource("TermNotIcon");
                    case MTFIcons.TermNotEqual: return element.FindResource("TermNotEqualIcon");
                    case MTFIcons.TermOr: return element.FindResource("TermOrIcon");
                    case MTFIcons.TermStringFormat: return element.FindResource("TermStringFormatIcon");
                    case MTFIcons.TermSubtract: return element.FindResource("TermSubtractIcon");
                    case MTFIcons.TermVal: return element.FindResource("TermValIcon");
                    case MTFIcons.TermVariable: return element.FindResource("TermVarIcon");
                    case MTFIcons.TermDivide: return element.FindResource("TermDivideIcon");
                    case MTFIcons.TermMultiply: return element.FindResource("TermMultiplyIcon");
                    case MTFIcons.TermFillValidationTable: return element.FindResource("TermValidationTableIcon");
                    case MTFIcons.TermMin: return element.FindResource("TermMinIcon");
                    case MTFIcons.TermMax: return element.FindResource("TermMaxIcon");
                    case MTFIcons.TermSum: return element.FindResource("TermSumIcon");
                    case MTFIcons.TermAvg: return element.FindResource("TermAvgIcon");
                    case MTFIcons.TermRange: return element.FindResource("TermRangeIcon");
                    case MTFIcons.TermIsNull: return element.FindResource("TermIsNullIcon");
                    case MTFIcons.TermModulo: return element.FindResource("TermModuloIcon");
                    case MTFIcons.TermAbsoluteValue: return element.FindResource("TermAbsoluteValueIcon");
                    case MTFIcons.TermPower: return element.FindResource("TermPowerIcon");
                    case MTFIcons.TermRoot: return element.FindResource("TermRootIcon");
                    case MTFIcons.TermRound: return element.FindResource("TermRoundIcon");
                    case MTFIcons.TermRoundToValue: return element.FindResource("TermRoundToValueIcon");
                    case MTFIcons.TermRegEx: return element.FindResource("TermRegExIcon");
                    case MTFIcons.TermRegExValue: return element.FindResource("TermRegExValueIcon");

                    case MTFIcons.Communication: return element.FindResource("IconCommunication");
                    case MTFIcons.Clima: return element.FindResource("IconClima");
                    case MTFIcons.BarcodeScanner: return element.FindResource("IconBarcodeScanner");
                    case MTFIcons.PLC: return element.FindResource("IconPLC");
                    case MTFIcons.PowerSupply: return element.FindResource("IconPowerSupply");
                    case MTFIcons.ProfiNet: return element.FindResource("IconProfiNet");
                    case MTFIcons.Relay: return element.FindResource("IconRelay");
                    case MTFIcons.Test: return element.FindResource("IconTest");
                    case MTFIcons.Vision: return element.FindResource("IconVision");
                    case MTFIcons.General: return element.FindResource("IconGeneral");
                    case MTFIcons.SkullAndCrossbones: return element.FindResource("IconSkullAndCrossbones");
                    case MTFIcons.EnduranceCheck: return element.FindResource("IconEnduranceCheck");
                    case MTFIcons.Oscilloscope: return element.FindResource("IconOscilloscope");
                    case MTFIcons.Search: return element.FindResource("IconSearch");
                    case MTFIcons.LED: return element.FindResource("IconLED");
                    case MTFIcons.Message: return element.FindResource("IconMessage");
                    case MTFIcons.Execute: return element.FindResource("IconExecute");
                    case MTFIcons.Pencil: return element.FindResource("IconPencil");
                    case MTFIcons.ErrorHandling: return element.FindResource("IconErrorHandling");
                    case MTFIcons.Sequence: return element.FindResource("IconSequence");
                    case MTFIcons.Variable: return element.FindResource("IconVariable");
                    case MTFIcons.Tune: return element.FindResource("IconTune");
                    case MTFIcons.Settings: return element.FindResource("IconSettings");
                    case MTFIcons.Parsing: return element.FindResource("IconParsing");
                    case MTFIcons.DataCollection: return element.FindResource("IconDataCollection");
                    case MTFIcons.Presentation: return element.FindResource("IconPresentation");
                    case MTFIcons.CDaq: return element.FindResource("IconCDaq");
                    case MTFIcons.Database: return element.FindResource("IconDatabase");
                    case MTFIcons.RFID: return element.FindResource("IconRFID");
                    case MTFIcons.Merge: return element.FindResource("IconMerge");
                    case MTFIcons.Rename: return element.FindResource("IconRename");
                    case MTFIcons.Link: return element.FindResource("IconLink");
                    case MTFIcons.Bug: return element.FindResource("IconBug");
                    case MTFIcons.ALCheck: return element.FindResource("IconALCheck");
                    case MTFIcons.GoldSample: return element.FindResource("IconGoldSample");
                    case MTFIcons.NoPhoto: return element.FindResource("IconNoPhoto");
                    case MTFIcons.Photo: return element.FindResource("IconPhoto");
                    case MTFIcons.ValidationTable: return element.FindResource("IconValidationTable");
                    case MTFIcons.RemoveTab: return element.FindResource("IconRemoveTab");
                    case MTFIcons.CollapseAll: return element.FindResource("IconCollapseAll");
                    case MTFIcons.ExpandAll: return element.FindResource("IconExpandAll");
                    case MTFIcons.Back: return element.FindResource("IconBack");
                    case MTFIcons.Convert: return element.FindResource("IconConvert");
                    case MTFIcons.Certificate: return element.FindResource("IconCertificate");
                    case MTFIcons.ProductionCounter: return element.FindResource("IconProductionCounter");
                    case MTFIcons.Printer: return element.FindResource("IconPrinter");
                    case MTFIcons.GraphicalView: return element.FindResource("IconGraphicalView");

                    case MTFIcons.StepInto: return element.FindResource("IconStepInto");
                    case MTFIcons.StepOver: return element.FindResource("IconStepOver");
                    case MTFIcons.StepOut: return element.FindResource("IconStepOut");
                    case MTFIcons.Variant: return element.FindResource("IconVariant");
                    case MTFIcons.Service: return element.FindResource("IconService");
                    case MTFIcons.Teaching: return element.FindResource("IconTeaching");

                    case MTFIcons.RightArrow: return element.FindResource("IconRightArrow");
                    case MTFIcons.Motor: return element.FindResource("IconMotor");

                    case MTFIcons.ActiveBreakPoint: return element.FindResource("IconActiveBreakPoint");
                    case MTFIcons.DeleteBreakPoint: return element.FindResource("IconDeleteBreakPoint");

                    case MTFIcons.AL: return element.FindResource("IconAL");
                    case MTFIcons.IconBackup: return element.FindResource("IconBackup");
                    case MTFIcons.Conveyor: return element.FindResource("IconConveyor");

                    case MTFIcons.LanguageEn: return element.FindResource("LanguageEn");
                    case MTFIcons.LanguageCz: return element.FindResource("LanguageCz");
                    case MTFIcons.LanguageDe: return element.FindResource("LanguageDe");
                    case MTFIcons.LanguageIt: return element.FindResource("LanguageIt");
                    case MTFIcons.LanguageMx: return element.FindResource("LanguageMx");

                    case MTFIcons.LaserMarker: return element.FindResource("IconLaserMarker");
                    case MTFIcons.VideoCamera: return element.FindResource("IconVideoCamera");
                    case MTFIcons.Graph: return element.FindResource("Graph");

                    default: return element.FindResource("IconNone");
                }
            }

            return element.FindResource("IconNone");

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
