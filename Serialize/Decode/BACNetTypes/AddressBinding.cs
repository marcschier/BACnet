namespace System.IO.BACnet.Serialize.Decode
{
    public class AddressBinding
    {
        public BacnetObjectId ObjectId { get; }
        public BacnetAddress Address { get; }

        public AddressBinding(BacnetObjectId objectId, BacnetAddress address)
        {
            if (objectId == null || address == null)
                throw new ArgumentException("Both object-ID and address need to be specified!");

            ObjectId = objectId;
            Address = address;
        }

        public override string ToString()
            => $"{ObjectId}@{Address}";
    }
}
