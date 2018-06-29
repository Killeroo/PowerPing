/*
MIT License - PowerPing 

Copyright (c) 2018 Matthew Carney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;

namespace PowerPing
{
    /// <summary>
    /// ICMP class, for creating Internet Control Message Protocol (ICMP) packet objects 
    /// </summary>
    class ICMP
    {
        // Packet attributes
        public byte type;
        public byte code;
        public UInt16 checksum;
        public int messageSize;
        public byte[] message = new byte[1024];

        // Constructors
        public ICMP() { }
        public ICMP(byte[] data, int size)
        {
            type = data[20];
            code = data[21];
            // TODO: ICMP_COOKIES: Debug here to check ensure whole message field is saved
            checksum = BitConverter.ToUInt16(data, 22);
            messageSize = size - 24;
            Buffer.BlockCopy(data, 24, message, 0, messageSize);
        }

        /// <summary>
        /// Convert ICMP packet to byte array
        /// </summary>
        /// <returns>Packet in byte array</returns>
        public byte[] GetBytes()
        {
            byte[] data = new byte[messageSize + 9]; // TODO: here we assume packet size, this is probably causing message clip
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, data, 2, 2);
            Buffer.BlockCopy(message, 0, data, 4, messageSize);
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
            int packetSize = messageSize + 8;
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
