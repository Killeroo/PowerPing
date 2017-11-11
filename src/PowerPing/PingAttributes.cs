/// <summary>
/// Stores ping attributes
/// </summary>

namespace PowerPing
{

    public class PingAttributes
    {
        // Properties
        public string Address { get; set; } // Address to send the ping to
        public string Message { get; set; } // Message to store with in ICMP message field
        public int Interval { get; set; } // Time interval between sending each ping
        public int Timeout { get; set; } // Recieve timeout (milliseconds)
        public int Count { get; set; } // Number of pings to send
        public int Ttl { get; set; } // Time to live
        public byte Type { get; set; } // ICMP type field value
        public byte Code { get; set; } // ICMP code field value
        public bool Continous { get; set; } // Continously send pings
        public bool ForceV4 { get; set; } // Force use of IPv4
        public bool ForceV6 { get; set; } // Force use of IPv6
        public string[] AddressList { get; set; } // Optional attribute: Used when scanning, stores addresses to ping

        public PingAttributes()
        {
            // Default attributes
            Address = "127.0.0.1";
            Message = "R U Alive?";
            Interval = 1000;
            Timeout = 3000;
            Count = 5;
            Ttl = 255;
            Type = 0x08;
            Code = 0x00;
            Continous = false;
            ForceV4 = true;
            ForceV6 = false;
            AddressList = null;
        }
    }

}