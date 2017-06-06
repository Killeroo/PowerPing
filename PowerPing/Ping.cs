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

    class Ping
    {
        // Properties
        public PingResults Results { get; private set; } = new PingResults(); // Store current ping results
        public PingAttributes Attributes { get; private set; } = new PingAttributes(); // Stores the current operation's attributes
        public bool ShowOutput { get; set; } = true;
        public bool IsRunning { get; private set; } = false;
        public int Threads { get; set; } = 5;

        // Local variables
        private bool cancelFlag = false;

        // Constructor
        public Ping() { }

        /// <summary>
        /// Sends a set of ping packets
        /// </summary>
        public void Send(PingAttributes attrs)
        {
            // Get inputted address
            string inputAddress = attrs.Address; 

            // Load user inputted attributes
            this.Attributes = attrs;

            // Lookup address
            Attributes.Address = PowerPing.Helper.VerifyAddress(Attributes.Address, Attributes.ForceV4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

            // Display intro message
            if (ShowOutput)
                PowerPing.Display.PingIntroMsg(inputAddress, this);

            // Perform ping operation
            this.SendICMP(Attributes);

            // Display stats
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
                // Bind socket to local address
                listeningSocket.Bind(new IPEndPoint(localAddress, 0));
                // Set SIO_RCVALL flag to socket IO control
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });

                // Display initial message
                PowerPing.Display.ListenIntroMsg();

                // Listening loop
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    // Endpoint for storing source address
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    // Recieve any incoming ICMPv4 packets
                    int bytesRead = listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    // Create ICMP object of response
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display captured packet
                    PowerPing.Display.CapturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.SetPacketType(response.type);
                    results.Recieved++;

                    if (cancelFlag)
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

            // Display results
            Display.ListenResults(results);
        }
        /// <summary>
        /// ICMP Traceroute
        /// </summary>
        public void Trace() { }
        /// <summary>
        /// Recursive network scan method 
        /// </summary>
        /// <param name="range"></param>
        public void Scan(string range, bool recursing = false)
        {
            List<string> scanList = new List<string>(); // List of addresses to scan
            String[] ipSegments = range.Split('.');
            List<string>[] addressLists = new List<string>[Threads]; // Lists of addresses to be scanned in each thread
            PingAttributes attrs = new PingAttributes();

            // Check format of address (for '-'s and disallow multipl '-'s in one segment, also check format of address
            if (!range.Contains("-"))
                Display.Error("Scan - No range specified, must be specified in format: 192.168.1.1-254", true, true);

            // Holds the ranges for each ip segment
            int[] segLower = new int[4];
            int[] segUpper = new int[4];

            // Work out upper and lower ranges for each segment
            for (int y = 0; y < 4; y++)
            {
                string[] ranges = ipSegments[y].Split('-');
                segLower[y] = Convert.ToInt16(ranges[0]);
                segUpper[y] = (ranges.Length == 1) ? segLower[y] : Convert.ToInt16(ranges[1]);
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

            // Divide scanlist into lists for each thread
            int splitListSize = (int)Math.Ceiling(scanList.Count / (double)Threads);
            int x = 0;

            for (int i = 0; i < addressLists.Length; i++)
            {
                addressLists[i] = new List<string>();
                for (int j = x; j < x + splitListSize; j++)
                {
                    if (j >= scanList.Count)
                        break; // Stop if we are out of bounds
                    addressLists[i].Add(scanList[j]);
                }
                x += splitListSize;
            }

            // Setup scan ping attributes
            attrs.Timeout = 500;
            attrs.Interval = 0;
            attrs.OpType = OperationTypes.Scanning;
            
            // Finally, fire up the threads!
            for (int i = 0; i < Threads - 1; i++)
            {
                Thread thread = new Thread(() =>
                {
                    attrs.AddressList = addressLists[i].ToArray();
                    Ping p = new Ping();
                    p.Send(attrs);
                });
                thread.IsBackground = true;
                thread.Start();
            }

            // Display results of scan
            //PowerPing.Display.ScanResult(scanList.Count, activeHosts.Count);
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
            attrs.OpType = OperationTypes.Flooding;
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
                Display.FloodResults(p.Results);

                // Check for exit flag
                if (this.cancelFlag)
                    break;

                // Wait before updating (save our CPU load)
                Thread.Sleep(1000);
            }

            // Cleanup
            IsRunning = false;
            p.Stop();
            thread.Abort();

            // Display results
            Display.PingResults(p);
            
        }
        /// <summary>
        /// Stop any ping operations
        /// </summary>
        public void Stop()
        {
            // If a ping operation is running send cancel flag
            if (IsRunning)
            {
                cancelFlag = true;

                // wait till ping stops running
                while (IsRunning)
                    Task.Delay(25);
            }

            // Reset cancel flag
            cancelFlag = false;
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
                PowerPing.Display.Error("Socket cannot be created\nPlease run as Administrator and try again.", true);
            }
            return s;
        }
        private void SendICMP(PingAttributes attrs)
        {
            IPEndPoint iep = null;
            EndPoint ep = null;
            IPAddress ipAddr = null;
            ICMP packet = new ICMP();
            Socket sock = null;
            Stopwatch responseTimer = new Stopwatch();
            int bytesRead, packetSize, index = 1;

            // Convert to IPAddress
            ipAddr = IPAddress.Parse(Attributes.Address);

            // Setup endpoint
            iep = new IPEndPoint(ipAddr, 0);
            ep = (EndPoint)iep;

            // Setup raw socket 
            sock = CreateRawSocket(ipAddr.AddressFamily);

            // Set socket options
            sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Attributes.Timeout); // Socket timeout
            sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, Attributes.Ttl);
            //sock.Ttl = (short)attributes.ttl;

            // Construct our ICMP packet
            packet.type = Attributes.Type;
            packet.code = Attributes.Code;
            Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2); // Add seq num to ICMP message
            byte[] payload = Encoding.ASCII.GetBytes(Attributes.Message);
            Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length); // Add text into ICMP message
            packet.messageSize = payload.Length + 4;
            packetSize = packet.messageSize + 4;

            responseTimer.Start();

            // Sending loop
            while (Attributes.Continous ? true : index <= Attributes.Count)
            {
                // Exit loop if cancel flag recieved
                if (cancelFlag)
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
                    // Send ping request
                    sock.SendTo(packet.GetBytes(), packetSize, SocketFlags.None, iep); // Packet size = message field + 4 header bytes
                    Results.Sent++;

                    // Wait for response
                    byte[] buffer = new byte[5096];
                    bytesRead = sock.ReceiveFrom(buffer, ref ep);
                    responseTimer.Stop();

                    // Store reply packet
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display reply packet
                    if (ShowOutput)
                        PowerPing.Display.ReplyPacket(response, ep.ToString(), index, responseTimer.ElapsedMilliseconds, bytesRead);

                    // Store response info
                    Results.SetPacketType(response.type);
                    Results.SetCurResponseTime(responseTimer.ElapsedMilliseconds);
                    Results.Recieved++;
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
                        PowerPing.Display.PingTimeout();
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
                    Thread.Sleep(Attributes.Interval);

                    responseTimer.Restart();
                }  
            }

            // Clean up
            IsRunning = false;
            sock.Close();
        }
    }

}
