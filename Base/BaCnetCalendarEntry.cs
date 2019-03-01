using System.Collections.Generic;
using System.IO.BACnet.Serialize;

namespace System.IO.BACnet
{
    public struct BACnetCalendarEntry : ASN1.IEncode, ASN1.IDecode
    {
        public object Entry; // BacnetDate or BacnetDateRange or BacnetweekNDay

        public void Encode(EncodeBuffer buffer)
        {
            var encEntry = Entry as ASN1.IEncode;

            if (encEntry is BacnetDate)
            {
                ASN1.encode_tag(buffer, 0, true, 4);
                encEntry.Encode(buffer);
            }

            if (encEntry is BacnetDateRange)
            {
                ASN1.encode_opening_tag(buffer, 1);
                encEntry.Encode(buffer);
                ASN1.encode_closing_tag(buffer, 1);
            }

            if (encEntry is BacnetweekNDay)
            {
                ASN1.encode_tag(buffer, 2, true, 3);
                encEntry.Encode(buffer);
            }
        }

        public int Decode(byte[] buffer, int offset, uint count)
        {
            var len = 0;

            byte tagNumber;
            len += ASN1.decode_tag_number(buffer, offset + len, out tagNumber);

            switch (tagNumber)
            {
                case 0:
                    var bdt = new BacnetDate();
                    len += bdt.Decode(buffer, offset + len, count);
                    Entry = bdt;
                    break;
                case 1:
                    var bdr = new BacnetDateRange();
                    len += bdr.Decode(buffer, offset + len, count);
                    Entry = bdr;
                    len++; // closing tag
                    break;
                case 2:
                    var bwd = new BacnetweekNDay();
                    len += bwd.Decode(buffer, offset + len, count);
                    Entry = bwd;
                    break;
            }

            return len; 

        }
        
    }
}