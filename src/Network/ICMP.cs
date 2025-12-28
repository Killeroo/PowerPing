/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2026 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    /// <summary>
    /// ICMP class, for creating Internet Control Message Protocol (ICMP) packet objects
    /// </summary>
    public class ICMP
    {
        private const int kIcmpHeaderSize = 4;

        public static readonly ICMP EmptyPacket = new()
        {
            Type = 0,
            Code = 0,
            Checksum = 0,
            MessageSize = 0,
            Message = new byte[1024],
        };

        // Packet header
        public byte Type;
        public byte Code;
        public UInt16 Checksum;
        public byte[] Message = new byte[1024];

        public int MessageSize;

        // Constructors
        public ICMP() { }
        public ICMP(byte[] data, int size, int offset = 20) // Offset first 20 bytes which are the IPv4 header
        {   
            Type = data[offset];
            Code = data[offset + 1];
            Checksum = BitConverter.ToUInt16(data, offset + 2);
            MessageSize = size - (offset + kIcmpHeaderSize);
            if (MessageSize > Message.Length)
            {
                Message = new byte[MessageSize];
            }
            Buffer.BlockCopy(data, (offset + kIcmpHeaderSize), Message, 0, MessageSize);
        }

        /// <summary>
        /// Convert ICMP packet to byte array
        /// </summary>
        /// <returns>Packet in byte array</returns>
        public byte[] GetBytes()
        {
            byte[] data = new byte[MessageSize + kIcmpHeaderSize];
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Checksum), 0, data, 2, 2);
            Buffer.BlockCopy(Message, 0, data, kIcmpHeaderSize, MessageSize);
            return data;
        }

        /// <summary>
        /// Calculate checksum of packet using internet checksum (16bit one's compliment checksum)
        /// </summary>
        /// <returns>Packet checksum</returns>
        public UInt16 GetChecksum()
        {
            UInt32 chksm = 0;
            byte[] data = GetBytes();
            int packetSize = MessageSize + kIcmpHeaderSize;
            int index = 0;

            while (index < packetSize)
            {
                if (index + 2 > packetSize)
                {
                    chksm += data[index];
                }
                else
                {
                    chksm += (uint)BitConverter.ToUInt16(data, index);
                }
                index += 2;
            }

            chksm = (chksm >> 16) + (chksm & 0xffff);
            chksm += (chksm >> 16);

            return (UInt16)(~chksm);
        }

        public string PrettyPrint()
        {
            return $"Type={Type.ToString()} Code={Code.ToString()} Checksum={Checksum} MessageSize={MessageSize}";
        }
    }
}