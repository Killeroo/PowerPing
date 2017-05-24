using System;
using System.Reflection;

/// <summary>
///  Responsible for displaying ping results, information and other output (designed for console) 
/// </summary>

namespace PowerPing
{

    class Display
    {
        private static ConsoleColor[] typeColors = new ConsoleColor[] {ConsoleColor.DarkGreen, ConsoleColor.Black, ConsoleColor.Black,
                                                                       ConsoleColor.DarkRed, ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue, ConsoleColor.Black,
                                                                       ConsoleColor.Black, ConsoleColor.DarkYellow, ConsoleColor.DarkYellow, ConsoleColor.DarkRed,
                                                                       ConsoleColor.DarkRed, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue,
                                                                       ConsoleColor.DarkBlue};

        // Type code values
        private static string[] destUnreachableCodeValues = new string[] {"Network unreachable", "Host unreachable", "Protocol unreachable",
                                                                          "Port unreachable", "Fragmentation needed & DF flag set", "Source route failed",
                                                                          "Destination network unkown", "Destination host unknown", "Source host isolated",
                                                                          "Communication with destination network prohibited", "Communication with destination network prohibited",
                                                                          "Network unreachable for ICMP", "Host unreachable for ICMP"};
        private static string[] redirectCodeValues = new string[] {"Packet redirected for the network", "Packet redirected for the host",
                                                                   "Packet redirected for the ToS & network", "Packet redirected for the ToS & host"};
        private static string[] timeExceedCodeValues = new string[] { "TTL expired in transit", "Fragment reassembly time exceeded" };
        private static string[] badParameterCodeValues = new string[] { "IP header pointer indicates error", "IP header missing an option", "Bad IP header length" };

        /// <summary>
        /// Displays help message
        /// </summary>
        public static void Help()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string version = Assembly.GetExecutingAssembly().GetName().Name + " Version " + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ")";
            Console.WriteLine(version);
            Console.WriteLine("\nDescription:");
            Console.WriteLine("     This advanced ping utility provides geoip querying, ICMP packet info");
            Console.WriteLine("     and result colourization.");
            Console.WriteLine("\nUsage: PowerPing [--?] | [--whoami] | [--location address] | [--listen] |");
            Console.WriteLine("                 [--graph address] | [--t] [--c count] [--w timeout] ");
            Console.WriteLine("                 [--m message] [--i TTL] [--in interval] [--pt type]");
            Console.WriteLine("                 [--pc code] [--4] target_name");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("     --?             Displays this help message");
            Console.WriteLine("     --t             Ping the target until stopped (Control-C to stop)");
            Console.WriteLine("     --c count       Number of pings to send");
            Console.WriteLine("     --w timeout     Time to wait for reply (in milliseconds)");
            Console.WriteLine("     --m message     Ping packet message");
            Console.WriteLine("     --i ttl         Time To Live");
            Console.WriteLine("     --in interval   Interval between each ping (in milliseconds)");
            Console.WriteLine("     --pt type       Use custom ICMP type");
            Console.WriteLine("     --pc code       Use custom ICMP code value");
            Console.WriteLine("     --4             Force using IPv4");
            //Console.WriteLine("     --6             Force using IPv6");
            Console.WriteLine();
            Console.WriteLine("     --whoami        Location info for current host");
            Console.WriteLine("     --location addr Location info for an address");
            Console.WriteLine();
            Console.WriteLine("     --listen        Listen for ICMP packets");
            Console.WriteLine();
            Console.WriteLine("     --graph addrs   Ping an address, display results in graph view");
            Console.WriteLine();
            Console.WriteLine("\nWritten by Matthew Carney [matthewcarney64@gmail.com] =^-^=");
            Console.WriteLine("Find the project here [https://github.com/Killeroo/PowerPing]\n");
            PowerPing.Helper.Pause();
        }
        /// <summary>
        /// Display Initial ping message to screen, declaring simple info about the ping
        /// </summary>
        /// <param name="host">Resolved host address</param>
        /// <param name="ping">Ping object</param>
        public static void PingIntroMsg(String host, Ping ping)
        {
            // Load ping attributes
            PingAttributes attrs = ping.Attributes;

            Console.Write("\nPinging {0} ", host);

            // Only show resolved address if inputted address and resolved address are different
            if (!String.Equals(host, attrs.Address)) 
                Console.Write("[{0}] ", attrs.Address);

            Console.WriteLine("(Packet message \"{0}\") [Type={1} Code={2}] [TTL={3}]:", attrs.Message, attrs.Type, attrs.Code, attrs.Ttl);
        }
        /// <summary>
        /// Display initial listening message
        /// </summary>
        public static void ListenIntroMsg()
        {
            Console.WriteLine("Listening for ICMP Packets . . .");
        }
        /// <summary>
        /// Display information about reply ping packet
        /// </summary>
        /// <param name="packet">Reply packet</param>
        /// <param name="address">Reply address</param>
        /// <param name="index">Sequence number</param>
        /// <param name="replyTime">Time taken before reply recieved in milliseconds</param>
        public static void ReplyPacket(ICMP packet, String address, int index, long replyTime)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("Reply from: {0} ", address);
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
                    Console.Write(packet.code > 13 ? "DESTINATION UNREACHABLE" : destUnreachableCodeValues[packet.code].ToUpper());
                    break;
                case 4:
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("SOURCE QUENCH");
                    break;
                case 5:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write(packet.code > 3 ? "PING REDIRECT" : redirectCodeValues[packet.code].ToUpper());
                    break;
                case 9:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("ROUTER ADVERTISEMENT");
                    break;
                case 10:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("ROUTER SOLICITATION");
                    break;
                case 11:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(packet.code > 1 ? "TIME EXCEEDED" : timeExceedCodeValues[packet.code].ToUpper());
                    break;
                case 12:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(packet.code > 2 ? "PARAMETER PROBLEM" : badParameterCodeValues[packet.code].ToUpper());
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
            if (replyTime <= 100L)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (replyTime <= 500L)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (replyTime > 500L)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0}ms ", replyTime < 1 ? "<1" : replyTime.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }
        /// <summary>
        /// Display information about a captured packet
        /// </summary>
        public static void CapturedPacket(ICMP packet, String address, String timeRecieved, int bytesRead)
        {
            // Display captured packet
            Console.BackgroundColor = packet.type > 15 ? ConsoleColor.Black : typeColors[packet.type];
            Console.ForegroundColor = packet.type < 16 ? ConsoleColor.Black : ConsoleColor.Gray;
            Console.WriteLine("{0}: ICMPv4: {1} bytes from {2} [type {3}] [code {4}]", timeRecieved, bytesRead, address, packet.type, packet.code);

            // Reset console colours
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        /// <summary>
        /// Display results of scan
        /// </summary>
        public static void ScanResult(int scanned, int found)
        {
            Console.WriteLine("Scan complete. {0} addresses scanned. {1} hosts active.", scanned, found);
            PowerPing.Helper.Pause();
        }
        /// <summary>
        /// Displays statistics for a ping object
        /// </summary>
        /// <param name="ping"> </param>
        public static void PingResults(Ping ping)
        {
            // Load attributes
            PingAttributes attrs = ping.Attributes;
            PingResults results = ping.Results;

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;

            // Display stats
            double percent = (double)results.Lost / results.Sent;
            percent = Math.Round(percent * 100, 1);
            Console.WriteLine("\nPing statistics for {0}:", attrs.Address);

            Console.Write("     Packet: Sent ");
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[ " + results.Sent + " ]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(", Recieved ");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[ " + results.Recieved + " ]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(", Lost ");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[ " + results.Lost + " ]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" (" + percent + "% loss)");

            Console.WriteLine("Response times:");
            Console.WriteLine("     Minimum [ {0}ms ], Maximum [ {1}ms ]", results.MinTime, results.MaxTime);

            Console.WriteLine("Total elapsed time (HH:MM:SS.FFF): {0:hh\\:mm\\:ss\\.fff}", results.TotalRunTime);
            Console.WriteLine();

            // Confirm to exit
            PowerPing.Helper.Pause(true);
        }
        /// <summary>
        /// Display Timeout message
        /// </summary>
        public static void PingTimeout()
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("Request timed out.");
            // Make double sure we dont get the red line bug
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();
        }
        /// <summary>
        /// Display error message
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        /// <param name="exit">Whether to exit program after displaying error</param>
        public static void Error(String errorMessage, bool exit = false, bool pause = false)
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;

            // Write error message
            Console.WriteLine("ERROR: " + errorMessage);

            // Reset console colours
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

            if (pause)
                PowerPing.Helper.Pause();

            if (exit)
                Environment.Exit(0);
        }
        /// <summary>
        /// Display a general message
        /// </summary>
        public static void Message(String message, ConsoleColor color = ConsoleColor.DarkGray)
        {
            Console.BackgroundColor = color;
            Console.WriteLine(message);
        }
    }
}
