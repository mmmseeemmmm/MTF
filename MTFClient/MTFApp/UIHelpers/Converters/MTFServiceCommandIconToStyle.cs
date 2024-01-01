using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MTFClientServerCommon;

namespace MTFApp.UIHelpers.Converters
{
    public class MTFServiceCommandIconToStyle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FrameworkElement element = new FrameworkElement();
            if (value is MTFServiceCommandIcon)
            {
                switch ((MTFServiceCommandIcon) value)
                {
                    case MTFServiceCommandIcon.HighBeam: return element.FindResource("IconHighBeam");
                    case MTFServiceCommandIcon.LowBeam: return element.FindResource("IconLowBeam");
                    case MTFServiceCommandIcon.PositionLight: return element.FindResource("IconPositionLight");
                    case MTFServiceCommandIcon.RangeControl: return element.FindResource("IconRangeControl");
                    case MTFServiceCommandIcon.TurnIndicator: return element.FindResource("IconTurnIndicator");
                    case MTFServiceCommandIcon.Drl: return element.FindResource("IconServiceCommandDrl");
                    case MTFServiceCommandIcon.CommingHome: return element.FindResource("IconServiceCommandCommingHome");
                    case MTFServiceCommandIcon.CornerLight: return element.FindResource("IconServiceCommandCornerLight");
                    case MTFServiceCommandIcon.SideMarker: return element.FindResource("IconServiceCommandSideMarker");
                    case MTFServiceCommandIcon.ErrorMemory: return element.FindResource("IconServiceCommandErroMemory");
                    case MTFServiceCommandIcon.Identification: return element.FindResource("IconServiceCommandIdentification");
                    case MTFServiceCommandIcon.Parking: return element.FindResource("IconServiceCommandParking");
                    case MTFServiceCommandIcon.Clean: return element.FindResource("IconServiceCommandClean");

                    case MTFServiceCommandIcon.Settings: return element.FindResource("IconSettings");
                    case MTFServiceCommandIcon.Service: return element.FindResource("IconService");
                    case MTFServiceCommandIcon.GoldSample: return element.FindResource("IconGoldSample");
                        
                    case MTFServiceCommandIcon.TriangleCenterHorizontal: return element.FindResource("IconTriangleCenterHorizontal");
                    case MTFServiceCommandIcon.TriangleCenterVertical: return element.FindResource("IconTriangleCenterVertical");
                    case MTFServiceCommandIcon.TriangleDown: return element.FindResource("IconTriangleDown");
                    case MTFServiceCommandIcon.TriangleFullDown: return element.FindResource("IconTriangleFullDown");
                    case MTFServiceCommandIcon.TriangleFullLeft: return element.FindResource("IconTriangleFullLeft");
                    case MTFServiceCommandIcon.TriangleFullLeftDown: return element.FindResource("IconTriangleFullLeftDown");
                    case MTFServiceCommandIcon.TriangleFullLeftTop: return element.FindResource("IconTriangleFullLeftTop");
                    case MTFServiceCommandIcon.TriangleFullRight: return element.FindResource("IconTriangleFullRight");
                    case MTFServiceCommandIcon.TriangleFullRightDown: return element.FindResource("IconTriangleFullRightDown");
                    case MTFServiceCommandIcon.TriangleFullRightTop: return element.FindResource("IconTriangleFullRightTop");
                    case MTFServiceCommandIcon.TriangleFullUp: return element.FindResource("IconTriangleFullUp");
                    case MTFServiceCommandIcon.TriangleLeft: return element.FindResource("IconTriangleLeft");
                    case MTFServiceCommandIcon.TriangleRight: return element.FindResource("IconTriangleRight");
                    case MTFServiceCommandIcon.TriangleUp: return element.FindResource("IconTriangleUp");

                    case MTFServiceCommandIcon.Connect: return element.FindResource("IconServiceConnect");
                    case MTFServiceCommandIcon.Disconnect: return element.FindResource("IconServiceDisconnect");
                    case MTFServiceCommandIcon.Database: return element.FindResource("IconDatabase");
                    case MTFServiceCommandIcon.Configuration: return element.FindResource("IconServiceConfiguration");
                    case MTFServiceCommandIcon.Laser: return element.FindResource("IconServiceLaser");
                    case MTFServiceCommandIcon.Binning: return element.FindResource("IconServiceBinning");
                    case MTFServiceCommandIcon.Temperature: return element.FindResource("IconServiceTemperature");
                    case MTFServiceCommandIcon.AutoMode: return element.FindResource("IconServiceCommandAutoMode");
                    case MTFServiceCommandIcon.ManualMode: return element.FindResource("IconServiceCommandManualMode");
                    case MTFServiceCommandIcon.Home: return element.FindResource("IconServiceCommandHome");
                    case MTFServiceCommandIcon.Alarm: return element.FindResource("IconServiceCommandAlarm");

                    default: return element.FindResource("IconAL");
                }
            }

            return element.FindResource("IconAL");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
