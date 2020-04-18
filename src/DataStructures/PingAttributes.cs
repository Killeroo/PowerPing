/*
MIT License - PowerPing 

Copyright (c) 2020 Matthew Carney

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

namespace PowerPing
{
    public enum PingOperation
    {
        Normal,
        Flood,
        Listen,
        Scan,
        Graph,
        CompactGraph,
        Location,
        Whoami,
        Whois
    }

    /// <summary>
    /// Stores attributes of a ping operation
    /// </summary>
    public class PingAttributes
    {
        public string InputtedAddress { get; set; }     // Address as it was inputted by user 
        public string Address { get; set; }     // Resolved address (after lookup) the actual address ping is sent to
        public string Message { get; set; }             // Message to store with in ICMP message field
        public string SourceAddress { get; set; }       // (NOT USED)
        public int Interval { get; set; }               // Time interval between each ping
        public int Timeout { get; set; }                // Time (in milliseconds) before timeout occurs
        public int Count { get; set; }                  // Number of pings to send
        public int Ttl { get; set; }                    // IP packet Time to live (TTL) value (https://en.wikipedia.org/wiki/Time_to_live)
        public int BeepLevel { get; set; }              // Beep level - 1 to beep on timeout, 2 for beep on reply, 0 for no beep
        public int RecieveBufferSize { get; set; }
        public int Size { get; set; }                   // Size of packet to send
        public byte Type { get; set; }                  // ICMP type field value (https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)
        public byte Code { get; set; }                  // ICMP code field value (https://en.wikipedia.org/wiki/Internet_Control_Message_Protocol#Control_messages)
        public bool Continous { get; set; }             // Option to continously send pings
        public bool ForceV4 { get; set; }               // Force use of IPv4
        public bool ForceV6 { get; set; }               // Force use of IPv6 
        public bool RandomMsg { get; set; }             // Fills ICMP message field with random characters    
        public bool DontFragment { get; set; }          // Sets the IP head Don't Fragment flag
        public bool RandomTiming { get; set; }          // Use random send interval between pingss
        public PingOperation Operation { get; set; }    // Current ping operation being performed

        public PingAttributes()
        {
            // Default attributes
            InputtedAddress = "";
            Address = "";
            Message = "abcdefghijklmnopqrstuvyxyz";
            Interval = 1000;
            Timeout = 3000;
            Count = 5;
            Ttl = 255;
            Type = 0x08;
            Code = 0x00;
            Size = -1;
            Continous = false;
            ForceV4 = true;
            ForceV6 = false;
            RandomMsg = false;
            BeepLevel = 0;
            RecieveBufferSize = 5096;
            DontFragment = false;
            RandomTiming = false;
            Operation = PingOperation.Normal;
        }
    }

}