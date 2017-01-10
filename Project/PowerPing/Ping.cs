using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

/* Ping Class */
// For constructing and sending ping ping packets

class Ping
{
    // Ping attributes
    public string address { get; set; }
    public string message { get; set; }
    public int interval { get; set; } // Time interval between each ping
    public int timeout { get; set; }
    public int count { get; set; }
    public int ttl { get; set; }
    public bool continous { get; set; }
    public bool forceV4 = false;
    public bool forceV6 = false;
    public bool isRunning = false; // Is Ping currently sending?
    public bool noSendOutput = false; // Hide out put when sending a ping
    public TimeSpan getTotalRunTime { get { return overallTimer.Elapsed; } } // Get the total amount of time spent on current ping task
    public long getCurrentResponseTime { get { return responseTimer.ElapsedMilliseconds; } } // Response time of most recent ping
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

    // Local variables setup
    private static Stopwatch responseTimer = new Stopwatch();
    private static Stopwatch overallTimer = new Stopwatch();
    private static Socket sock = null;
    private bool cancelFlag = false;

    // Constructor
    public Ping() { }

    /// <summary>
    /// Send ping to address
    /// </summary>
    public void send()
    {
        // Local variable setup
        IPEndPoint iep = null;
        EndPoint ep = null;
        IPAddress ipAddr = null;
        ICMP packet = new ICMP();
        int bytesRead, index = 1;

        // Verify address
        ipAddr = verifyAddress(address);

        // Setup ping endpoint
        iep = new IPEndPoint(ipAddr, 0);
        ep = (EndPoint)iep;

        // Setup socket 
        if (sock == null)
            setupSocket(ipAddr.AddressFamily);

        // Set socket options
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout); // Timeout
        sock.Ttl = (short)ttl; // TTL

        // Construct ping packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
        packet.messageSize = payload.Length + 4;
        int packetSize = packet.messageSize + 4;

        overallTimer.Start(); // Start timing ping operation
        PowerPing.Display.displayPingIntroMsg(ep.ToString(), this); // Display intro message

        // Sending loop
        while (continous ? true : index <= count)
        {
            // Calculate packet checksum
            packet.checksum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, packet.message, 2, 2); // Include sequence number in ping message
            UInt16 chksm = packet.getChecksum();
            packet.checksum = chksm;

            // Exit if cancel flag recieved
            if (cancelFlag)
            {
                isRunning = false;
                return;
            }
            else
            {
                isRunning = true;
            }

            try
            {
                // Send ping request
                responseTimer.Start();
                sock.SendTo(packet.getBytes(), packetSize, SocketFlags.None, iep);
                sent++;

                // Try recieve ping response
                byte[] buffer = new byte[1024];
                bytesRead = sock.ReceiveFrom(buffer, ref ep);
                responseTimer.Stop();

                // Store reply packet
                ICMP response = new ICMP(buffer, bytesRead);

                // Display reply packet
                PowerPing.Display.displayReplyPacket(response, ep.ToString(), index,  responseTimer.ElapsedMilliseconds);

                // Increment number of recieved replys
                recieved++;

                // Check response time against current max and min
                if (responseTimer.ElapsedMilliseconds > max)
                    max = responseTimer.ElapsedMilliseconds;
                if (responseTimer.ElapsedMilliseconds < min || min == -1)
                    min = responseTimer.ElapsedMilliseconds;

            }
            catch (SocketException)
            {
                PowerPing.Display.displayTimeout();
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
        this.stop();

    }

    /// <summary>
    /// Stop any ping operations running and release resources
    /// </summary>
    public void stop()
    {
        // If a ping operation is running send cancel flag
        if (isRunning)
        {
            cancelFlag = true;

            // wait till ping stops running
            while (isRunning)
                Task.Delay(25);
        }

        // Stop counting total elapsed timer
        overallTimer.Stop();

        // Close socket
        sock.Close();

        // Reset cancel flag
        cancelFlag = false;
    }

    /// <summary>
    /// Listen for an ICMP packets 
    /// </summary>
    public void listen()
    {
        IPAddress localAddress = null;

        // Find local address
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                localAddress = ip;

        try
        {
            // Create listener socket
            setupSocket(AddressFamily.InterNetwork);
            // Bind socket to local address
            sock.Bind(new IPEndPoint(localAddress, 0));
            // Set SIO_RCVALL flag to socket IO control
            sock.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 });

            // Display initial message
            PowerPing.Display.displayListeningIntroMsg();

            // Listening loop
            while (true)
            {
                byte[] buffer = new byte[4096];
                // Endpoint for storing source address
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                // Recieve any incoming ICMPv4 packets
                int bytesRead = sock.ReceiveFrom(buffer, ref remoteEndPoint);
                // Create ICMP object of response
                ICMP response = new ICMP(buffer, bytesRead);

                // Display captured packet
                PowerPing.Display.displayCapturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);
            }
        }
        catch (SocketException)
        {
            PowerPing.Display.displayError("Socket Error - Error occured while reading from socket\nPlease try again.", true);
        }
        catch (NullReferenceException)
        {
            PowerPing.Display.displayError("Error fetching local address, connect to a network and try again.", true);
        }
    }

    public void trace() { }

    public void scan() { }

    public void flood() { }

    private void setupSocket(AddressFamily family)
    {
        // Create socket
        try
        {
            sock = new Socket(family, SocketType.Raw, family == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6);
        }
        catch (SocketException)
        {
            PowerPing.Display.displayError("Socket cannot be created\nPlease run as Administrator and try again.", true);
        }
    }

    private IPAddress verifyAddress(string address)
    {
        IPAddress ipAddr = null;

        // Parse the address to IPAddress
        IPAddress.TryParse(address, out ipAddr);

        // Lookup address
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
            PowerPing.Display.displayError("PowerPing could not find the host address [" + address + "]\nCheck address and try again.");
        }
        catch (NullReferenceException)
        {
            PowerPing.Display.displayError("PowerPing could not find the host address [" + address + "]\nCheck address and try again.");
        }

        return ipAddr;

    }
    
}
