namespace System.IO.BACnet
{
    public enum BacnetSilencedState : byte
    {
        UNSILENCED = 0,
        AUDIBLE_SILENCED =1,
        VISIBLE_SILENCED =2,
        ALL_SILENCED =3
    }
}
