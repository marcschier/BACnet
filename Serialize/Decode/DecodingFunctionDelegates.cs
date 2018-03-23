namespace System.IO.BACnet.Serialize.Decode
{
    public delegate ResultBase
        DecodingFunctionDelegate(Context context, byte? expectedTag = null, bool required = true);

    public delegate Result<T> DecodingFunctionDelegate<T>(Context context, byte? expectedTag = null,
        bool required = true);

    //public delegate Result<List<T>> SequenceOfDecodingFunctionDelegate<T>(byte[] buffer, int offset,
    //    DecodingFunctionDelegate<T> method, byte? contextTag = null);
}
