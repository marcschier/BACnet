namespace System.IO.BACnet.Serialize.Decode
{
    public class Decoder
    {
        public static Decoder Standard = new Decoder(
            Services.AlarmAndEventServices.Standard,
            Services.ObjectAccessServices.Standard);

        public Services.AlarmAndEventServices AlarmAndEventServices { get; }
        public Services.ObjectAccessServices ObjectAccessServices { get; }

        public Decoder(Services.AlarmAndEventServices alarmAndEventServices,
            Services.ObjectAccessServices objectAccessServices)
        {
            ObjectAccessServices = objectAccessServices;
            AlarmAndEventServices = alarmAndEventServices;
        }
    }
}
