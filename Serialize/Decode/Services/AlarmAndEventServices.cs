namespace System.IO.BACnet.Serialize.Decode.Services
{
    public class AlarmAndEventServices
    {
        public static AlarmAndEventServices Standard = new AlarmAndEventServices(PrimitiveDecoder.Standard, ConstructDecoder.Standard);

        protected readonly PrimitiveDecoder PrimitiveDecoder;
        protected readonly ConstructDecoder ConstructDecoder;

        public AlarmAndEventServices(PrimitiveDecoder primitiveDecoder, ConstructDecoder constructDecoder)
        {
            PrimitiveDecoder = primitiveDecoder;
            ConstructDecoder = constructDecoder;
        }

        public virtual Result<EventInformation> DecodeGetEventInformationAck(Context context)
            => ConstructDecoder.DecodeSequence(
                context, ()
                    => new EventInformation(
                        ConstructDecoder.DecodeSequenceOf(context, ConstructDecoder.DecodeEventSummary, 0).Value,
                        PrimitiveDecoder.DecodeBoolean(context, 1).Value));

    }
}
