namespace System.IO.BACnet.Serialize.Decode
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DecodesAttribute : Attribute
    {
        public BacnetPropertyIds Property { get; }
        public BacnetObjectTypes? ObjectType { get; }
        public Type EnumType { get; }

        public DecodesAttribute(BacnetPropertyIds property)
        {
            Property = property;
        }

        public DecodesAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType) : this(property)
        {
            ObjectType = objectType;
        }
        public DecodesAttribute(BacnetPropertyIds property, BacnetObjectTypes objectType, Type enumType) : this(property, enumType)
        {
            ObjectType = objectType;
        }

        public DecodesAttribute(BacnetPropertyIds property, Type enumType) : this(property)
        {
            if(!enumType.IsEnum)
                throw new InvalidOperationException();

            EnumType = enumType;
        }
    }
}
