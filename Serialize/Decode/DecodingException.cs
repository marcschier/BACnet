namespace System.IO.BACnet.Serialize.Decode
{
    public class DecodingException: BacnetException

    {
        public DecodingException(string message) : base(message)
        {
        }
    }

    public class ExpectedTagNotFoundException : DecodingException
    {
        public ExpectedTagNotFoundException(Class expectedClass, byte? expectedTag, Tag actual)
            : base($"Expected {expectedClass}:{expectedTag ?? (object)"ANY"} but got {actual.Class}:{actual.Number}")
        {
        }
    }
}
