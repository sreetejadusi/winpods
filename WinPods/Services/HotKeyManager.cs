using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace WinPods.Services
{
    public class HotKeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int HOTKEY_ID = 9000;
        
        // Modifiers
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;

        public static void RegisterGlobalHotKey(IntPtr windowHandle, uint modifier, uint key)
        {
            RegisterHotKey(windowHandle, HOTKEY_ID, modifier, key);
        }

        public static void UnregisterGlobalHotKey(IntPtr windowHandle)
        {
            UnregisterHotKey(windowHandle, HOTKEY_ID);
        }
    }
}
