namespace System.IO.BACnet.Serialize.Decode
{
    public class NumericDecoder
    {
        public static NumericDecoder Standard = new NumericDecoder();

        public NumericDecoder()
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotImplementedException();
        }

        public virtual Result<T> DecodeNumber<T>(byte[] buffer, int offset, int encodedSize, int sizeOfType)
            where T : struct
        {
            var expectedSize = sizeOfType;
            if (encodedSize > expectedSize || encodedSize < 1)
                throw new ArgumentOutOfRangeException(nameof(encodedSize));

            byte[] GetWorkBuffer(bool isSigned)
            {
                var workBuffer1 = new byte[expectedSize];

                var fillWith = (byte)(isSigned && buffer[offset] >> 7 == 1 ? 0xFF : 0x00);

                for (var i = 0; i < workBuffer1.Length; i++)
                {
                    workBuffer1[i] =
                        i >= encodedSize
                            ? fillWith
                            : buffer[offset + encodedSize - 1 - i];
                }

                return workBuffer1;
            }

            T result;

            switch (typeof(T).FullName)
            {
                case "System.UInt16":
                    result = (T)(object)BitConverter.ToUInt16(GetWorkBuffer(false), 0);
                    break;
                case "System.UInt32":
                    result = (T)(object)BitConverter.ToUInt32(GetWorkBuffer(false), 0);
                    break;
                case "System.Int32":
                    result = (T)(object)BitConverter.ToInt32(GetWorkBuffer(true), 0);
                    break;
                case "System.Single":
                    result = (T)(object)BitConverter.ToSingle(GetWorkBuffer(true), 0);
                    break;
                case "System.Double":
                    result = (T)(object)BitConverter.ToDouble(GetWorkBuffer(true), 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(typeof(T).FullName);
            }

            return new Result<T>(result, encodedSize);
        }
    }
}
