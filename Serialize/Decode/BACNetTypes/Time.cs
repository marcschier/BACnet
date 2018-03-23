using System.Linq;

namespace System.IO.BACnet.Serialize.Decode
{
    public class Time : TimeStamp
    {
        public int? Hour { get; }
        public int? Minute { get; }
        public int? Second { get; }
        public int? Hundredths { get; }

        public bool IsPattern { get; }

        public TimeSpan? TimeSpan { get; }

        public static Time Any = new Time(0xFF, 0xFF, 0xFF, 0xFF);

        public Time(byte hour, byte minute, byte second, byte hundreths)
        {
            Hour = hour == 0xFF ? default(int?) : hour;
            Minute = minute == 0xFF ? default(int?) : minute;
            Second = second == 0xFF ? default(int?) : second;
            Hundredths = hundreths == 0xFF ? default(int?) : hundreths;

            IsPattern = new[] {Hour, Minute, Second, Hundredths}.Any(v => !v.HasValue);
            if(IsPattern)
                return;

            // ReSharper disable PossibleInvalidOperationException
            TimeSpan = new TimeSpan(0, Hour.Value, Minute.Value, Second.Value, Hundredths.Value*10);
            // ReSharper restore PossibleInvalidOperationException
        }
        public override string ToString()
            => $"{Hour?.ToString("d2") ?? "**"}:{Minute?.ToString("d2") ?? "**"}:{Second?.ToString("d2") ?? "**"}."
               + (Hundredths.HasValue ? Hundredths.Value + "0" : "*");
    }
}
