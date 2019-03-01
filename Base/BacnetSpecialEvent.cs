using System;
using System.Collections.Generic;
using System.IO.BACnet.Serialize;
using System.Linq;
using System.Text;

namespace System.IO.BACnet
{
    public struct BacnetSpecialEvent : ASN1.IEncode, ASN1.IDecode
    {
        /// <summary>
        /// has to be BacnetCalendarEntry or BacnetObjectId(Calendar)
        /// </summary>
        public object Period { set; get; }

       /* public bool IsCalendarEntry;
        public BACnetCalendarEntry CalendarEntry;
        public BacnetObjectId CalendarReference;*/

        public List<BacnetTimeValue> ListOfTimeValues;
        public byte EventPriority;
        

        public int Decode(byte[] buffer, int offset, uint count)
        {
            int len = 0;

            byte periodType;
            len += ASN1.decode_tag_number(buffer, offset + len, out periodType); // -> type

            switch (periodType)
            {                
                case 0: //calendarEntry
                    var calendar = new BACnetCalendarEntry();
                    len += calendar.Decode(buffer, offset + len, count);
                    Period = calendar;
                    len += 1; // -> closingtag
                    break;
                case 1: //calendarReference
                    len += ASN1.decode_object_id(buffer, offset + len, out var calRef);
                    Period = calRef;
                    break;
            }

            ListOfTimeValues = new List<BacnetTimeValue>();

            if (ASN1.IS_OPENING_TAG(buffer[offset + len]))
            {
                len++;
                //end of daily sched
                while (!ASN1.IS_CLOSING_TAG(buffer[offset + len]))
                {
                    var timeVal = new BacnetTimeValue();
                    len += timeVal.Decode(buffer, offset + len, count);
                    ListOfTimeValues.Add(timeVal);
                }
                //closing tag
                len++;
            }


            uint evPr;
            len += ASN1.decode_context_unsigned(buffer, offset + len, 3, out evPr);
            EventPriority = (byte)evPr;
            return len;
        }

        public void Encode(EncodeBuffer buffer)
        {
            byte periodType;
            if (Period is BACnetCalendarEntry) periodType = 0;
            else if (Period is BacnetObjectId) periodType = 1;
            else throw new Exception("BacnetSpecialEvent - unsupported period type -> has to be BacnetCalendarEntry or BacnetObjectId");

            ASN1.encode_opening_tag(buffer, periodType);

            if(Period is BACnetCalendarEntry)
            {
                ((BACnetCalendarEntry)Period).Encode(buffer);
                ASN1.encode_closing_tag(buffer, 0);
            }
            else if(Period is BacnetObjectId)
            {
                if (((BacnetObjectId)Period).Type != BacnetObjectTypes.OBJECT_CALENDAR) throw new Exception("Period Object is not an calendar");
                ASN1.encode_bacnet_object_id(buffer, ((BacnetObjectId)Period).Type, ((BacnetObjectId)Period).Instance);
            }


            ASN1.encode_opening_tag(buffer, 2);
            if(ListOfTimeValues != null)
            {
                foreach (var tv in ListOfTimeValues)
                {
                    tv.Encode(buffer);
                }
            }
            ASN1.encode_closing_tag(buffer, 2);

            ASN1.encode_context_unsigned(buffer, 3, EventPriority);



        }
        

    }
}
