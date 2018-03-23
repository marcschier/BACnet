using System.Linq;

namespace System.IO.BACnet.Serialize.Decode
{
    public class BitString
    {
        public byte[] RawValue { get; }
        public byte UnusedBits { get; }

        private readonly string _stringRepresentation;
        private readonly uint? _uintRepresentation;

        public BitString() : this(new byte[] {0x00}, 8)
        {
        }

        public BitString(byte[] rawValue, byte unusedBits)
        {
            RawValue = rawValue;
            UnusedBits = unusedBits;

            _stringRepresentation = new string(
                Enumerable.Range(0, RawValue.Length * 8 - UnusedBits)
                    .Select(i => (RawValue[i / 8] & (1 << (7-i % 8))) == 0 ? '0' : '1').ToArray());

            if (rawValue.Length > sizeof(uint))
                return;

            var uintData = new byte[sizeof(uint)];
            Array.Copy(rawValue, uintData, rawValue.Length);
            _uintRepresentation = BitConverter.ToUInt32(uintData, 0) >> (unusedBits);
        }

        public override string ToString()
            => _stringRepresentation;

        public static BitString Parse(string input)
        {
            var data = new byte[input.Length / 8 + 1];

            for(var i=0; i < input.Length; i++)
            {
                if(input[i] == '0')
                    continue;

                data[i / 8] |= (byte)(1 << (7 - i % 8));
            }

            var unusedBits = 8 - input.Length % 8;

            return new BitString(data, (byte)unusedBits);
        }

        public uint ToUInt32()
            => _uintRepresentation ?? throw new InvalidOperationException(
                   $"raw value with length {RawValue.Length} can't be represented as {nameof(UInt32)}");
    }
}
