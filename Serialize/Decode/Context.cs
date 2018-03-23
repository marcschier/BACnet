namespace System.IO.BACnet.Serialize.Decode
{
    public class Context
    {
        public byte[] Buffer { get; }
        public int Offset { get; private set; }

        public Context(byte[] buffer, int offset)
        {
            Buffer = buffer;
            Offset = offset;
        }

        public Result<T> ProcessResult<T>(Result<T> result, int? addToOffset = null)
        {
            if (result == null)
                return null;

            Offset += addToOffset ?? result.Length;

            return result;
        }

        public Result<T> ProcessResult<T>(Result<T> result, Result<Tag> tagResult)
            => ProcessResult(result.AddLength(tagResult.Length), (int) tagResult.Value.LengthOrValueOrType);

        public byte[] GetRawData(uint length)
        {
            var result = new byte[length];
            Array.Copy(Buffer, Offset, result, 0, length);

            Offset += (int)length;

            return result;
        }
    }
}
