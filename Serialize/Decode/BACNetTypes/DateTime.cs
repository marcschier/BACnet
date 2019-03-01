namespace System.IO.BACnet.Serialize.Decode
{
    public class DateTime : TimeStamp
    {
        public Date Date { get; }
        public Time Time { get; }

        public bool IsPattern { get; }
        public System.DateTime? NativeDateTime { get; }

        public DateTime(Date date, Time time)
        {
            Date = date;
            Time = time;

            IsPattern = Date.IsPattern || Time.IsPattern;

            if(IsPattern)
                return;

            // ReSharper disable PossibleInvalidOperationException - IsPattern == false ensures that this doesn't happen
            NativeDateTime = Date.DateTime.Value.Add(Time.TimeSpan.Value);
            // ReSharper restore PossibleInvalidOperationException
        }

        public override string ToString()
            => $"[{Date} {Time}]";
    }
}
