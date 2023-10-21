/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    public enum PingOperation
    {
        Normal,
        Flood,
        Listen,
        Scan,
        Graph,
        Location,
        Whoami,
        Whois
    }

    /// <summary>
    /// Stores attributes of a ping operation
    /// </summary>
    public class PingAttributes
    {
        // Properties
        public string InputtedAddress { get; set; }     // Address as it was inputted by user 
        public string ResolvedAddress { get; set; }     // Resolved address (after lookup) the actual address ping is sent to
        public string SourceAddress { get; set; }       // Source address of IP packet (not implemented yet)
        public string Message { get; set; }             // Message to store in ICMP message field
        public int Interval { get; set; }               // Time interval between each ping
        public int Timeout { get; set; }                // Time (in milliseconds) before timeout occurs
        public int Count { get; set; }                  // Number of pings to send
        public int Ttl { get; set; }                    // IP packet Time to live (TTL) value (https://en.wikipedia.org/wiki/Time_to_live)
        public int ReceiveBufferSize { get; set; }      // Size of buffer used when receiving pings
        public int ArtificalMessageSize { get; set; }   // Sets the ICMP message to be a specifed number of empty bytes 
        public byte Type { get; set; }                  // ICMP type field value (https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)
        public byte Code { get; set; }                  // ICMP code field value (https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)

        // Options
        public int BeepMode { get; set; }               // Beep level - 1 to beep on timeout, 2 for beep on reply, 0 for no beep
        public bool Continous { get; set; }             // Option to continously send pings
        public bool UseICMPv4 { get; set; }             // Force use of IPv4/ICMPv4
        public bool UseICMPv6 { get; set; }             // Force use of IPv6/ICMPv6 (not implemented yet)
        public bool RandomMessage { get; set; }         // Fills ICMP message field with random characters    
        public bool DontFragment { get; set; }          // Sets the IP head Don't Fragment flag
        public bool RandomTiming { get; set; }          // Use random send interval between pings
        public bool EnableLogging { get; set; }         // Log results to a file
        public string LogFilePath { get; set; }         // File to log file to

        public PingOperation Operation { get; set; }    // Current ping operation being performed

        public PingAttributes()
        {
            // Analyser complains about these not being null even though they are set in ResetToDefaultValues (sigh...)
            InputtedAddress = string.Empty;
            ResolvedAddress = string.Empty;
            SourceAddress = string.Empty;
            LogFilePath = string.Empty;
            Message = Helper.RandomString(26);

            ResetToDefaultValues();
        }

        public PingAttributes(PingAttributes attributes)
        {
            InputtedAddress = attributes.InputtedAddress;
            ResolvedAddress = attributes.ResolvedAddress;
            SourceAddress = attributes.SourceAddress;
            Message = attributes.Message;
            LogFilePath = attributes.LogFilePath;
            Interval = attributes.Interval;
            Timeout = attributes.Timeout;
            Count = attributes.Count;
            Ttl = attributes.Ttl;
            Type = attributes.Type;
            Code = attributes.Code;
            ArtificalMessageSize = attributes.ArtificalMessageSize;
            BeepMode = attributes.BeepMode;
            ReceiveBufferSize = attributes.ReceiveBufferSize;

            Continous = attributes.Continous;
            UseICMPv4 = attributes.UseICMPv4;
            UseICMPv6 = attributes.UseICMPv6;
            RandomMessage = attributes.RandomMessage;
            DontFragment = attributes.DontFragment;
            RandomTiming = attributes.RandomTiming;
            Operation = attributes.Operation;
        }

        public void ResetToDefaultValues()
        {
            // Default properties
            InputtedAddress = string.Empty;
            ResolvedAddress = string.Empty;
            SourceAddress = string.Empty;
            LogFilePath = string.Empty;
            Message = Helper.RandomString(26);
            Interval = 1000;
            Timeout = 3000;
            Count = 5;
            Ttl = 255;
            Type = 0x08;
            Code = 0x00;
            ArtificalMessageSize = -1;
            BeepMode = 0;
            ReceiveBufferSize = 5096;

            // Default options
            Continous = false;
            UseICMPv4 = true;
            UseICMPv6 = false;
            RandomMessage = false;
            DontFragment = false;
            RandomTiming = false;
            Operation = PingOperation.Normal;
            LogFilePath = string.Empty;
        }
    }

}