using System;

/* ICMP class (Internet Control Message Protocol) */
// For creating ICMP (ping) packet objects

class ICMP
{
    // Packet attributes
    public byte type;
    public byte code;
    public UInt16 checksum;
    public int messageSize;
    public byte[] message = new byte[1024];

    // Default constructor
    public ICMP() { }

    // Constructor
    public ICMP(byte[] data, int size)
    {
        type = data[20];
        code = data[21];
        checksum = BitConverter.ToUInt16(data, 22);
        messageSize = size - 24;
        Buffer.BlockCopy(data, 24, message, 0, messageSize);
    }

    /// <summary>
    /// Convert ICMP packet to byte array
    /// </summary>
    /// <returns>Packet in byte array</returns>
    public byte[] getBytes()
    {
        byte[] data = new byte[messageSize + 9];
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
    public UInt16 getChecksum()
    {
        UInt32 chksm = 0;

        byte[] data = getBytes();
        int packetSize = messageSize + 8;
        int index = 0;

        while (index < packetSize)
        {
            chksm += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
            index += 2;
        }

        chksm = (chksm >> 16) + (chksm & 0xffff);
        chksm += (chksm >> 16);

        return (UInt16)(~chksm);
    }
}
