using System;
using System.Net;

namespace PowerPing 
{
    /// <summary>
    /// IPv4 class, for creating Internet Protocol Version 4 (IPv4) packet objects
    /// </summary>
    class IPv4 
    {
        // Packet header
        public byte Version; // Only 4 bits used (cos byte is _8_ bits)
        public byte HeaderLength; // Only 4 bits used 
        public byte TypeOfService; // More explict?
        public UInt16 TotalLength;
        public UInt16 Identification;
        public byte Flags; // Only 3 bits used
        public UInt16 FragmentOffset;
        public byte TimeToLive;
        public byte Protocol;
        public UInt16 HeaderChecksum;
        public IPAddress Source;
        public IPAddress Destination;

        // Packet data
        public byte[] Data;

        // Constructors
        public IPv4() { }
        public IPv4(byte[] data, int size)
        {
            //https://stackoverflow.com/a/10493604
            Version = (byte) ((data[0] >> 4) & 0xF); // shift version to first 4 bits and then use a mask to only copy the first 4 bits using AND (mask is just 4 1 bits so with AND only the first bytes will pass the and operation with 00001111 (masking may not be needed here, shift might be enough
            HeaderLength = (byte)((data[0] & 0xF) * 0x4); // mask version bytes and times by 4 (length is number of 32 bit words (4 bytes) = 5 x 4)
            TypeOfService = data[1];
            TotalLength = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 2));
            Identification = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 4));
            Flags = (byte) (data[6] >> 5);
            FragmentOffset = (UInt16) ((data[6] & 0x1f) << 8 | data[7]);
            TimeToLive = data[8];
            Protocol = data[9];
            HeaderChecksum = (ushort) IPAddress.NetworkToHostOrder(BitConverter.ToInt16(data, 10));
            Source = new IPAddress(BitConverter.ToUInt32(data, 12));
            Destination = new IPAddress(BitConverter.ToUInt32(data, 16));

            // body
            // 20 - size?
        }

        // Game plan:
        // - (DONE) Sort network order/big endian shit
        // - Implement Get bytes (remember hosttonetwork order)
        // - Read out ippacket thenreconvert it to bytes, check with wireshark to check result is right
        // - Add body
        // - Add checksum
        // - try sending

        // - Log ip and icmp packet (preset log levels like doom)

        public byte[] GetBytes()
        {
            byte[] payload = new byte[HeaderLength + Data.Length];
            Buffer.BlockCopy(BitConverter.GetBytes((byte)(Version << 4) | HeaderLength), 0, payload, 0, 1); // TODO: divide length by 4, Shift 4 bits to right, OR (basically copy) the other 4 bits into where the version was
            Buffer.BlockCopy(BitConverter.GetBytes(TypeOfService), 0, payload, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(TotalLength), 0, payload, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Identification), 0, payload, 4, 2);
            byte[] test = BitConverter.GetBytes(FragmentOffset);
            //Buffer.BlockCopy(BitConverter.GetBytes((byte)(Flags << 5) | test[0]), 0, data, );
            
            return payload;
        }

        public UInt16 GetChecksum()
        {
            return 0;
        }

        public string PrettyPrint()
        {
            return $"version={Version.ToString()} HeaderLength={HeaderLength.ToString()} TypeOfService={TypeOfService.ToString()}" +
                $" TotalLength={TotalLength} Identification={Identification} flags={Flags.ToString()} FragmentationOffset={FragmentOffset}" +
                $" TimeToLive={TimeToLive} Protocol={Protocol} HeaderCheckum={HeaderChecksum} ({HeaderChecksum.ToString("X")}) SourceAddress={Source.ToString()} DestinationAddress={Destination.ToString()}";

            // TODO: print raw bytes
        }
    }
}
