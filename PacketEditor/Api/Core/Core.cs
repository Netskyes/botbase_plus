using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacketEditor.Api
{
    using NetRelay.Utils;

    public class Core : CoreBase
    {
        public Core()
        {
            DebugLog("Core loaded: " + AppDomain.CurrentDomain.FriendlyName);
        }

        public void DebugLog(string text)
            => Utils.Log(text);
        
        public byte[] BuildPacket(byte[] payload, byte opcode)
        {
            var bytes = new byte[5 + payload.Length];

            var lenBytes = BitConverter.GetBytes((short)payload.Length + 3);
            Array.Copy(lenBytes, 0, bytes, 0, 2);
            bytes[2] = opcode;
            Array.Copy(payload, 0, bytes, 3, payload.Length);

            for (int i = (bytes.Length - 2); i < bytes.Length; i++)
            {
                bytes[i] = 0x00;
            }

            return bytes;
        }
    }
}
