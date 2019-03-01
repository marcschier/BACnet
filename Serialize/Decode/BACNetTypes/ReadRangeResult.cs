using System.Collections;

namespace System.IO.BACnet.Serialize.Decode
{
    public class ReadRangeResult
    {
        public BacnetObjectId ObjectId { get; }
        public BacnetPropertyIds PropertyId { get; }
        public uint ArrayIndex { get; }
        public BacnetResultFlags ResultFlags { get; }
        public uint ItemCount { get; }
        public IList ItemData { get; }
        public uint? FirstSequenceNumber { get; }

        public ReadRangeResult(BacnetObjectId objectId, BacnetPropertyIds propertyId, BacnetResultFlags resultFlags,
            uint itemCount, IList itemData, uint? firstSequenceNumber = null,
            uint? arrayIndex = null)
        {
            ObjectId = objectId;
            PropertyId = propertyId;
            ArrayIndex = arrayIndex ?? ASN1.BACNET_ARRAY_ALL;
            ResultFlags = resultFlags;
            ItemData = itemData;
            ItemCount = itemCount;
            FirstSequenceNumber = firstSequenceNumber;
        }
    }
}
