using System.Collections;
using System.Collections.Generic;

namespace System.IO.BACnet.Serialize.Decode.Services
{
    public class ObjectAccessServices
    {
        public static ObjectAccessServices Standard = new ObjectAccessServices(PrimitiveDecoder.Standard, ConstructDecoder.Standard);

        protected readonly PrimitiveDecoder PrimitiveDecoder;
        protected readonly ConstructDecoder ConstructDecoder;

        public ObjectAccessServices(PrimitiveDecoder primitiveDecoder, ConstructDecoder constructDecoder)
        {
            PrimitiveDecoder = primitiveDecoder;
            ConstructDecoder = constructDecoder;
        }

        public virtual Result<IList<BacnetReadAccessSpecification>> DecodeReadPropertyMultiple(Context context)
            => ConstructDecoder.DecodeSequenceOf(context, ConstructDecoder.DecodeReadAccessSpecification);

        public virtual Result<IList<BacnetReadAccessResult>> DecodeReadPropertyMultipleAck(Context context)
            => ConstructDecoder.DecodeSequenceOf(context, ConstructDecoder.DecodeReadAccessResult);

        /*
        public virtual Result<IList<BacnetReadAccessResult>> DecodeReadRange(Context context)
        {
            var oid = _primitiveDecoder.DecodeObjectIdentifier(context, 0).Value;
            var propId = _primitiveDecoder.DecodeEnumerated<BacnetPropertyIds>(context, 1).Value;
            var propArrayIndex = _primitiveDecoder.DecodeUInt(context, 2, false)?.Value;
            var choiceTag = _primitiveDecoder.DecodeTag(context, Class.OPENING, required:false).Value;
            switch (choiceTag?.Number)
            {
                case null: // choice-tag is optional according to spec -> not an error!
                    break;

                case 3: // by position
                    break;

                case 6: // by sequence
                    break;

                case 7: // by time
                    break;

                default:
                    throw new DecodingException($"CHOICE '{choiceTag.Number}' is unexpected");
            }
        }
        */

        public virtual Result<ReadRangeResult> DecodeReadRangeAck(Context context)
        {
            return ConstructDecoder.DecodeSequence(
                context, () =>
                {
                    var oid = PrimitiveDecoder.DecodeObjectIdentifier(context, 0).Value;
                    var propId = PrimitiveDecoder.DecodeEnumerated<BacnetPropertyIds>(context, 1).Value;
                    var propArrayIndex = PrimitiveDecoder.DecodeUInt(context, 2, false)?.Value;
                    var resultFlags = PrimitiveDecoder.DecodeBitString(context, 3).Value;
                    var itemCount = PrimitiveDecoder.DecodeUInt(context, 4).Value;
                    var itemData = (IList) ConstructDecoder.DecodeAny(context, oid, propId, 5).ValueAsObject;
                    var firstSequenceNumber = PrimitiveDecoder.DecodeUInt(context, 6, false)?.Value;

                    return new ReadRangeResult(
                        oid, propId, (BacnetResultFlags) (resultFlags.ToUInt32() >> 1), itemCount, itemData,
                        firstSequenceNumber, propArrayIndex);
                });
        }
    }
}
