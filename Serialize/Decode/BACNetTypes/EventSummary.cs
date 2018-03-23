using System.IO.BACnet.Helpers;
using System.Linq;

namespace System.IO.BACnet.Serialize.Decode
{
    public class EventSummary
    {
        public BacnetObjectId ObjectId { get; }
        public BacnetEventStates EventState { get; }
        public BitString AcknowledgedTransitions { get; }
        public TimeStamp[] EventTimeStamps { get; }
        public BacnetNotifyTypes NotifyType { get; }
        public BitString EventEnable { get; }
        public uint[] EventPriorities { get; }

        public EventSummary(BacnetObjectId objectId, BacnetEventStates eventState, BitString acknowledgedTransitions,
            TimeStamp[] eventTimeStamps, BacnetNotifyTypes notifyType, BitString eventEnable,
            uint[] eventPriorities)
        {
            ObjectId = objectId;
            EventState = eventState;
            AcknowledgedTransitions = acknowledgedTransitions ?? throw new ArgumentNullException(nameof(acknowledgedTransitions));
            EventTimeStamps = eventTimeStamps ?? throw new ArgumentNullException(nameof(eventTimeStamps));
            NotifyType = notifyType;
            EventEnable = eventEnable ?? throw new ArgumentNullException(nameof(eventEnable));
            EventPriorities = eventPriorities ?? throw new ArgumentNullException(nameof(eventPriorities));
        }

        public override string ToString()
            => string.Join(
                   ", ",
                   this.PropertiesWithValues(
                       StringFormatterExtension.Casing.DontChange, nameof(EventTimeStamps), nameof(EventPriorities))) +
               $"\r\n{nameof(EventTimeStamps)}\r\n" + string.Join("\r\n", EventTimeStamps.Select((e, i) => $"\t{i}: {e}")) +
               $"\r\n{nameof(EventPriorities)}\r\n" + string.Join("\r\n", EventPriorities.Select((e, i) => $"\t{i}: {e}"));
    }
}
