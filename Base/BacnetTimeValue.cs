using System;
using System.Collections.Generic;
using System.IO.BACnet.Serialize;
using System.Linq;
using System.Text;

namespace System.IO.BACnet
{
    public struct BacnetTimeValue : ASN1.IEncode, ASN1.IDecode
    {
        public BacnetGenericTime Time;
        public BacnetValue Value;

        public BacnetTimeValue(BacnetGenericTime time, BacnetValue value)
        {
            Time = time;
            Value = value;
        }

        public int Decode(byte[] buffer, int offset, uint count)
        {
            int len = 0;
            len++; //ignore apptag time ?
            len += ASN1.decode_bacnet_time(buffer, offset + len, out DateTime time);

            Time = new BacnetGenericTime(time, BacnetTimestampTags.TIME_STAMP_TIME);

            var tagLen = ASN1.decode_tag_number_and_value(buffer, offset + len, out BacnetApplicationTags tagNumber, out uint lenValueType);

            if (tagLen > 0)
            {
                len += tagLen;
                var decodeLen = ASN1.bacapp_decode_data(buffer, offset + len, offset + len + 1, tagNumber, lenValueType, out Value);
                len += decodeLen;
            }
            else
            {
                Value = new BacnetValue(BacnetApplicationTags.BACNET_APPLICATION_TAG_NULL, null);
            }

            return len;
        }

        public void Encode(EncodeBuffer buffer)
        {
             ASN1.encode_tag(buffer, (byte)BacnetApplicationTags.BACNET_APPLICATION_TAG_TIME, false, 4);
            ASN1.encode_bacnet_time(buffer, Time.Time);

            ASN1.bacapp_encode_application_data(buffer, Value);
        }

        public override string ToString()
        {
            return $"{Time} = {Value}";
        }
    }
}
