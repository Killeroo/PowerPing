using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// For constructing and sending ping ping packets
/// </summary>

namespace PowerPing
{

    class Ping : IDisposable
    {
        // Properties
        public PingResults Results { get; private set; } = new PingResults(); // Store current ping results
        public PingAttributes Attributes { get; private set; } = new PingAttributes(); // Stores the current operation's attributes
        public bool ShowOutput { get; set; } = true;
        public bool ShowRequest { get; set; } = false;
        public bool IsRunning { get; private set; } = false;
        public int Threads { get; set; } = 5;

        private ManualResetEvent cancelEvent = new ManualResetEvent(false);

        public Ping() { }

        /// <summary>
        /// Sends a set of ping packets
        /// </summary>
        public void Send(PingAttributes attrs)
        {
            // Load user inputted attributes
            string inputAddress = attrs.Address; 
            this.Attributes = attrs;

            // Lookup address
            Attributes.Address = PowerPing.Helper.VerifyAddress(Attributes.Address, Attributes.ForceV4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

            if (ShowOutput)
                PowerPing.Display.PingIntroMsg(inputAddress, this);

            // Perform ping operation and store results
            this.SendICMP(Attributes);

            if (ShowOutput)
                PowerPing.Display.PingResults(this);

        }
        /// <summary>
        /// Listen for an ICMPv4 packets
        /// </summary>
        public void Listen()
        {
            IPAddress localAddress = null;
            Socket listeningSocket = null;
            PingResults results = new PingResults();

            // Find local address
            foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    localAddress = ip;

            IsRunning = true;

            
            try
            {
                // Create listener socket
                listeningSocket = CreateRawSocket(AddressFamily.InterNetwork);
                listeningSocket.Bind(new IPEndPoint(localAddress, 0));
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 }); // Set SIO_RCVALL flag to socket IO control

                PowerPing.Display.ListenIntroMsg();

                // Listening loop
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    
                    // Recieve any incoming ICMPv4 packets
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display captured packet
                    PowerPing.Display.CapturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.SetPacketType(response.type);
                    results.Received++;

                    if (cancelEvent.WaitOne(0))
                        break;
                }
            }
            catch (SocketException)
            {
                PowerPing.Display.Error("Could not read packet from socket");
                results.Lost++;
            }
            catch (Exception)
            {
                PowerPing.Display.Error("General exception occured", true);
            }

            // Clean up
            IsRunning = false;
            listeningSocket.Close();

            Display.ListenResults(results);
        }
        /// <summary>
        /// ICMP Traceroute
        /// </summary>
        public void Trace() { }
        /// <summary>
        /// Network scan method 
        /// </summary>
        /// <param name="range"></param>
        public void Scan(string range, bool recursing = false)
        {
            List<string> scanList = new List<string>(); // List of addresses to scan
            String[] ipSegments = range.Split('.');
            PingResults results = new PingResults();
            List<string> activeHosts = new List<string>();
            List<double> activeHostTimes = new List<double>();
            Stopwatch scanTimer = new Stopwatch();
            int scanned = 0;
            //List<string>[] addressLists = new List<string>[Threads]; // Lists of addresses to be scanned in each thread

            // Setup scan ping attributes
            PingAttributes attrs = new PingAttributes();
            attrs.Timeout = 500;
            attrs.Interval = 0;
            attrs.Count = 1;
            ShowOutput = false;

            // Check format of address (for '-'s and disallow multipl '-'s in one segment)
            if (!range.Contains("-"))
                Display.Error("Scan - No range specified, must be specified in format: 192.168.1.1-254", true, true);

            // Holds the ranges for each ip segment
            int[] segLower = new int[4];
            int[] segUpper = new int[4];

            try
            {
                // Work out upper and lower ranges for each segment
                for (int y = 0; y < 4; y++)
                {
                    string[] ranges = ipSegments[y].Split('-');
                    segLower[y] = Convert.ToInt16(ranges[0]);
                    segUpper[y] = (ranges.Length == 1) ? segLower[y] : Convert.ToInt16(ranges[1]);
                }
            }
            catch (FormatException)
            {
                Display.Error("Scan - Incorrect format [" + range + "], must be specified in format: 192.168.1.1-254", true, true);
            }

            // Build list of addresses from ranges
            for (int seg1 = segLower[0]; seg1 <= segUpper[0]; seg1++)
            {
                for (int seg2 = segLower[1]; seg2 <= segUpper[1]; seg2++)
                {
                    for (int seg3 = segLower[2]; seg3 <= segUpper[2]; seg3++)
                    {
                        for (int seg4 = segLower[3]; seg4 <= segUpper[3]; seg4++)
                        {
                            scanList.Add(new IPAddress(new byte[] { (byte)seg1, (byte)seg2, (byte)seg3, (byte)seg4 }).ToString());
                        }
                    }
                }
            }

            //// Divide scanlist into lists for each thread
            //int splitListSize = (int)Math.Ceiling(scanList.Count / (double)Threads);
            //int x = 0;

            //for (int i = 0; i < addressLists.Length; i++)
            //{
            //    addressLists[i] = new List<string>();
            //    for (int j = x; j < x + splitListSize; j++)
            //    {
            //        if (j >= scanList.Count)
            //            break; // Stop if we are out of bounds
            //        addressLists[i].Add(scanList[j]);
            //    }
            //    x += splitListSize;
            //}

            scanTimer.Start();

            // Scan loop
            foreach (string host in scanList)
            {
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

                if (results.Lost == 0 & results.ErrorPackets != 1)
                {
                    // If host is active, add to list
                    activeHosts.Add(host);
                    activeHostTimes.Add(results.CurTime);
                }

            }

            scanTimer.Stop();
            PowerPing.Display.EndScanResults(scanList.Count, activeHosts, activeHostTimes);
        }
        /// <summary>
        /// ICMP flood
        /// </summary>
        public void Flood(string address)
        {
            PingAttributes attrs = new PingAttributes();
            Thread[] floodThreads = new Thread[Threads];
            Ping p = new Ping();

            // Verify address
            attrs.Address = Helper.VerifyAddress(address, AddressFamily.InterNetwork);

            // Setup ping attributes
            attrs.Interval = 0;
            attrs.Timeout = 100;
            attrs.Message = "R U Dead Yet?";
            attrs.Continous = true;

            // Disable output for faster speeds
            p.ShowOutput = false;

            // Start threads
            //for (int i = 0; i < Threads - 1; i++)
            //{
            //    floodThreads[i] = new Thread(() =>
            //    {
            //        Ping p = new Ping();
            //        //p.ShowOutput = false;
            //        p.Send(attrs);
            //    });
            //    floodThreads[i].IsBackground = true;
            //    floodThreads[i].Start();
            //}

            //for (int i = 0; i < Threads - 1; i++)
            //{
            //    floodThreads[i].Abort();
            //}

            // Start flood thread
            var thread = new Thread(() =>
            {
                p.Send(attrs);
            });
            thread.IsBackground = true;
            thread.Start();
            IsRunning = true;

            // Results loop 
            while (IsRunning)
            {
                // Update results text
                Display.FloodProgress(p.Results);

                // Wait before updating (save our CPU load) and check for cancel event
                if (cancelEvent.WaitOne(1000))
                    break;
            }

            // Cleanup
            IsRunning = false;
            p.Dispose();
            thread.Abort();

            // Display results
            Display.PingResults(p);
            
        }

        private Socket CreateRawSocket(AddressFamily family)
        {
            Socket s = null;
            try
            {
                s = new Socket(family, SocketType.Raw, family == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6);
            }
            catch (SocketException)
            {
                PowerPing.Display.Error("Socket cannot be created " + Environment.NewLine + "Please run as Administrator and try again.", true);
            }
            return s;
        }
        private PingResults SendICMP(PingAttributes attrs)
        {
            IPEndPoint iep = null;
            EndPoint ep = null;
            IPAddress ipAddr = null;
            ICMP packet = new ICMP();
            Socket sock = null;
            Stopwatch responseTimer = new Stopwatch();
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
            //sock.Ttl = (short)attributes.ttl;

            // Construct our ICMP packet
            packet.type = attrs.Type;
            packet.code = attrs.Code;
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2); // Add seq num to ICMP message
            byte[] payload = Encoding.ASCII.GetBytes(attrs.Message);
            Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length); // Add text into ICMP message
            packet.messageSize = payload.Length + 4;
            packetSize = packet.messageSize + 4;

            //responseTimer.Start();

            // Sending loop
            while (attrs.Continous ? true : index <= attrs.Count)
            {
                // Exit loop if cancel event is set
                if (cancelEvent.WaitOne(0))
                    break;
                else
                    IsRunning = true;

                // Update ICMP checksum and seq
                packet.checksum = 0;
                Buffer.BlockCopy(BitConverter.GetBytes(index), 0, packet.message, 2, 2); // Include sequence number in ping message
                UInt16 chksm = packet.GetChecksum();
                packet.checksum = chksm;

                try
                {
                    // Show ping request
                    if (ShowRequest)
                        Display.RequestPacket(packet, attrs.Address, index);

                    // Send ping request
                    sock.SendTo(packet.GetBytes(), packetSize, SocketFlags.None, iep); // Packet size = message field + 4 header bytes
                    responseTimer.Restart();
                    Results.Sent++;

                    // Wait for response
                    byte[] buffer = new byte[5096];
                    bytesRead = sock.ReceiveFrom(buffer, ref ep);
                    responseTimer.Stop();

                    // Store reply packet
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display reply packet
                    if (ShowOutput)
                        PowerPing.Display.ReplyPacket(response, ep.ToString(), index, responseTimer.Elapsed, bytesRead);

                    // Store response info
                    Results.SetPacketType(response.type);
                    Results.SetCurResponseTime(responseTimer.Elapsed.TotalMilliseconds);
                    Results.Received++;
                }
                catch (IOException)
                {
                    if (ShowOutput)
                        PowerPing.Display.Error("General transmit error");
                    Results.SetCurResponseTime(-1);
                    Results.Lost++;
                }
                catch (SocketException)
                {
                    if (ShowOutput)
                        PowerPing.Display.PingTimeout(index);
                    Results.SetCurResponseTime(-1);
                    Results.Lost++;
                }
                catch (Exception)
                {
                    if (ShowOutput)
                        PowerPing.Display.Error("General error occured");
                    Results.SetCurResponseTime(-1);
                    Results.Lost++;
                }
                finally
                {
                    // Increment seq and wait for interval
                    index++;
                    cancelEvent.WaitOne(attrs.Interval);

                    // Make sure timer is stopped
                    responseTimer.Stop();
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
            if (!disposedValue)
            {
                if (IsRunning)
                {
                    cancelEvent.Set();

                    // wait till ping stops running
                    while (IsRunning)
                        Task.Delay(25);
                }

                // Reset cancel event
                cancelEvent.Reset();

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
