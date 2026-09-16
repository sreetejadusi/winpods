using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WinPods.Services
{
    public class AacpManager
    {
        private StreamSocket _socket;
        private DataWriter _writer;
        private DataReader _reader;
        private BluetoothDevice _bluetoothDevice;

        // The Apple Accessory Protocol typically uses a specific RFCOMM service UUID.
        // Known Apple UUIDs for Accessory/AACP:
        private static readonly Guid AACP_SERVICE_UUID = Guid.Parse("74ec2172-0bad-4d01-8f77-997b2be0722a"); 

        public event EventHandler<byte[]> PacketReceived;
        public event EventHandler<EarDetectionEventArgs> EarDetectionReceived;
        public event EventHandler<AacpBatteryEventArgs> BatteryReceived;
        public event EventHandler<bool> ConversationalAwarenessReceived;
        public event EventHandler ConnectionClosed;

        public class EarDetectionEventArgs : EventArgs
        {
            public bool IsLeftInEar { get; set; }
            public bool IsRightInEar { get; set; }
        }

        public class AacpBatteryEventArgs : EventArgs
        {
            public int LeftLevel { get; set; }
            public int RightLevel { get; set; }
            public int CaseLevel { get; set; }
        }

        public async Task<bool> ConnectAsync(ulong bluetoothAddress)
        {
            try
            {
                _bluetoothDevice = await BluetoothDevice.FromBluetoothAddressAsync(bluetoothAddress);
                if (_bluetoothDevice == null)
                    return false;

                // Request RFCOMM services
                var rfcommServices = await _bluetoothDevice.GetRfcommServicesAsync(BluetoothCacheMode.Uncached);
                
                // Fallback to searching for the right service, or just use the first available for prototype
                var service = rfcommServices.Services.FirstOrDefault(s => s.ServiceId.Uuid == AACP_SERVICE_UUID) 
                              ?? rfcommServices.Services.FirstOrDefault();

                if (service == null)
                    return false;

                _socket = new StreamSocket();
                await _socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

                _writer = new DataWriter(_socket.OutputStream);
                _reader = new DataReader(_socket.InputStream);
                _reader.InputStreamOptions = InputStreamOptions.Partial;

                _ = ListenForPacketsAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to connect AACP: {ex.Message}");
                return false;
            }
        }

        private async Task ListenForPacketsAsync()
        {
            try
            {
                while (true)
                {
                    await _reader.LoadAsync(1024);
                    if (_reader.UnconsumedBufferLength > 0)
                    {
                        byte[] buffer = new byte[_reader.UnconsumedBufferLength];
                        _reader.ReadBytes(buffer);
                        
                        PacketReceived?.Invoke(this, buffer);
                        ParsePacket(buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AACP Listening error: {ex.Message}");
                ConnectionClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ParsePacket(byte[] packet)
        {
            // Verify minimum length and header 0x04 0x00 0x04 0x00
            if (packet.Length >= 6 && packet[0] == 0x04 && packet[1] == 0x00)
            {
                byte opcode = packet[4];
                if (opcode == 0x06) // Opcodes.EAR_DETECTION
                {
                    if (packet.Length >= 8)
                    {
                        bool leftInEar = packet[6] == 0x00;
                        bool rightInEar = packet[7] == 0x00;
                        EarDetectionReceived?.Invoke(this, new EarDetectionEventArgs { IsLeftInEar = leftInEar, IsRightInEar = rightInEar });
                    }
                }
                else if (opcode == 0x04) // Opcodes.BATTERY_INFO
                {
                    if (packet.Length >= 22)
                    {
                        int firstLevel = packet[9];
                        int secondLevel = packet[14];
                        int caseLevel = packet[19];

                        // Simplified: In LibrePods, component ID is at 7, 12, 17
                        // We map them loosely here
                        BatteryReceived?.Invoke(this, new AacpBatteryEventArgs
                        {
                            LeftLevel = firstLevel,
                            RightLevel = secondLevel,
                            CaseLevel = caseLevel
                        });
                    }
                }
                else if (opcode == 0x4B) // Opcodes.CONVERSATION_AWARENESS
                {
                    if (packet.Length >= 10)
                    {
                        bool isEnabled = packet[9] == 0x01;
                        ConversationalAwarenessReceived?.Invoke(this, isEnabled);
                    }
                }
            }
        }

        public async Task SendPacketAsync(byte[] payload)
        {
            if (_writer == null) return;
            try
            {
                // In full implementation, we add Checksums and Headers here.
                _writer.WriteBytes(payload);
                await _writer.StoreAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error sending AACP packet: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _writer?.Dispose();
            _reader?.Dispose();
            _socket?.Dispose();
            _bluetoothDevice?.Dispose();
        }
    }
}
