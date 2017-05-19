using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;

/* Ping Class */
// For constructing and sending ping ping packets

class Ping
{
    // Ping attributes
    public string address { get; set; }
    public string message { get; set; }
    public int interval { get; set; } // Time interval between sending each ping
    public int timeout { get; set; }
    public int count { get; set; }
    public int ttl { get; set; }
    public bool continous { get; set; }
    public bool forceV4 { get; set; }
    public bool forceV6 { get; set; }
    public bool showOutput { get; set; } // Hide output when sending a ping
    public bool isRunning { get { return running; } }
    public TimeSpan getTotalRunTime { get { return totalRunTime.Elapsed; } } // Total amount of time pings have been sending
    public long getLastResponseTime { get { return lastResponseTime; } }
    public int getPacketsSent { get { return sent; } }
    public int getPacketsRecieved { get { return recieved; } }
    public int getPacketsLost { get { return lost; } }
    public long getMaxReplyTime { get { return max; } }
    public long getMinReplyTime { get { return min; } }

    // Timing enumerations
    public enum Timings
    {
        Insane = 0,
        Speedy = 1,
        Nimble = 2,
        Normal = 3,
        Quiet = 4,
        Sneaky = 5,
        Paranoid = 6
    }

    // Result attributes
    private int sent = 0;
    private int recieved = 0;
    private int lost = 0;
    private long max = 0;
    private long min = -1;
    
    // Scan Variables
    public int threads = 5; // Move to global attributes?
    private ConcurrentStack<IPAddress> activeHosts = new ConcurrentStack<IPAddress>();

    // Local variables
    private static Stopwatch responseTimer = new Stopwatch();
    private static Stopwatch totalRunTime = new Stopwatch();
    private bool cancelFlag = false;
    private bool running = false;
    private long lastResponseTime = 0;

    // Constructors
    public Ping()
    {
        // Assign defaul ping values
        address = "127.0.0.1";
        count = 5;
        timeout = 3000;
        ttl = 255;
        interval = 1000;
        continous = false;
        forceV4 = true;
        forceV6 = false;
        showOutput = true;
        message = "R U Alive?";
    }
    public Ping(string addr)
    {
        // Use provided address
        address = addr;

        // Assign default ping values
        count = 5;
        timeout = 3000;
        ttl = 255;
        interval = 1000;
        continous = false;
        forceV4 = true;
        forceV6 = false;
        showOutput = true;
        message = "R U Alive?";
    }

    /// <summary>
    /// Sends a set of ping packets
    /// </summary>
    public void Send() 
    {
        IPEndPoint iep = null;
        EndPoint ep = null;
        IPAddress ipAddr = null;
        ICMP packet = new ICMP();
        Socket sock = null;
        int bytesRead, packetSize, index = 1;

        // Lookup address
        ipAddr = PowerPing.Helper.VerifyAddress(address, forceV4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

        // Setup endpoint
        iep = new IPEndPoint(ipAddr, 0);
        ep = (EndPoint)iep;

        // Setup raw socket 
        sock = CreateRawSocket(ipAddr.AddressFamily);

        // Set socket options
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout); // Timeout
        sock.Ttl = (short)ttl;

        // Construct our ICMP packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2); // Add seq num to ICMP message
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length); // Add text into ICMP message
        packet.messageSize = payload.Length + 4;
        packetSize = packet.messageSize + 4;

        // Start timing ping operation
        totalRunTime.Start();
        if (showOutput)
            PowerPing.Display.PingIntroMsg(ep.ToString(), this); // Display intro message

        // Sending loop
        while (continous ? true : index <= count)
        {
            // Exit loop if cancel flag recieved
            if (cancelFlag) 
                break;
            else
                running = true;

            // Update ICMP checksum and seq
            packet.checksum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, packet.message, 2, 2); // Include sequence number in ping message
            UInt16 chksm = packet.GetChecksum();
            packet.checksum = chksm;

            try
            {
                // Send ping request
                responseTimer.Start();
                sock.SendTo(packet.GetBytes(), packetSize, SocketFlags.None, iep); // Packet size = message field + 4 header bytes
                sent++;

                // Wait for response
                byte[] buffer = new byte[1024];
                bytesRead = sock.ReceiveFrom(buffer, ref ep);
                responseTimer.Stop();

                // Store reply packet
                ICMP response = new ICMP(buffer, bytesRead);

                // Display reply packet
                if (showOutput)
                    PowerPing.Display.ReplyPacket(response, ep.ToString(), index,  responseTimer.ElapsedMilliseconds);

                // Store response time
                lastResponseTime = responseTimer.ElapsedMilliseconds;
                recieved++;

                // Check response time against current max and min
                if (responseTimer.ElapsedMilliseconds > max)
                    max = responseTimer.ElapsedMilliseconds;
                if (responseTimer.ElapsedMilliseconds < min || min == -1)
                    min = responseTimer.ElapsedMilliseconds;

            }
            catch (SocketException)
            {
                if (showOutput)
                    PowerPing.Display.PingTimeout();
                lastResponseTime = 0;
                lost++;
            }
            finally
            {
                responseTimer.Reset();
            }

            index++;
            Thread.Sleep(interval);
        }

        // Stop operation
        running = false;
        sock.Close();
        this.Stop();

        // Display stats
        if (showOutput)
            PowerPing.Display.PingResults(this);
    }
    /// <summary>
    /// Listen for an ICMPv4 packets
    /// </summary>
    public void Listen() 
    {
        IPAddress localAddress = null;
        Socket listeningSocket = null;

        // Check network status
        if (!NetworkInterface.GetIsNetworkAvailable())
            PowerPing.Display.Error("Not connected to network.", true, true);

        // Find local address
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localAddress = ip;

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
            }
        }
        catch (SocketException)
        {
            PowerPing.Display.Error("Socket Error - Error occured while reading from socket\nPlease try again.", true);
        }
        catch (NullReferenceException)
        {
            PowerPing.Display.Error("Error fetching local address, connect to a network and try again.", true);
        }
    }
    /// <summary>
    /// ICMP Traceroute
    /// </summary>
    public void Trace() { }
    /// <summary>
    /// Recursive network scan method 
    /// </summary>
    /// <param name="target"></param>
    public void Scan(string target, bool recursing = false)
    {
        List<IPAddress> scanList = new List<IPAddress>();
        String[] ipSegments = target.Split('.');

        if (!recursing)
        {
            // Setup scan

            // Check format of address (for '-'s and disallow multipl '-'s in one segment, also check format of address


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
                            scanList.Add(new IPAddress(new byte[] { (byte)seg1, (byte)seg2, (byte)seg3, (byte)seg4 }));
                        }
                    }
                }
            }

            // Divide scanlist into lists for each thread
            List<IPAddress>[] threadLists = new List<IPAddress>[threads];
            int splitListSize = (int)Math.Ceiling(scanList.Count / (double)threads);
            int x = 0;

            for (int i = 0; i < threadLists.Length; i++)
            {
                threadLists[i] = new List<IPAddress>();
                for (int j = x; j < x + splitListSize; j++)
                {
                    if (j >= scanList.Count)
                        break; // Stop if we are out of bounds
                    threadLists[i].Add(scanList[j]);
                }
                x += splitListSize;
            }

            // *Bug here*
            // Finally, fire up the threads!
            for (int i = 0; i < threads - 1; i++)
            {
                Thread thread = new Thread(() => ScanThread(threadLists[i]));
                thread.Start();
            }

            // Display results of scan
            PowerPing.Display.ScanResult(scanList.Count, activeHosts.Count);

        }
        else
        {
            // Recursive ping sender
        }
    }
    /// <summary>
    /// ICMP flood
    /// </summary>
    public void Flood() { }
    /// <summary>
    /// Stop any ping operations
    /// </summary>
    public void Stop() 
    {
        // If a ping operation is running send cancel flag
        if (running)
        {
            cancelFlag = true;

            // wait till ping stops running
            while (running)
                Task.Delay(25);
        }

        // Stop counting total elapsed timer
        if (totalRunTime.IsRunning)
            totalRunTime.Stop();

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
    private void ScanThread(List<IPAddress> addressList)
    {
        // Local variable declaration
        int bytesRead, packetSize;
        IPEndPoint iep;
        EndPoint ep;
        Socket scanSocket = null;
        Stopwatch st = new Stopwatch();
        ICMP packet = new ICMP();

        // Setup and configure socket
        scanSocket = CreateRawSocket(AddressFamily.InterNetwork);
        scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);
        scanSocket.Ttl = (short)255;

        // Construct ping packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2); // Add seq num to ICMP message
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length); // Add text into ICMP message
        packet.messageSize = payload.Length + 4;
        packetSize = packet.messageSize + 4;

        // Ping each address on list
        foreach (IPAddress host in addressList)
        {
            // Setup endpoint for current host
            iep = new IPEndPoint(host, 0);
            ep = (EndPoint)iep;

            try
            {
                // Ping host
                st.Start();
                scanSocket.SendTo(packet.GetBytes(), packetSize, SocketFlags.None, ep);
                // Wait for reply
                byte[] buffer = new byte[1024];
                bytesRead = scanSocket.ReceiveFrom(buffer, ref ep);
                st.Stop();

                // Only count hosts that send reply packets
                if (buffer[20] == 0x00)
                {
                    PowerPing.Display.Message("Host " + host + " is active. [Latency " + st.ElapsedMilliseconds + "ms]");
                    activeHosts.Push(host);
                }
            }
            catch (SocketException) { }
            finally
            {
                st.Reset();
            }
        }
    }
}
