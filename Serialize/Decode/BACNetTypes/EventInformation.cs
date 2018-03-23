using System.Collections.Generic;
using System.Linq;

namespace System.IO.BACnet.Serialize.Decode
{
    public class EventInformation
    {
        public IList<EventSummary> EventSummaries { get; }
        public bool MoreEvents { get; }

        public EventInformation(IList<EventSummary> eventSummaries, bool moreEvents)
        {
            EventSummaries = eventSummaries ?? throw new ArgumentNullException(nameof(eventSummaries));
            MoreEvents = moreEvents;
        }

        public override string ToString()
            => string.Join("\r\n", EventSummaries.Select((e, i) => $"{i:D2}: {e}")) +
               $"\r\n{nameof(MoreEvents)}: {MoreEvents}";
    }
}
