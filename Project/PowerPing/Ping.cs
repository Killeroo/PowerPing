using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Xml;

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
    public bool continous { get; set; }

    // Local variables setup
    private static Stopwatch timer = new Stopwatch();
    private static Socket sock; // Socket to send and recieve pings on
    private static int sent = 0;
    private static int recieved = 0;
    private static int lost = 0;

    // Destination unreachable code values
    private static string[] codeValues = new string[] {"Network unreachable", "Host unreachable", "Protocol unreachable",
                                                        "Port unreachable", "Fragmentation needed & DF flag set", "Source route failed",
                                                        "Destination network unkown", "Destination host unknown", "Source host isolated",
                                                        "Communication with destination network prohibited", "Communication with destination network prohibited",
                                                        "Network unreachable for ICMP", "Host unreachable for ICMP"};

    // Constructor
    public Ping()
    {
        // Attempt to create socket
        try
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
        }
        catch (SocketException)
        {
            Console.WriteLine("Socket cannot be created\nPlease run as Administrator and try again");
            Environment.Exit(0); // Move this to outside ping class?
        }
    }

    /// <summary>
    /// Send ping to address
    /// </summary>
    public void send()
    {
        // Local variable setup
        IPEndPoint iep = null;
        EndPoint ep = null;
        ICMP packet = new ICMP();
        int recv, index = 1;

        // Verify address
        try
        {
            iep = new IPEndPoint(Dns.GetHostAddresses(address)[0], 0);
            ep = (EndPoint)iep;
        }
        catch (SocketException)
        {
            Console.WriteLine("Host address not found");
            return;
        }

        // Set timeout for ping
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);

        // Construct ping packet
        packet.type = 0x08;
        packet.code = 0x00;
        Buffer.BlockCopy(BitConverter.GetBytes(1), 0, packet.message, 0, 2);
        byte[] payload = Encoding.ASCII.GetBytes(message);
        Buffer.BlockCopy(payload, 0, packet.message, 4, payload.Length);
        packet.messageSize = payload.Length + 4;
        int packetSize = packet.messageSize + 4;

        Console.WriteLine("Pinging {0} [{1}] (Packet message \"{2}\"):", address, ep.ToString(), message);
        while (continous ? true : index <= count)
        {
            // Calculate packet checksum
            packet.checksum = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(index), 0, packet.message, 2, 2); // Include sequence number in ping message
            UInt16 chksm = packet.getChecksum();
            packet.checksum = chksm;

            // Send ping request
            timer.Start();
            sock.SendTo(packet.getBytes(), packetSize, SocketFlags.None, iep);
            sent++;

            try
            {
                // Try recieve ping response
                byte[] buffer = new byte[1024];
                recv = sock.ReceiveFrom(buffer, ref ep);
                timer.Stop();

                // Display reply packet
                ICMP response = new ICMP(buffer, recv);
                displayReplyPacket(response, ep, index);
                recieved++;

            }
            catch (SocketException)
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Request timed out.");
                lost++;
            }
            finally
            {
                Console.BackgroundColor = ConsoleColor.Black;
                timer.Reset();
            }

            index++;
            Thread.Sleep(interval);
        }
    }

    public void trace(string addr)
    {

    }

    /// <summary>
    /// Display information about reply ping packet
    /// </summary>
    /// <param name="packet">Repy packet</param>
    /// <param name="ep">Ping reply endpoint object</param>
    /// <param name="index">Sequence number</param>
    private void displayReplyPacket(ICMP packet, EndPoint ep, int index)
    {
        Console.Write("Reply from: {0} ", ep.ToString());
        Console.Write("Seq={0} ", index);
        Console.Write("Type=");
        switch (packet.type)
        {
            case 0:
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.Write("ECHO REPLY");
                break;
            case 8:
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.Write("ECHO REQUEST");
                break;
            case 3:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write(packet.code > 13 ? "DESTINATION UNREACHABLE" : codeValues[packet.code].ToUpper());
                break;
            case 4:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("SOURCE QUENCH");
                break;
            case 5:
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                Console.Write("PING REDIRECT");
                break;
            case 11:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("TIME EXCEEDED");
                break;
            case 12:
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("PARAMETER PROBLEM");
                break;
            case 13:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("TIMESTAMP REQUEST");
                break;
            case 14:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("TIMESTAMP REPLY");
                break;
            case 15:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("INFORMATION REQUEST");
                break;
            case 16:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("INFORMATION REPLY");
                break;
            default:
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.Write("UNKNOWN TYPE");
                break;
        }
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(" time=");
        if (timer.ElapsedMilliseconds <= 100L)
            Console.ForegroundColor = ConsoleColor.Green;
        else if (timer.ElapsedMilliseconds <= 500L)
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (timer.ElapsedMilliseconds > 500L)
            Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("{0}ms", timer.ElapsedMilliseconds < 1 ? "<1" : timer.ElapsedMilliseconds.ToString());
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine();
    }

    /// <summary>
    /// Displays statistics of current ping task
    /// </summary>
    public void displayStatistics()
    {
        double percent = Math.Round((double)((lost / sent) * 100), 1);
        Console.WriteLine("\nPing statistics for {0}:", address);
        Console.WriteLine("     Packet: Sent = " + sent + ", Recieved = " + recieved + ", Lost = " + lost + " (" + percent + "% loss})");
        Console.WriteLine();
    }
    
}
