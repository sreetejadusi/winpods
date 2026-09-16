using System;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using System.Linq;
using System.Diagnostics;

namespace WinPods.Services
{
    public class AirPodsStatusEventArgs : EventArgs
    {
        public ulong BluetoothAddress { get; set; }
        public int LeftBattery { get; set; }
        public int RightBattery { get; set; }
        public int CaseBattery { get; set; }
        public bool IsLeftInEar { get; set; }
        public bool IsRightInEar { get; set; }
    }

    public class BluetoothScanner
    {
        private BluetoothLEAdvertisementWatcher _watcher;

        public event EventHandler<AirPodsStatusEventArgs> AirPodsFound;

        public BluetoothScanner()
        {
            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.Received += OnAdvertisementReceived;
        }

        public void StartScanning()
        {
            _watcher.Start();
            Debug.WriteLine("Started scanning for AirPods...");
        }

        public void StopScanning()
        {
            _watcher.Stop();
        }

        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Apple's Manufacturer ID is 0x004C
            var manufacturerData = args.Advertisement.ManufacturerData.FirstOrDefault(md => md.CompanyId == 0x004C);
            if (manufacturerData != null)
            {
                using (var reader = DataReader.FromBuffer(manufacturerData.Data))
                {
                    byte[] data = new byte[reader.UnconsumedBufferLength];
                    reader.ReadBytes(data);

                    // Typical AirPods data packet starts with 0x07 (Type) and length
                    if (data.Length >= 2 && data[0] == 0x07)
                    {
                        ParseAirPodsData(data, args.BluetoothAddress);
                    }
                }
            }
        }

        private void ParseAirPodsData(byte[] data, ulong address)
        {
            try
            {
                // Basic parsing (Placeholder logic, actual packet format requires full reverse-engineering as in LibrePods)
                // In a real implementation, indices and bitmasks depend on the specific AirPods model.
                if (data.Length > 12)
                {
                    // Battery levels are typically encoded in half-bytes or specific bytes.
                    // This is a simplified extraction based on general reverse-engineering.
                    int leftBattery = (data[12] & 0x0F) * 10;
                    int rightBattery = ((data[12] & 0xF0) >> 4) * 10;
                    int caseBattery = (data[13] & 0x0F) * 10;

                    // Status flags (In-ear detection, etc.) are in other bytes.

                    if (leftBattery <= 100 || rightBattery <= 100 || caseBattery <= 100)
                    {
                        var status = new AirPodsStatusEventArgs
                        {
                            BluetoothAddress = address,
                            LeftBattery = leftBattery > 100 ? 100 : leftBattery,
                            RightBattery = rightBattery > 100 ? 100 : rightBattery,
                            CaseBattery = caseBattery > 100 ? 100 : caseBattery
                        };
                        
                        AirPodsFound?.Invoke(this, status);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing AirPods data: {ex.Message}");
            }
        }
    }
}
