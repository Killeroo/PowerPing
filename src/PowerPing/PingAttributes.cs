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

namespace PowerPing
{

    /// <summary>
    /// Stores attributes of a ping operation
    /// </summary>
    public class PingAttributes
    {
        // Properties
        public string Host { get; set; } // Name of host being pinged
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
        public bool RandomMsg { get; set; } // Fills ICMP message field with random characters                                      
	    public int BeepLevel { get; set; } // Beep level - 1 to beep on timeout, 2 for beep on reply, 0 for no beep
        public string[] AddressList { get; set; } // Optional attribute: Used when scanning, stores addresses to ping

        public PingAttributes()
        {
            // Default attributes
            Host = "localhost";
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
            RandomMsg = false;
	        BeepLevel = 0;
            AddressList = null;
        }
    }

}