using System;
using System.Linq;

namespace WinPods.Services
{
    public static class AacpProtocol
    {
        // Based on LibrePods reverse engineering
        public const byte MODE_OFF = 0x01;
        public const byte MODE_ANC = 0x02;
        public const byte MODE_TRANSPARENCY = 0x03;
        public const byte MODE_ADAPTIVE = 0x04;

        public const byte OP_CONTROL_COMMAND = 0x09;
        public const byte ID_LISTENING_MODE = 0x0D;

        public static byte[] CreateSetListeningModePacket(byte mode)
        {
            byte[] header = { 0x04, 0x00, 0x04, 0x00 };
            byte[] payload = new byte[7];
            
            payload[0] = OP_CONTROL_COMMAND;
            payload[1] = 0x00;
            payload[2] = ID_LISTENING_MODE;
            payload[3] = mode;

            return header.Concat(payload).ToArray();
        }

        public static byte[] CreateSetConversationalAwarenessPacket(bool enabled)
        {
            // Identifier for Conversation Detect Config is 0x28
            byte[] header = { 0x04, 0x00, 0x04, 0x00 };
            byte[] payload = new byte[7];
            
            payload[0] = OP_CONTROL_COMMAND;
            payload[1] = 0x00;
            payload[2] = 0x28; // CONVERSATION_DETECT_CONFIG
            payload[3] = (byte)(enabled ? 0x01 : 0x00);

            return header.Concat(payload).ToArray();
        }

        public static byte[] CreateRenamePacket(string name)
        {
            byte[] header = { 0x04, 0x00, 0x04, 0x00 };
            byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(name);
            byte[] payload = new byte[5 + nameBytes.Length];
            
            payload[0] = 0x1A; // Opcodes.RENAME
            payload[1] = 0x00;
            payload[2] = 0x01;
            payload[3] = (byte)nameBytes.Length;
            payload[4] = 0x00;
            
            Array.Copy(nameBytes, 0, payload, 5, nameBytes.Length);
            return header.Concat(payload).ToArray();
        }
    }
}
