namespace System.IO.BACnet.Serialize.Decode
{
    public class DeviceObjectReference
    {
        public BacnetObjectId? DeviceId { get; }
        public BacnetObjectId ObjectId { get; }

        public DeviceObjectReference(BacnetObjectId objectId, BacnetObjectId? deviceId = null)
        {
            DeviceId = deviceId;
            ObjectId = objectId;
        }

        public override string ToString()
            => ObjectId + (DeviceId.HasValue ? $"@{DeviceId}" : "");
    }
}
