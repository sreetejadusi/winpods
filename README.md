# WinPods

<div align="center">
  <h3>AirPods liberated for Windows</h3>
  <p>A native C# and WinUI 3 application that ports the reverse-engineered Apple Accessory Control Protocol (AACP) from <a href="https://github.com/kavishdevar/librepods">LibrePods</a> to Windows 10/11.</p>
</div>

## What is WinPods?

WinPods allows you to use Apple-exclusive AirPods features natively on Windows. It implements the proprietary protocol used to exchange data between AirPods and Apple devices via Bluetooth Classic (RFCOMM) and BLE, bringing seamless control to your PC.

## Features

- **Changing Listening Mode:** Switch between Noise Cancellation, Transparency, Adaptive, and Off directly from Windows.
- **Battery Status:** Live battery percentages for the Left Pod, Right Pod, and Case using both BLE advertisements and direct AACP data.
- **Ear Detection:** Real-time in-ear and out-of-ear detection.
- **Conversational Awareness:** Toggle conversational awareness on supported AirPods models.
- **Rename AirPods:** Rename your AirPods directly from the app.
- **System Tray Integration:** Run WinPods in the background and control your AirPods from the taskbar notification area.
- **Global Hotkeys:** Bind keyboard shortcuts to instantly toggle ANC/Transparency from anywhere in Windows.

## Installation

WinPods is built using **C#** and the **Windows App SDK (WinUI 3)**.

1. Ensure you have **Visual Studio 2022** installed with the following workloads:
   - .NET desktop development
   - Windows application development (Windows App SDK)
2. Open `WinPods.sln` in Visual Studio.
3. Build the project for your architecture (`x64` or `ARM64`).
4. Run the application. 

## How it Works

When you open your AirPods case near your PC, WinPods uses `Windows.Devices.Bluetooth.Advertisement` to intercept the Apple BLE Manufacturer Data (`0x004C`) to display the initial battery status.

Once the AirPods connect to Windows via Bluetooth Classic, WinPods establishes an RFCOMM socket connection over Apple's proprietary Accessory UUID. It then encodes and decodes raw byte packets (such as Opcode `0x09` for Control Commands and Opcode `0x06` for Ear Detection) identical to how an iPhone communicates with them.

## Known Limitations & Pending Migrations (vs. Android)

While the core functionality of LibrePods has been fully migrated, some advanced features from the Android implementation are pending or restricted due to Windows OS limitations:

- **VendorID Spoofing:** Features like Loud Sound Reduction and Multi-Device Connectivity (Hijacking) require spoofing the Bluetooth Vendor ID to pretend the host is an Apple device. While this is achievable on rooted Android, Windows requires a custom Bluetooth kernel driver to modify this.
- **Head Gestures (Accelerometer):** Parsing the raw 70+ byte payload containing Euler angles for head gestures is highly complex and not yet ported to the C# parser.
- **Custom EQ / Hearing Aid:** Deep DSP adjustments and Custom EQ byte manipulations are planned for a future update.

## Credits & License

WinPods is a direct migration of the reverse-engineering efforts from the [LibrePods](https://github.com/kavishdevar/librepods) project. 

The original protocols and byte structures were heavily derived from the LibrePods Kotlin/Rust implementation.
