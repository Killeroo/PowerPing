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
        forceV4 = false;
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
        forceV4 = false;
        forceV6 = false;
        showOutput = true;
        message = "R U Alive?";
    }

    public void send() // Sends a set of ping packets 
    {
        IPEndPoint iep = null;
        EndPoint ep = null;
        IPAddress ipAddr = null;
        ICMP packet = new ICMP();
        Socket sock = null;
        int bytesRead, packetSize, index = 1;

        // check address
        ipAddr = lookupAddress(address);

        // Setup endpoint
        iep = new IPEndPoint(ipAddr, 0);
        ep = (EndPoint)iep;

        // Setup socket 
        sock = createSocket(ipAddr.AddressFamily);

        // Set socket options
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout); // Timeout
        sock.Ttl = (short)ttl; // TTL

        // Construct our ICMP packet
        packet = createPacket(0x08, 0x00);
        packetSize = packet.messageSize + 4;

        totalRunTime.Start(); // Start timing ping operation
        if (showOutput)
            PowerPing.Display.pingIntroMsg(ep.ToString(), this); // Display intro message

        // Sending loop
        while (continous ? true : index <= count)
        {
            // Exit loop if cancel flag recieved
            if (cancelFlag) 
                break;
            else
                running = true;

            // Calculate packet checksum
            calPacketChksm(packet);

            // Update packet sequence number
            updatePacketSeq(packet, index);

            try
            {
                // Send ping request
                responseTimer.Start();
                sock.SendTo(packet.getBytes(), packetSize, SocketFlags.None, iep); // Packet size = message field + 4 header bytes
                sent++;

                // Wait for response
                byte[] buffer = new byte[1024];
                bytesRead = sock.ReceiveFrom(buffer, ref ep);
                responseTimer.Stop();

                // Store reply packet
                ICMP response = new ICMP(buffer, bytesRead);

                // Display reply packet
                if (showOutput)
                    PowerPing.Display.replyPacket(response, ep.ToString(), index,  responseTimer.ElapsedMilliseconds);

                // Store response time
                lastResponseTime = responseTimer.ElapsedMilliseconds;
                recieved++;

                // Check response time against current max and min
                if (responseTimer.ElapsedMilliseconds > max)
                    max = responseTimer.ElapsedMilliseconds;
                if (responseTimer.ElapsedMilliseconds < min || min == -1)
                    min = responseTimer.ElapsedMilliseconds;

            }
            catch (SocketException s)
            {
                if (showOutput)
                    PowerPing.Display.pingTimeout();
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
        this.stop();

        // Display stats
        if (showOutput)
            PowerPing.Display.pingResults(this);
    }
    public void listen() // Listen for an ICMPv4 packets 
    {
        IPAddress localAddress = null;
        Socket listeningSocket = null;

        // Check network status
        if (!NetworkInterface.GetIsNetworkAvailable())
            PowerPing.Display.error("Not connected to network.", true, true);

        // Find local address
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localAddress = ip;

        try
        {
            // Create listener socket
            listeningSocket = createSocket(AddressFamily.InterNetwork);
            // Bind socket to local address
            listeningSocket.Bind(new IPEndPoint(localAddress, 0));
            // Set SIO_RCVALL flag to socket IO control
            listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });

            // Display initial message
            PowerPing.Display.listeningIntroMsg();

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
                PowerPing.Display.capturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);
            }
        }
        catch (SocketException)
        {
            PowerPing.Display.error("Socket Error - Error occured while reading from socket\nPlease try again.", true);
        }
        catch (NullReferenceException)
        {
            PowerPing.Display.error("Error fetching local address, connect to a network and try again.", true);
        }
    }
    public void trace() { }
    public void scan(string target) 
    {
        // Local variable setup
        List<IPAddress> scanList = new List<IPAddress>();
        String[] ipSegments = target.Split('.');

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
                        scanList.Add(new IPAddress( new byte[] { (byte)seg1, (byte)seg2, (byte)seg3, (byte)seg4 }));
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
            for (int j = x; j < x + splitListSize; j++) {
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
            Thread thread = new Thread(() => scan_slave(threadLists[i]));
            thread.Start();
        }

        // Display results of scan
        PowerPing.Display.scanResults(scanList.Count, activeHosts.Count);
    }
    public void flood() { }
    public void stop() // Stop any ping operations 
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

    private void scan_slave(List<IPAddress> addressList)
    {
        // Local variable declaration
        int bytesRead;
        IPEndPoint iep;
        EndPoint ep;
        Socket scanSocket = null;
        Stopwatch st = new Stopwatch();
        ICMP packet = new ICMP();

        // Setup and configure socket
        scanSocket = createSocket(AddressFamily.InterNetwork);
        scanSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 500);
        scanSocket.Ttl = (short)255;

        // Construct ping packet
        packet = createPacket(0x08, 0x00, " ");

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
                scanSocket.SendTo(packet.getBytes(), packet.messageSize + 4, SocketFlags.None, ep); 
                // Wait for reply
                byte[] buffer = new byte[1024];
                bytesRead = scanSocket.ReceiveFrom(buffer, ref ep);
                st.Stop();

                // Only count hosts that send reply packets
                if (buffer[20] == 0x00)
                {
                    PowerPing.Display.message("Host " + host + " is active. [Latency " + st.ElapsedMilliseconds + "ms]");
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
    
    private ICMP createPacket(byte type, byte code, string msg = "") 
    {
        ICMP packet = new ICMP();

        // Construct our ICMP packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
        byte[] payload = Encoding.ASCII.GetBytes(msg == "" ? message : msg); // Use message unless msg contains something
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
        packet.messageSize = payload.Length + 4;
        int packetSize = packet.messageSize + 4;

        return packet;
    }
    private UInt16 calPacketChksm(ICMP packet)
    {
        // Calculate packet checksum
        packet.checksum = 0;
        UInt16 chksm = packet.getChecksum();
        return chksm;
    }
    private void updatePacketSeq(ICMP packet, int seq) 
    {
        // update sequence number in ICMP message field
        Buffer.BlockCopy(BitConverter.GetBytes(seq), 0, packet.message, 2, 2); // update sequence number in ICMP message field
    }
    private Socket createSocket(AddressFamily family) 
    {
        Socket s = null;
        try
        {
            s = new Socket(family, SocketType.Raw, family == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6);
        }
        catch (SocketException)
        {
            PowerPing.Display.error("Socket cannot be created\nPlease run as Administrator and try again.", true);
        }
        return s;
    }
    private IPAddress lookupAddress(string address) 
    {
        IPAddress ipAddr = null;
        IPAddress.TryParse(address, out ipAddr); // Parse the address to IPAddress

        try
        {
            // Query DNS for host address
            if (forceV4 || forceV6) // If we are forcing a particular address family
            {
                foreach (IPAddress a in Dns.GetHostEntry(address).AddressList)
                {
                    // Run through addresses until we find one that matches the family we are forcing
                    if (a.AddressFamily == AddressFamily.InterNetwork && forceV4
                        || a.AddressFamily == AddressFamily.InterNetworkV6 && forceV6)
                        ipAddr = a;
                }
            }
            else
            {
                ipAddr = Dns.GetHostAddresses(address)[0];
            }
        }
        catch (SocketException)
        {
            PowerPing.Display.error("PowerPing could not find the host address [" + address + "]\nCheck address and try again.");
        }
        catch (NullReferenceException)
        {
            PowerPing.Display.error("PowerPing could not find the host address [" + address + "]\nCheck address and try again.");
        }

        return ipAddr;
    }
    private IPAddress getLocalAddress(AddressFamily af = AddressFamily.InterNetwork) // Get rid (DUPLICATE IN MACROS)
    {
        IPAddress address = null;
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == af) {
                address = ip;
                break;
            }
        return address;
    }


}
