namespace System.IO.BACnet.EventNotification.EventValues {
    public class ChangeOfCharacterString : EventValuesBase {
        public override BacnetEventTypes EventType => BacnetEventTypes.EVENT_CHANGE_OF_CHARACTER_STRING;

        public string ChangedValue { get; set; }
        public BacnetBitString StatusFlags { get; set; }
        public string AlarmValue { get; set; }
    }
}
