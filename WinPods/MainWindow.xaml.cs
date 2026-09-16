using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinPods.Services;

namespace WinPods
{
    public sealed partial class MainWindow : Window
    {
        private BluetoothScanner _scanner;
        private AacpManager _aacpManager;
        private ulong _currentAirPodsAddress;

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            
            _scanner = new BluetoothScanner();
            _scanner.AirPodsFound += OnAirPodsFound;
            _scanner.StartScanning();

            _aacpManager = new AacpManager();
            _aacpManager.PacketReceived += OnAacpPacketReceived;
            _aacpManager.EarDetectionReceived += OnEarDetectionReceived;
            _aacpManager.BatteryReceived += OnAacpBatteryReceived;
            _aacpManager.ConversationalAwarenessReceived += OnConversationalAwarenessReceived;
        }

        private void OnAacpBatteryReceived(object sender, AacpManager.AacpBatteryEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // This will override the BLE battery info with highly accurate AACP info
                BatteryValueText.Text = $"Left: {e.LeftLevel}%, Right: {e.RightLevel}%, Case: {e.CaseLevel}% (AACP)";
            });
        }

        private void OnConversationalAwarenessReceived(object sender, bool isEnabled)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                // Temporarily unhook event to prevent loop
                ConversationalAwarenessToggle.Toggled -= OnConversationalAwarenessToggled;
                ConversationalAwarenessToggle.IsOn = isEnabled;
                ConversationalAwarenessToggle.Toggled += OnConversationalAwarenessToggled;
            });
        }

        private void OnEarDetectionReceived(object sender, AacpManager.EarDetectionEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                StatusText.Text = $"Ear Detection: Left ({(e.IsLeftInEar ? "In" : "Out")}), Right ({(e.IsRightInEar ? "In" : "Out")})";
            });
        }

        private async void OnAirPodsFound(object sender, AirPodsStatusEventArgs e)
        {
            if (_currentAirPodsAddress == 0)
            {
                _currentAirPodsAddress = e.BluetoothAddress;
                // Attempt to connect AACP
                bool connected = await _aacpManager.ConnectAsync(_currentAirPodsAddress);
                
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (connected)
                    {
                        StatusText.Text = $"Connected to AirPods via AACP";
                        BtnAnc.IsEnabled = true;
                        BtnOff.IsEnabled = true;
                        BtnTransparency.IsEnabled = true;
                        ConversationalAwarenessToggle.IsEnabled = true;
                        RenameBox.IsEnabled = true;
                        RenameButton.IsEnabled = true;
                    }
                });
            }

            DispatcherQueue.TryEnqueue(() =>
            {
                if (StatusText.Text != "Connected to AirPods via AACP")
                    StatusText.Text = $"Found AirPods: {e.BluetoothAddress}";
                    
                BatteryPanel.Visibility = Visibility.Visible;
                BatteryValueText.Text = $"Left: {e.LeftBattery}%, Right: {e.RightBattery}%, Case: {e.CaseBattery}%";
            });
        }

        private void OnAacpPacketReceived(object sender, byte[] e)
        {
            // Process incoming AACP packets (Ear detection, battery, etc.)
            DispatcherQueue.TryEnqueue(() =>
            {
                // StatusText.Text = $"Received packet: {BitConverter.ToString(e)}";
            });
        }

        private async void OnModeChanged(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string mode)
            {
                StatusText.Text = $"Setting mode to: {mode}...";
                byte modeByte = AacpProtocol.MODE_OFF;
                
                if (mode == "Anc") modeByte = AacpProtocol.MODE_ANC;
                else if (mode == "Transparency") modeByte = AacpProtocol.MODE_TRANSPARENCY;

                byte[] packet = AacpProtocol.CreateSetListeningModePacket(modeByte);
                await _aacpManager.SendPacketAsync(packet);
            }
        }

        private async void OnConversationalAwarenessToggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggle)
            {
                byte[] packet = AacpProtocol.CreateSetConversationalAwarenessPacket(toggle.IsOn);
                await _aacpManager.SendPacketAsync(packet);
            }
        }

        private async void OnRenameClicked(object sender, RoutedEventArgs e)
        {
            string newName = RenameBox.Text;
            if (!string.IsNullOrWhiteSpace(newName))
            {
                byte[] packet = AacpProtocol.CreateRenamePacket(newName);
                await _aacpManager.SendPacketAsync(packet);
                StatusText.Text = $"Renamed AirPods to: {newName}";
            }
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            _aacpManager?.Disconnect();
            Application.Current.Exit();
        }
    }
}
