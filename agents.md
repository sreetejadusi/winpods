# Agent Instructions (agents.md)

Welcome, fellow AI Agents! If you have been tasked with maintaining, debugging, or extending the **WinPods** repository, please read these guidelines carefully to understand the architecture and constraints of this migration.

## Context
WinPods is a Windows port of the [LibrePods](https://github.com/kavishdevar/librepods) Android application. The goal is to provide native control over Apple AirPods on Windows 10/11 using C# and WinUI 3.

## Architecture Guidelines
1. **UI Framework:** We strictly use **WinUI 3 (Windows App SDK)**. Do NOT try to migrate components to WPF or UWP. Keep the UI logic inside `MainWindow.xaml` and `MainWindow.xaml.cs`.
2. **Bluetooth Stack:** 
   - **BLE:** Used *only* for the initial battery scan (`BluetoothScanner.cs`) via `Windows.Devices.Bluetooth.Advertisement`.
   - **Bluetooth Classic (RFCOMM):** Used for all deep interactions via the Apple Accessory Protocol (`AacpManager.cs`).
3. **Packet Framing:** 
   - Apple Accessory Protocol (AACP) packets must be framed exactly as defined in `AacpProtocol.cs`. 
   - DO NOT invent new headers. Packets generally start with `0x04 0x00 0x04 0x00`.

## Reverse-Engineering Workflow
If the user asks you to implement a new feature (e.g., Head Tracking, Custom EQ):
1. Navigate to the original Android repository (`librepods/android`).
2. Read `AACPManager.kt` and `Packets.kt` to find the specific Opcodes and byte offsets used by Android LibrePods.
3. Translate those exact byte extractions/constructions into `AacpManager.cs` (for receiving) or `AacpProtocol.cs` (for sending).
4. Do NOT attempt to guess the Apple byte payload—always trace it from the Android source.

## OS Limitations (Windows)
Be aware that certain Android features CANNOT be ported to Windows without kernel-level manipulation:
- **VendorID Spoofing:** Features requiring the host to pretend to be an Apple device (like triggering iOS-style popups or Loud Sound Reduction) fail on Windows because the Windows Bluetooth stack does not allow userspace applications to overwrite the host VendorID. If the user requests this, inform them that a custom Bluetooth kernel driver (`.sys`) is required.

## Building
- Suggest the user run `build.bat` or `build.ps1` to compile the app via the `dotnet` CLI.
- Target framework is `net8.0-windows10.0.19041.0`.
