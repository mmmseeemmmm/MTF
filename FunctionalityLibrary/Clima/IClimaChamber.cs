using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomotiveLighting.MTFCommon;

namespace ClimaChamberDriver
{
    interface IClimaChamber
    {
        double Temperature { get; set; }

        double Humidity { get; set; }

        double Fan { get; set; }

        List<string> Alarms { get; }

        //bool Start { get; set; }

        bool Run { get; set; }

        void TemperatureWithGradient(double temperature, double gradient);

        void HumidityWithGradient(double humidity, double gradient);

        //void TemperatureRamp(double startTemperature, double finTemperature, double dwell);

        //void HumidityRamp(double startHumidity, double finHumidity ,double dwell);

        //void TempAndHumidRamp(double startTemp, double startHumid, double finTemp, double finHumid, double dwell);

        void AckAlarms();

        IMTFSequenceRuntimeContext RuntimeContext { get; set; }

        void Close();
    }
}
