using System.IO.BACnet.Serialize.Decode;

namespace System.IO.BACnet.Tests.TestData
{
    public static class Field
    {
        public static (Destination Object, byte[] EncodedBytes) BACNetDestination()
            => (
                new Destination(BitString.Parse("1111111"), Time.Any, new Time(23, 59, 59, 99),
                    new Recipient(new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, 100)), 1, false,
                    BitString.Parse("111")),

                new byte[]
                {
                    0x82, 0x01, 0xFE, 0xB4, 0xFF, 0xFF, 0xFF, 0xFF, 0xB4, 0x17, 0x3B, 0x3B, 0x63, 0x0C, 0x02, 0x00,
                    0x00, 0x64, 0x21, 0x01, 0x10, 0x82, 0x05, 0xE0
                });

        public static (AddressBinding Object, byte[] EncodedBytes) BACNetAddressBinding()
            => (
                new AddressBinding(new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, 100),
                    new BacnetAddress(BacnetAddressTypes.IP, "192.168.100.100:51489")),

                new byte[] {0xC4, 0x02, 0x00, 0x00, 0x64, 0x21, 0x00, 0x65, 0x06, 0xC0, 0xA8, 0x64, 0x64, 0xC9, 0x21});

        public static (DeviceObjectReference Object, byte[] EncodedBytes) BACNetDeviceObjectReference()
            => (
                new DeviceObjectReference(new BacnetObjectId(BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, 454)),

                new byte[] {0x1C, 0x05, 0x80, 0x01, 0xC6});

        public static (EventInformation Object, byte[] EncodedBytes) EmptyEventInformation()
        {
            return (new EventInformation(new EventSummary[0], false),

                new byte[]
                {
                    0x0E, 0x0F, 0x19, 0x00
                });
        }
    }
}
