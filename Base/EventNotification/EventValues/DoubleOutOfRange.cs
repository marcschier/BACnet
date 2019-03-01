namespace System.IO.BACnet.EventNotification.EventValues {
    public class DoubleOutOfRange : EventValuesBase {
        public override BacnetEventTypes EventType => BacnetEventTypes.EVENT_DOUBLE_OUT_OF_RANGE;

        public double ExceedingValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public double Deadband { get; set; }
        public double ExceededLimit { get; set; }
    }
}
