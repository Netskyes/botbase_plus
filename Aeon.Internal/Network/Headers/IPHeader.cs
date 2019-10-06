using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Aeon.Internal.Network.Core.Headers
{
    public class IpHeader
    {


        #region Private Fields

        private byte versionHeaderLength;
        private byte serviceType;
        private ushort totalHMLength;
        private ushort identification;
        private ushort flagsAndOffset;
        private byte TTL;
        private byte protocol;
        private ushort checksum;
        private uint sourceAddress;
        private uint destAddress;
        private byte headerLength;
        private byte[] IPData = new byte[4096];

        #endregion

        public IpHeader(byte[] bytes, int received)
        {
            try
            {
                var memoryStream = new MemoryStream(bytes, 0, received);
                var binaryReader = new BinaryReader(memoryStream);

                versionHeaderLength = binaryReader.ReadByte();
                serviceType = binaryReader.ReadByte();
                totalHMLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                identification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                flagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                TTL = binaryReader.ReadByte();
                protocol = binaryReader.ReadByte();
                checksum = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                sourceAddress = (uint)binaryReader.ReadInt32();
                destAddress = (uint)binaryReader.ReadInt32();

                headerLength = versionHeaderLength;
                headerLength <<= 4;
                headerLength >>= 4;
                headerLength *= 4;

                Array.Copy(bytes, headerLength, IPData, 0, totalHMLength - headerLength);
            }
            catch (Exception e)
            {
                Utils.Log(e.Message + " " + e.StackTrace, "error");
            }
        }
    }
}
