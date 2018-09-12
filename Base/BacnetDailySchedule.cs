using System;
using System.Collections.Generic;
using System.IO.BACnet.Serialize;
using System.Linq;
using System.Text;

namespace System.IO.BACnet
{
    public struct BacnetDailySchedule : ASN1.IEncode, ASN1.IDecode
    {
        public List<BacnetTimeValue> DaySchedule;

        

        public int Decode(byte[] buffer, int offset, uint count)
        {
            int len = 0;
            DaySchedule = new List<BacnetTimeValue>();
            //begin of daily sched
            if (ASN1.IS_OPENING_TAG(buffer[offset + len]))
            {
                len++;
                //end of daily sched
                while (!ASN1.IS_CLOSING_TAG(buffer[offset + len]) )
                {
                    var timeVal = new BacnetTimeValue();
                    len += timeVal.Decode(buffer, offset + len, count);
                    DaySchedule.Add(timeVal);
                }
                //closing tag
                len++;
            }
            
            return len;
        }

        public void Encode(EncodeBuffer buffer)
        {
            ASN1.encode_opening_tag(buffer, 0);

            if (DaySchedule != null)
            {
                foreach (var dayItem in DaySchedule)
                {
                    dayItem.Encode(buffer);
                }
            }
            ASN1.encode_closing_tag(buffer, 0);
        }

        public override string ToString()
        {
            return $"DaySchedule Len: {DaySchedule?.Count() ?? 0}";
        }

    }
}
