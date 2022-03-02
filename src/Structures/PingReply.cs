/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 * ************************************************************************/

using System;
using System.Net;

namespace PowerPing
{
    public struct PingReply
    {
        public ICMP Packet;
        public IPEndPoint Endpoint;
        public DateTime Timestamp;
        public int SequenceNumber;
        public TimeSpan RoundTripTime;
        public int BytesRead;
    }
}
