using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace System.IO.BACnet
{
    // A reference to PacketDotNet.dll & SharpPcap.dll should be made
    // in order to use this code
    // This class is not in the file BacnetTransport.cs to avoid integration
    // of two dll when Bacnet/Ethernet is not used

    public class BacnetEthernetProtocolTransport : BacnetTransportBase
    {
        private LibPcapLiveDevice _device;
        private byte[] _deviceMac; // Mac of the device
        private readonly string _deviceName;

        /// <summary>
        /// </summary>
        /// <param name="friendlydeviceName">Something like "Local Lan 1", "Wireless network", ...</param>
        public BacnetEthernetProtocolTransport(string friendlydeviceName)
        {
            _deviceName = friendlydeviceName;
            Type = BacnetAddressTypes.Ethernet;
            MaxAdpuLength = BacnetMaxAdpu.MAX_APDU1476;
            HeaderLength = 6 + 6 + 2 + 3;
            MaxBufferLength = 1500;
        }

        public override BacnetAddress GetBroadcastAddress()
        {
            return new BacnetAddress(BacnetAddressTypes.Ethernet, "FF-FF-FF-FF-FF-FF");
        }

        public override void Start()
        {
            _device = Open();
            if (_device == null) throw new Exception("Cannot open Ethernet interface");

            _deviceMac = _device.Interface.MacAddress.GetAddressBytes();

            // filter to only bacnet packets
            _device.Filter = "ether proto 0x82";

            var th = new Thread(CaptureThread) { IsBackground = true };
            th.Start();
        }

        public override int Send(byte[] buffer, int offset, int dataLength, BacnetAddress address,
            bool waitForTransmission, int timeout)
        {
            var hdrOffset = 0;

            for (var i = 0; i < 6; i++)
                buffer[hdrOffset++] = address.adr[i];

            // write the source mac address bytes
            for (var i = 0; i < 6; i++)
                buffer[hdrOffset++] = _deviceMac[i];

            // the next 2 bytes are used for the packet length
            buffer[hdrOffset++] = (byte)(((dataLength + 3) & 0xFF00) >> 8);
            buffer[hdrOffset++] = (byte)((dataLength + 3) & 0xFF);

            // DSAP and SSAP
            buffer[hdrOffset++] = 0x82;
            buffer[hdrOffset++] = 0x82;

            // LLC control field
            buffer[hdrOffset] = 0x03;

            lock (_device)
            {
                _device.SendPacket(buffer, dataLength + HeaderLength);
            }

            return dataLength + HeaderLength;
        }

        public override void Dispose()
        {
            lock (_device)
                _device.Close();
        }

        public override string ToString()
        {
            return "Ethernet";
        }

        private LibPcapLiveDevice Open()
        {
            var devices = LibPcapLiveDeviceList.Instance.Where(dev => dev.Interface != null);

            if (!string.IsNullOrEmpty(_deviceName)) // specified interface
            {
                try
                {
                    var device = devices.FirstOrDefault(dev => dev.Interface.FriendlyName == _deviceName);
                    device?.Open(DeviceMode.Normal, 1000); // 1000 ms read timeout
                    return device;
                }
                catch
                {
                    return null;
                }
            }
            foreach (var device in devices)
            {
                device.Open(DeviceMode.Normal, 1000); // 1000 ms read timeout
                if (device.LinkType == LinkLayers.Ethernet
                    && device.Interface.MacAddress != null)
                    return device;
                device.Close();
            }

            return null;
        }

        private void CaptureThread()
        {
            _device.NonBlockingMode = true; // Without that it's very, very slow
            while (true)
            {
                try
                {
                    var packet = _device.GetNextPacket();
                    if (packet != null)
                        OnPacketArrival(packet);
                    else
                        Thread.Sleep(10); // NonBlockingMode, we need to slow the overhead
                }
                catch
                {
                    return;
                } // closed interface sure !
            }
        }

        private bool _isOutboundPacket(IList<byte> buffer, int offset)
        {
            // check to see if the source mac 100%
            // matches the device mac address of the local device

            for (var i = 0; i < 6; i++)
            {
                if (buffer[offset + i] != _deviceMac[i])
                    return false;
            }

            return true;
        }

        private static byte[] Mac(byte[] buffer, int offset)
        {
            var b = new byte[6];
            Buffer.BlockCopy(buffer, offset, b, 0, 6);
            return b;
        }

        private void OnPacketArrival(RawCapture packet)
        {
            // don't process any packet too short to not be valid
            if (packet.Data.Length <= 17)
                return;

            var buffer = packet.Data;
            var offset = 0;

            // Got frames send by me, not for me, not broadcast
            var dest = Mac(buffer, offset);
            if (!_isOutboundPacket(dest, 0) && dest[0] != 255)
                return;

            offset += 6;

            // source address
            var bacSource = new BacnetAddress(BacnetAddressTypes.Ethernet, 0, Mac(buffer, offset));
            offset += 6;

            // len
            var length = buffer[offset]*256 + buffer[offset + 1];
            offset += 2;

            // 3 bytes LLC hearder
            var dsap = buffer[offset++];
            var ssap = buffer[offset++];
            var control = buffer[offset++];

            length -= 3; // Bacnet content length eq. ethernet lenght minus LLC header length

            // don't process non-BACnet packets
            if (dsap != 0x82 || ssap != 0x82 || control != 0x03)
                return;

            InvokeMessageRecieved(buffer, HeaderLength, length, bacSource);
        }
    }
}