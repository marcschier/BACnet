using System.IO.BACnet.Helpers;

namespace System.IO.BACnet.Serialize.Decode
{
    public class Destination
    {
        public BitString DaysOfWeek { get; }
        public Time FromTime { get; }
        public Time ToTime { get; }
        public Recipient Recipient { get; }
        public uint ProcessId { get; }
        public bool IssueConfirmedNotifications { get; }
        public BitString Transitions { get; }

        public Destination(BitString daysOfWeek, Time fromTime, Time toTime, Recipient recipient, uint processId,
            bool issueConfirmedNotifications, BitString transitions)
        {
            DaysOfWeek = daysOfWeek;
            FromTime = fromTime;
            ToTime = toTime;
            Recipient = recipient;
            ProcessId = processId;
            IssueConfirmedNotifications = issueConfirmedNotifications;
            Transitions = transitions;
        }

        public override string ToString()
            => "[" + string.Join(", ", this.PropertiesWithValues(StringFormatterExtension.Casing.DontChange)) + "]";
    }
}