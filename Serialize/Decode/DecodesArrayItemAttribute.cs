namespace System.IO.BACnet.Serialize.Decode
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DecodesArrayItemAttribute : DecodesAttribute
    {
        public int? ExpectedCount { get; }

        public DecodesArrayItemAttribute(BacnetPropertyIds property, int expectedCount) : this(property)
        {
            ExpectedCount = expectedCount;
        }

        public DecodesArrayItemAttribute(BacnetPropertyIds property) : base(property)
        {
        }

        public DecodesArrayItemAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType) : base(property, objectType)
        {
        }

        public DecodesArrayItemAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType, Type enumType) : base(property, objectType, enumType)
        {
        }

        public DecodesArrayItemAttribute(BacnetPropertyIds property, Type enumType) : base(property, enumType)
        {
        }
    }
}
