/// <summary>
/// Stores ping attributes
/// </summary>
public class PingAttributes
{
    public string address { get; set; } // Address to send the ping to
    public string message { get; set; } // Message to store with in ICMP message field
    public int interval { get; set; } // Time interval between sending each ping
    public int timeout { get; set; } // Recieve timeout (milliseconds)
    public int count { get; set; } // Number of pings to send
    public int ttl { get; set; } // Time to live
    public byte type { get; set; } // ICMP type field value
    public byte code { get; set; } // ICMP code field value
    public bool continous { get; set; } // Continously send pings
    public bool forceV4 { get; set; } // Force use of IPv4
    public bool forceV6 { get; set; } // Force use of IPv6
  //public Timing timeOpt { get; set; } // Timing option

    public PingAttributes()
    {
        // Default attributes
        address = "127.0.0.1";
        message = "R U Alive?";
        interval = 1000;
        timeout = 3000;
        count = 5;
        ttl = 255;
        type = 0x08;
        code = 0x00;
        continous = false;
        forceV4 = true;
        forceV6 = false;
    }
}