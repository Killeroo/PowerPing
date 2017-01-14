using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

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
    public bool forceV4 { get; set; } = false;
    public bool forceV6 { get; set; } = false;
    public bool showOutput { get; set; } = true; // Hide output when sending a ping
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

    // Local variables setup
    private static Stopwatch responseTimer = new Stopwatch();
    private static Stopwatch totalRunTime = new Stopwatch();
    private static Socket sock = null;
    private bool cancelFlag = false;
    private bool running = false;
    private long lastResponseTime = 0;

    // Constructor
    // TODO: Rework constructors, add default values
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
        message = "R U Alive?";
    }

    /// <summary>
    /// Sends a standard set ping packets to an address and waits for a reply
    /// </summary>
    public void Send()
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
        // TODO: Move to own method
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
        packet.messageSize = payload.Length + 4;
        int packetSize = packet.messageSize + 4;

        totalRunTime.Start(); // Start timing ping operation
        if (showOutput)
            PowerPing.Display.displayPingIntroMsg(ep.ToString(), this); // Display intro message

        // Sending loop
        while (continous ? true : index <= count)
        {
            // Calculate packet checksum
            packet.checksum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, packet.message, 2, 2); // Include sequence number in ping message
            UInt16 chksm = packet.getChecksum();
            packet.checksum = chksm;

            // Exit loop if cancel flag recieved
            if (cancelFlag)
                break;
            else
                running = true;

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
                if (showOutput)
                    PowerPing.Display.displayReplyPacket(response, ep.ToString(), index,  responseTimer.ElapsedMilliseconds);

                // Increment number of recieved replys
                recieved++;

                // Store reply time
                lastResponseTime = responseTimer.ElapsedMilliseconds;

                // Check response time against current max and min
                if (responseTimer.ElapsedMilliseconds > max)
                    max = responseTimer.ElapsedMilliseconds;
                if (responseTimer.ElapsedMilliseconds < min || min == -1)
                    min = responseTimer.ElapsedMilliseconds;

            }
            catch (SocketException)
            {
                if (showOutput)
                    PowerPing.Display.displayTimeout();
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
        this.Stop();

        // Display stats
        if (showOutput)
            PowerPing.Display.displayStatistics(this);
    }

    /// <summary>
    /// Stop any ping operations running and release resources
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

        // Close socket
        if (sock != null || sock.IsBound)
            sock.Close();

        // Reset cancel flag
        cancelFlag = false;
    }

    /// <summary>
    /// Listen for an ICMPv4 packets 
    /// </summary>
    public void Listen()
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

    public void Trace() { }

    public void Scan(int depth = 1)
    {
        // Local variable setup
        IPEndPoint iep = null;
        EndPoint ep = null;
        IPAddress curAddress; // Current address being scanned
        ICMP packet = new ICMP();
        String localAddress;
        String[] splitAddress;
        int bytesRead, failed = 0, replied = 0;
        List<IPAddress> activeAddresses = new List<IPAddress>();

        // Setup socket and option
        if (sock == null)
            setupSocket(AddressFamily.InterNetwork);

        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 250); 
        sock.Ttl = (short) 255;

        // Get local address of computer
        localAddress = PowerPing.Macros.GetLocalIPAddress();

        // If local address can't be found, display error and exit
        if (localAddress == null)
            PowerPing.Display.displayError("Local address can't be found, ensure you are connected to a netowkr and try again.", true, true);

        // Split up the address
        splitAddress = localAddress.Split('.');

        // Construct ping packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
        packet.messageSize = payload.Length + 4;
        int packetSize = packet.messageSize + 4;

        for (int outter = 0; outter <= 20; outter++)
        {

            // Scan looping
            // Increment address segment by one each iteration, ping and wait for reply
            for (int x = 0; x <= 255; x++)
            {
                // Build new address to try
                IPAddress.TryParse(splitAddress[0] + "." + splitAddress[1] + "." + outter + "." + x, out curAddress);

                Console.WriteLine("Trying [{0}] . . . ", curAddress.ToString());

                // Set up endpoint for current address
                iep = new IPEndPoint(curAddress, 0);
                ep = (EndPoint)iep;

                try
                {
                    // Ping address
                    sock.SendTo(packet.getBytes(), packetSize, SocketFlags.None, iep);

                    // Wait for reply
                    byte[] buffer = new byte[1024];
                    bytesRead = sock.ReceiveFrom(buffer, ref ep);

                    // Store reply packet
                    ICMP response = new ICMP(buffer, bytesRead);

                    replied++;
                    activeAddresses.Add(curAddress);

                    // Only register response if is a reply packet
                    if (response.type == 0x00)
                    {

                    }
                }
                catch (SocketException)
                {
                    failed++;
                }

                // Delay between iterations
                Thread.Sleep(50);
            }

        }

        Console.WriteLine("Scan of {0} complete", splitAddress[0] + "." + splitAddress[1] + "." + splitAddress[2] + ".###");
        Console.WriteLine("Non-Active Addresses : {0} Active Addresses : {1}", failed, replied);
        Console.WriteLine("Active Addresses : ");
        foreach (var address in activeAddresses)
            Console.WriteLine(" - ", address.ToString());
    }

    public void Flood() { }

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
