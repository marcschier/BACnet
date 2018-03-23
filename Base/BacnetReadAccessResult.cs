using System.Collections.Generic;

namespace System.IO.BACnet
{
    public class BacnetReadAccessResult
    {
        public BacnetObjectId objectIdentifier;
        public IList<BacnetPropertyValue> values;

        public BacnetReadAccessResult(BacnetObjectId objectIdentifier, IList<BacnetPropertyValue> values)
        {
            this.objectIdentifier = objectIdentifier;
            this.values = values;
        }
    }
}