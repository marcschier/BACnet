namespace System.IO.BACnet.EventNotification.EventValues {
    public class DoubleOutOfRange : EventValuesBase {

        public double ExceedingValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public double Deadband { get; set; }
        public double ExceededLimit { get; set; }
    }
}