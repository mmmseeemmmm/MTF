namespace AutomotiveLighting.MTFCommon.Types
{
    [MTFKnownClass]
    public class MeasuredTypedData : MeasuredData
    {
        public double Value { get; set; }
        public bool DigitalValue { get; set; }
    }
}
