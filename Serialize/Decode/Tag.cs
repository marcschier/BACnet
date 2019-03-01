namespace System.IO.BACnet.Serialize.Decode
{
    public class Tag
    {
        public Class Class { get; }
        public byte Number { get; }
        public uint LengthOrValueOrType { get; }

        public Tag(Class @class, byte number, uint lengthOrValueOrType)
        {
            Class = @class;
            Number = number;
            LengthOrValueOrType = lengthOrValueOrType;
        }
    }
}
