using System.Linq;

namespace System.IO.BACnet.Serialize.Decode
{
    public class Date
    {
        public int? Year { get; }
        public int? Month { get; }
        public int? Day { get; }
        public int? DayOfWeek { get; }

        public bool IsPattern { get; }

        public System.DateTime? DateTime { get; }

        public Date(byte year, byte month, byte day, byte dayOfWeek)
        {
            Year = year == 0xFF ? default(int?) : 1900 + year;
            Month = new [] {13, 14, 0xFF}.Contains(month) ? default(int?) : month;
            Day = new[] { 32, 33, 34, 0xFF }.Contains(day) ? default(int?) : day;
            DayOfWeek = dayOfWeek == 0xFF ? default(int?) : dayOfWeek;

            IsPattern = new[] {Year, Month, Day, DayOfWeek}.Any(v => !v.HasValue);
            if(IsPattern)
                return;

            // ReSharper disable PossibleInvalidOperationException
            DateTime = new System.DateTime(Year.Value, Month.Value, Day.Value);
            // ReSharper restore PossibleInvalidOperationException
        }

        public override string ToString()
            => $"{Year?.ToString("d4") ?? "****"}-{Month?.ToString("d2") ?? "**"}-{Day?.ToString("d2") ?? "**"}"
               + $" / DOW: {DayOfWeek?.ToString() ?? "*"}";
    }
}
