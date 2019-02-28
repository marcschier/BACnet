namespace System.IO.BACnet.EventNotification.EventValues {
    public class ChangeOfCharacterString : EventValuesBase {

        public string ChangedValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public string AlarmValue { get; set; }
    }
}