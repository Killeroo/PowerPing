/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2023 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 * ************************************************************************/

using System;
using System.Net;

namespace PowerPing
{
    public struct PingRequest
    {
        public IPEndPoint Destination;
        public ICMP Packet;
        public DateTime Timestamp;
        public int SequenceNumber;
        public int PacketSize;
    }
}
