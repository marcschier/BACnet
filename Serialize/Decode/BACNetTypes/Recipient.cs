namespace System.IO.BACnet.Serialize.Decode
{
    // TODO: Should be abstract base for ObjectIdentifier and Address. See TimeStamp.
    public class Recipient
    {
        public BacnetObjectId? Device { get; }
        public BacnetAddress Address { get; }

        public Recipient(BacnetObjectId? device = null, BacnetAddress address = null)
        {
            if(device == null && address == null)
                throw new ArgumentException("Either device or address has to be specified!");

            Device = device;
            Address = address;
        }

        public override string ToString()
            => Device.HasValue ? Device.ToString() : Address.ToString();
    }
}