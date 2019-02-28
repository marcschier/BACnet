namespace System.IO.BACnet.EventNotification.EventValues {
    public class UnsignedOutOfRange : EventValuesBase {

        public uint ExceedingValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public uint Deadband { get; set; }
        public uint ExceededLimit { get; set; }
    }
}