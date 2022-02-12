/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System;

namespace PowerPing
{
    /// <summary>
    /// ICMP class, for creating Internet Control Message Protocol (ICMP) packet objects 
    /// </summary>
    class ICMP
    {
        // Packet attributes
        public byte Type;
        public byte Code;
        public UInt16 Checksum;
        public int MessageSize;
        public byte[] Message = new byte[1024];

        // Constructors
        public ICMP() { }
        public ICMP(byte[] data, int size)
        {
            Type = data[20];
            Code = data[21];
            Checksum = BitConverter.ToUInt16(data, 22);
            MessageSize = size - 24;
            Buffer.BlockCopy(data, 24, Message, 0, MessageSize);
        }

        /// <summary>
        /// Convert ICMP packet to byte array
        /// </summary>
        /// <returns>Packet in byte array</returns>
        public byte[] GetBytes()
        {
            byte[] data = new byte[MessageSize + 9]; // TODO: here we assume packet size, this is probably causing message clip
            Buffer.BlockCopy(BitConverter.GetBytes(Type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(Checksum), 0, data, 2, 2);
            Buffer.BlockCopy(Message, 0, data, 4, MessageSize);
            return data;
        }

        /// <summary>
        /// Calculate checksum of packet
        /// </summary>
        /// <returns>Packet checksum</returns>
        public UInt16 GetChecksum()
        {
            UInt32 chksm = 0;

            byte[] data = GetBytes();
            int packetSize = MessageSize + 8;
            int index = 0;

            while (index < packetSize) {
                chksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }

            chksm = (chksm >> 16) + (chksm & 0xffff);
            chksm += (chksm >> 16);

            return (UInt16)(~chksm);
        }
    }

}
