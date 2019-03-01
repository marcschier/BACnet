namespace System.IO.BACnet.Serialize.Decode
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DecodesSequenceItemAttribute : DecodesAttribute
    {
        public DecodesSequenceItemAttribute(BacnetPropertyIds property) : base(property)
        {
        }

        public DecodesSequenceItemAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType) : base(property, objectType)
        {
        }

        public DecodesSequenceItemAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType, Type enumType) : base(property, objectType, enumType)
        {
        }

        public DecodesSequenceItemAttribute(BacnetPropertyIds property, Type enumType) : base(property, enumType)
        {
        }
    }
}
