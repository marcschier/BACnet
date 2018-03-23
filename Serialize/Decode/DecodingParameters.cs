namespace System.IO.BACnet.Serialize.Decode
{
    public class DecodingParameters
    {
        public byte[] Buffer { get; }
        public int Offset { get; }
        public byte? ExpectedTag { get; }
        public bool Required { get; }

        public DecodingParameters(byte[] buffer, int offset, byte? expectedTag = null, bool required = true)
        {
            Buffer = buffer;
            Offset = offset;
            ExpectedTag = expectedTag;
            Required = required;
        }
    }
}
