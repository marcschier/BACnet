namespace System.IO.BACnet.EventNotification.EventValues {
    public class SignedOutOfRange : EventValuesBase {
        public int ExceedingValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public uint Deadband { get; set; }
        public int ExceededLimit { get; set; }
    }
}