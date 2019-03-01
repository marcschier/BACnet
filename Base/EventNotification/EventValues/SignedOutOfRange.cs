namespace System.IO.BACnet.EventNotification.EventValues {
    public class SignedOutOfRange : EventValuesBase {
        public override BacnetEventTypes EventType => BacnetEventTypes.EVENT_SIGNED_OUT_OF_RANGE;
        public int ExceedingValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public uint Deadband { get; set; }
        public int ExceededLimit { get; set; }
    }
}
