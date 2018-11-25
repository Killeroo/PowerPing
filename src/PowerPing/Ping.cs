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
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace PowerPing
{
    /// <summary>
    /// Ping Class, used for constructing and sending ICMP packets.
    /// Also contains other ping-like functions such as flooding, listening
    /// scanning and others.
    /// </summary>
    class Ping : IDisposable
    {
        // Properties
        public PingResults Results { get; private set; } = new PingResults(); // Store current ping results
        public PingAttributes Attributes { get; private set; } = new PingAttributes(); // Stores the current operation's attributes
        public bool IsRunning { get; private set; } = false;

        private static readonly ushort sessionId = Helper.GenerateSessionId();
        private readonly ManualResetEvent cancelEvent = new ManualResetEvent(false);
        private bool debug = false;

        public Ping() { }

        /// <summary>
        /// Sends a set of ping packets, results are stores within
        /// Ping.Results of the called object
        ///
        /// Acts as a basic wrapper to SendICMP and feeds it a specific
        /// set of PingAttributes 
        /// </summary>
        public void Send(PingAttributes attrs)
        {
            // Load user inputted attributes
            this.Attributes = attrs;

            // Lookup address
            Attributes.Address = PowerPing.Lookup.QueryDNS(Attributes.Host, Attributes.ForceV4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

            PowerPing.Display.PingIntroMsg(Attributes.Host, attrs);

            if (Display.UseResolvedAddress) {
                Attributes.Host = PowerPing.Lookup.QueryHost(Attributes.Address);
                if (Attributes.Host == "") {
                    // If reverse lookup fails just display whatever is in the address field
                    Attributes.Host = Attributes.Address; 
                }
            }

            // Perform ping operation and store results
            this.SendICMP(Attributes);

            PowerPing.Display.PingResults(this);

        }
        /// <summary>
        /// Listens for all ICMPv4 activity on localhost.
        ///
        /// Does this by setting a raw socket to SV_IO_ALL which
        /// will recieve all packets and filters to just show
        /// ICMP packets. Runs until ctrl-c or exit
        /// </summary>
        /// <source>https://stackoverflow.com/a/9174392</source>
        public void Listen()
        {
            IPAddress localAddress = null;
            Socket listeningSocket = null;
            PingResults results = new PingResults();

            // Find local address
            localAddress = IPAddress.Parse(PowerPing.Lookup.LocalAddress());

            IsRunning = true;
            try {
                // Create listener socket
                listeningSocket = CreateRawSocket(AddressFamily.InterNetwork);
                listeningSocket.Bind(new IPEndPoint(localAddress, 0));
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 }); // Set SIO_RCVALL flag to socket IO control

                PowerPing.Display.ListenIntroMsg();

                // Listening loop
                while (true) {
                    byte[] buffer = new byte[4096]; // TODO: could cause overflow?
                    
                    // Recieve any incoming ICMPv4 packets
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display captured packet
                    PowerPing.Display.CapturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.CountPacketType(response.type);
                    results.Received++;

                    if (cancelEvent.WaitOne(0)) {
                        break;
                    }
                }
            } catch (SocketException) {
                PowerPing.Display.Error("Could not read packet from socket");
                results.Lost++;
            } catch (Exception) {
                PowerPing.Display.Error("General exception occured", true);
            }

            // Clean up
            IsRunning = false;
            listeningSocket.Close();

            Display.ListenResults(results);
        }
        /// <summary>
        /// ICMP Traceroute
        /// Not implemented yet
        /// </summary>
        public void Trace() { throw new NotSupportedException(); }
        /// <summary>
        /// Network scanning method.
        ///
        /// Uses pings to scan a IP address range and identify hosts
        /// that are active.
        ///
        /// range should be in format 192.0.0.1-255, where - denotes the range
        /// This can be specified at any octlet of the address (192.0.1-100.1.255)
        /// </summary>
        /// <param name="range">Range of addresses to scan</param>
        public void Scan(string range, bool recursing = false)
        {
            List<string> scanList = new List<string>(); // List of addresses to scan
            String[] ipSegments = range.Split('.');
            PingResults results = new PingResults();
            List<string> activeHosts = new List<string>();
            List<double> activeHostTimes = new List<double>();
            Stopwatch scanTimer = new Stopwatch();
            int scanned = 0;
            
            // Setup scan ping attributes
            PingAttributes attrs = new PingAttributes();
            attrs.Timeout = 500;
            attrs.Interval = 0;
            attrs.Count = 1;
            Display.ShowOutput = false;

            // Check format of address (for '-'s and disallow multipl '-'s in one segment)
            if (!range.Contains("-")) {
                Display.Error("Scan - No range specified, must be specified in format: 192.168.1.1-254", true, true);
            }

            // Holds the ranges for each ip segment
            int[] segLower = new int[4];
            int[] segUpper = new int[4];

            // Work out upper and lower ranges for each segment
            try{
                for (int y = 0; y < 4; y++) {
                    string[] ranges = ipSegments[y].Split('-');
                    segLower[y] = Convert.ToInt16(ranges[0]);
                    segUpper[y] = (ranges.Length == 1) ? segLower[y] : Convert.ToInt16(ranges[1]);
                }
            } catch (FormatException) {
                Display.Error("Scan - Incorrect format [" + range + "], must be specified in format: 192.168.1.1-254", true, true);
            }

            // Build list of addresses from ranges
            for (int seg1 = segLower[0]; seg1 <= segUpper[0]; seg1++) {
                for (int seg2 = segLower[1]; seg2 <= segUpper[1]; seg2++) {
                    for (int seg3 = segLower[2]; seg3 <= segUpper[2]; seg3++) {
                        for (int seg4 = segLower[3]; seg4 <= segUpper[3]; seg4++) {
                            scanList.Add(new IPAddress(new byte[] { (byte)seg1, (byte)seg2, (byte)seg3, (byte)seg4 }).ToString());
                        }
                    }
                }
            }

            scanTimer.Start();

            // Scan loop
            foreach (string host in scanList) {
                // Update host
                attrs.Address = host;
                scanned++;

                // Reset global results for accurate results 
                // (results need to be set globally while running for graph but need to be semi local for scan 
                // or results bleed through so active hosts can't be determined)
                this.Results = new PingResults();

                // Send ping
                results = SendICMP(attrs);
                Display.ScanProgress(scanned, activeHosts.Count, scanList.Count, scanTimer.Elapsed, range, attrs.Address);

                if (results.Lost == 0 && results.ErrorPackets != 1) {
                    // If host is active, add to list
                    activeHosts.Add(host);
                    activeHostTimes.Add(results.CurTime);
                }

            }

            scanTimer.Stop();
            PowerPing.Display.ScanResults(scanList.Count, activeHosts, activeHostTimes);
        }
        /// <summary>
        /// Sends high volume of ping packets
        /// </summary>
        public void Flood(string address)
        {
            PingAttributes attrs = new PingAttributes();
            Ping p = new Ping();

            // Verify address
            attrs.Address = PowerPing.Lookup.QueryDNS(address, AddressFamily.InterNetwork);

            // Setup ping attributes
            attrs.Interval = 0;
            attrs.Timeout = 100;
            attrs.Message = "R U Dead Yet?";
            attrs.Continous = true;

            // Disable output for faster speeds
            Display.ShowOutput = false;

            // Start flood thread
            var thread = new Thread(() => {
                p.Send(attrs);
            });
            thread.IsBackground = true;
            thread.Start();
            IsRunning = true;

            // Results loop 
            while (IsRunning) {
                // Update results text
                Display.FloodProgress(p.Results, address);

                // Wait before updating (save our CPU load) and check for cancel event
                if (cancelEvent.WaitOne(1000)) {
                    break;
                }
            }

            // Cleanup
            IsRunning = false;
            p.Dispose();
            thread.Abort();

            // Display results
            Display.PingResults(p);
            
        }

        /// <summary>
        /// Creates a raw socket for ping operations.
        ///
        /// We have to use raw sockets here as we are using our own 
        /// implementation of ICMP and only raw sockets will allow us
        /// to send whatever data we want through it.
        /// 
        /// The downside is this is why we need to run as administrator
        /// but it allows us the greater degree of control over the packets
        /// that we need
        /// </summary>
        /// <param name="family">AddressFamily to use (IP4 or IP6)</param>
        /// <returns>A raw socket</returns>
        private static Socket CreateRawSocket(AddressFamily family)
        {
            Socket s = null;
            try {
                s = new Socket(family, SocketType.Raw, family == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6);
            } catch (SocketException) {
                PowerPing.Display.Error("Socket cannot be created " + Environment.NewLine + "Please run as Administrator and try again.", true);
            }
            return s;
        }
        /// <summary>
        /// Core ICMP sending method (used by all other functions)
        /// Takes a set of attributes, performs operation and returns a set of results.
        ///
        /// Works specifically by creating a raw socket, creating a ICMP object and
        /// other socket properties (timeouts, interval etc) using the 
        /// inputted properties (attrs), then performs ICMP operation 
        /// before cleaning up and returning results.
        ///
        /// NOTE: There is a weird hack here, The set of PingAttributes used are 
        /// those provided in the parameter as opposed to the ones stored
        /// in this class (Ping.Attributes) (similar case with PingResults).
        /// This is due to some functions (flood, scan) requiring to be run on seperate threads
        /// and needing us to pass specific attributes directly to the object
        /// trust me it works (I think..)
        /// </summary>
        /// <param name="attrs">Properties of pings to be sent</param>
        /// <returns>Set of ping results</returns>
        private PingResults SendICMP(PingAttributes attrs)
        {
            IPEndPoint iep = null;
            EndPoint ep = null;
            IPAddress ipAddr = null;
            ICMP packet = new ICMP();
            Socket sock = null;
            byte[] receiveBuffer = new byte[attrs.RecieveBufferSize]; // Ipv4Header.length + IcmpHeader.length + attrs.recievebuffersize
            int bytesRead, packetSize, index = 1;

            // Convert to IPAddress
            ipAddr = IPAddress.Parse(attrs.Address);

            // Setup endpoint
            iep = new IPEndPoint(ipAddr, 0);
            ep = (EndPoint)iep;

            // Setup raw socket 
            sock = CreateRawSocket(ipAddr.AddressFamily);

            // Set socket options
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, attrs.Timeout); // Socket timeout
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, attrs.Ttl);
            sock.DontFragment = attrs.DontFragment;

            // Create packet message payload
            byte[] payload;
            if (attrs.Size != -1) {
                payload = Helper.GenerateByteArray(attrs.Size);
            } else {
                payload = Encoding.ASCII.GetBytes(attrs.Message);
            }

            // Construct our ICMP packet
            packet.type = attrs.Type;
            packet.code = attrs.Code;
            Buffer.BlockCopy(BitConverter.GetBytes(sessionId), 0, packet.message, 0, 2); // Add identifier to ICMP message
            Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length); // Add text into ICMP message
            packet.messageSize = payload.Length + 4;
            packetSize = packet.messageSize + 4;

            // Sending loop
            while (attrs.Continous ? true : index <= attrs.Count) {

                // Exit loop if cancel event is set
                if (cancelEvent.WaitOne(0)) {
                    break;
                } else {
                    IsRunning = true;
                }

                // Include sequence number in ping message
                ushort sequenceNum = (ushort)index;
                Buffer.BlockCopy(BitConverter.GetBytes(sequenceNum), 0, packet.message, 2, 2);

                // Fill ICMP message field
                if (attrs.RandomMsg) {
                    payload = Encoding.ASCII.GetBytes(Helper.RandomString());
                    Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
                }

                // Update packet checksum
                packet.checksum = 0;
                UInt16 chksm = packet.GetChecksum();
                packet.checksum = chksm;

                try {

                    // Show request packet
                    if (Display.ShowRequests) {
                        Display.RequestPacket(packet, Display.UseInputtedAddress | Display.UseResolvedAddress ? attrs.Host : attrs.Address, index);
                    }

                    // If there were extra responses from a prior request, ignore them
                    while (sock.Available != 0) {
                        bytesRead = sock.ReceiveFrom(receiveBuffer, ref ep);
                    }

                    // Send ping request
                    sock.SendTo(packet.GetBytes(), packetSize, SocketFlags.None, iep); // Packet size = message field + 4 header bytes
                    long requestTimestamp = Stopwatch.GetTimestamp();
                    try { Results.Sent++; }
                    catch (OverflowException) { Results.HasOverflowed = true; }
                    
                    if (debug) {
                        // Induce random wait for debugging 
                        Random rnd = new Random();
                        Thread.Sleep(rnd.Next(700));
                        if (rnd.Next(20) == 1) { throw new SocketException(); }
                    }

                    ICMP response;
                    TimeSpan replyTime;
                    do {
                        // Wait for response
                        bytesRead = sock.ReceiveFrom(receiveBuffer, ref ep);
                        replyTime = new TimeSpan(Helper.StopwatchToTimeSpanTicks(Stopwatch.GetTimestamp() - requestTimestamp));

                        // Store reply packet
                        response = new ICMP(receiveBuffer, bytesRead);

                        // Ignore unexpected echo responses
                        if (packet.type == 8 && response.type == 0) {
                            ushort responseSessionId = BitConverter.ToUInt16(response.message, 0);
                            ushort responseSequenceNum = BitConverter.ToUInt16(response.message, 2);
                            if (responseSessionId != sessionId || responseSequenceNum != sequenceNum) {
                                if (replyTime.TotalMilliseconds >= attrs.Timeout) {
                                    throw new SocketException();
                                }
                                response = null;
                            }
                        }
                    } while (response == null);

                    // Display reply packet
                    if (Display.ShowReplies) {
                        PowerPing.Display.ReplyPacket(response, Display.UseInputtedAddress | Display.UseResolvedAddress ? attrs.Host : ep.ToString(), index, replyTime, bytesRead);
                    }

                    // Store response info
                    try { Results.Received++; }
                    catch (OverflowException) { Results.HasOverflowed = true; }
                    Results.CountPacketType(response.type);
                    Results.SaveResponseTime(replyTime.TotalMilliseconds);
                    
		            if (attrs.BeepLevel == 2) {
                        try { Console.Beep(); }
                        catch (Exception) { } // Silently continue if Console.Beep errors
                    }
                } catch (IOException) {

                    if (Display.ShowOutput) {
                        PowerPing.Display.Error("General transmit error");
                    }
                    Results.SaveResponseTime(-1);
                    try { Results.Lost++; }
                    catch (OverflowException) { Results.HasOverflowed = true; }

                } catch (SocketException) {

                    PowerPing.Display.Timeout(index);
		            if (attrs.BeepLevel == 1) {
                        try { Console.Beep(); }
                        catch (Exception) { Results.HasOverflowed = true; }
                    }
                    Results.SaveResponseTime(-1);
                    try { Results.Lost++; }
                    catch (OverflowException) { Results.HasOverflowed = true; }

                } catch (Exception) {

                    if (Display.ShowOutput) {
                        PowerPing.Display.Error("General error occured");
                    }
                    Results.SaveResponseTime(-1);
                    try { Results.Lost++; }
                    catch (OverflowException) { Results.HasOverflowed = true; }

                } finally {
                    // Increment seq and wait for interval
                    index++;
                    
                    // Wait for set interval before sending again
                    cancelEvent.WaitOne(attrs.Interval);

                    // Generate random interval when RandomTimings flag is set
                    if (attrs.RandomTiming) {
                        attrs.Interval = Helper.RandomInt(5000, 100000);
                    }
                }

                // Stop sending if flag is set
                if (!IsRunning) {
                    break;
                }
            }

            // Clean up
            IsRunning = false;
            sock.Close();

            return Results;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (IsRunning) {
                    cancelEvent.Set();
                    IsRunning = false;

                    // wait till ping stops running
                    while (IsRunning) {
                        Task.Delay(25);
                    }
                }

                // Call GC to cleanup
                System.GC.Collect();

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
