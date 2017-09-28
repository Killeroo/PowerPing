using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

/// <summary>
///  Responsible for displaying ping results, information and other output (designed for console) 
/// </summary>

namespace PowerPing
{

    class Display
    {
        // Properties
        public static bool Short = false;
        public static bool NoColor = false;
        public static bool DisplayMessage = false;
        public static bool TimeStamp = false;
        public static ConsoleColor DefaultForegroundColor;
        public static ConsoleColor DefaultBackgroundColor;

        // Stores console cursor position, used for updating text at position
        private struct CursorPosition 
        {
            public int Left;
            public int Top;

            public CursorPosition(int l, int t)
            {
                Left = l;
                Top = t;
            }

            public void SetToPosition()
            {
                Console.CursorLeft = Left;
                Console.CursorTop = Top;
            }
        };

        // ICMP types and colour values
        private static string[] packetTypes = new string[] {"ECHO REPLY", "UNASSIGNED", "UNASSIGNED", "DESTINATION UNREACHABLE", "SOURCE QUENCH (DEP)", "PING REDIRECT",
                                                            "ALTERNATE HOST ADDRESS (DEP)", "UNASSIGNED", "ECHO REQUEST", "ROUTER ADVERTISEMENT", "ROUTER SOLICITATION",
                                                            "TIME EXCEEDED", "PARAMETER PROBLEM", "TIMESTAMP REQUEST", "TIMESTAMP REPLY", "INFORMATION REQUEST (DEP)",
                                                            "INFORMATION REPLY (DEP)", "ADDRESS MASK REQUEST (DEP)", "ADDRESS MASK REPLY (DEP)", "RESERVED FOR SECURITY",
                                                            "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT",
                                                            "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT",
                                                            "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT", "RESERVED FOR ROBUSTNESS EXPERIMENT",
                                                            "TRACEROUTE (DEP)", "DATAGRAM CONVERSATION ERROR (DEP)", "MOBILE HOST REDIRECT (DEP)", "IPv6 WHERE-ARE-YOU (DEP)",
                                                            "IPv6 HERE-I-AM (DEP)", "MOBILE REGISTRATION REQUEST (DEP)", "MOBILE REGISTRATION REPLY (DEP)", "DOMAIN NAME REQUEST (DEP)",
                                                            "DOMAIN NAME REPLY (DEP)", "SKIP ALGORITHM DISCOVERY PROTOCOL (DEP)", "PHOTURIS PROTOCOL SECURITY FAILURES",
                                                            "EXPERIMENTAL"};
        private static ConsoleColor[] typeColors = new ConsoleColor[] { ConsoleColor.DarkGreen, ConsoleColor.Black, ConsoleColor.Black, ConsoleColor.DarkRed, 
                                                                        ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue, ConsoleColor.DarkMagenta, ConsoleColor.Black,
                                                                        ConsoleColor.DarkYellow, ConsoleColor.DarkCyan, ConsoleColor.DarkCyan, ConsoleColor.DarkRed,
                                                                        ConsoleColor.DarkRed, ConsoleColor.DarkBlue, ConsoleColor.DarkBlue,ConsoleColor.DarkMagenta,
                                                                        ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.White,
                                                                        ConsoleColor.White, ConsoleColor.White, ConsoleColor.White, ConsoleColor.White,
                                                                        ConsoleColor.White, ConsoleColor.White, ConsoleColor.White, ConsoleColor.White,
                                                                        ConsoleColor.White, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta,
                                                                        ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta,
                                                                        ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkMagenta, ConsoleColor.DarkRed,
                                                                        ConsoleColor.White};

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
        private static StringBuilder sb = new StringBuilder();

        // Cursor position variables
        private static CursorPosition sentPos = new CursorPosition(0, 0); 
        private static CursorPosition ppsPos = new CursorPosition(0, 0); 
        private static CursorPosition progBarPos = new CursorPosition(0, 0); 
        private static CursorPosition scanInfoPos = new CursorPosition(0, 0);
        private static CursorPosition curAddrPos = new CursorPosition(0, 0);
        private static CursorPosition scanTimePos = new CursorPosition(0, 0);
        private static CursorPosition perComplPos = new CursorPosition(0, 0);

        // Used to work out pings per second during ping flooding
        private static long sentPings = 0; 

        /// <summary>
        /// Displays help message
        /// </summary>
        public static void Help()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string version = Assembly.GetExecutingAssembly().GetName().Name + " Version " + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ")";

            // Reset string builder
            sb.Clear();

            // Add message string
            sb.AppendLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            sb.AppendLine(version);
            sb.AppendLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            sb.AppendLine();
            sb.AppendLine("Description:");
            sb.AppendLine("     Advanced ping utility provides geoip querying, ICMP packet customization, ");
            sb.AppendLine("     graphs and result colourization.");
            sb.AppendLine();
            sb.AppendLine("Usage: PowerPing [--?] | [--li] | [--whoami] | [--loc] | [--g] | [--cg] | [--fl] | ");
            sb.AppendLine("                 [--t] [--c count] [--w timeout] [--m message] [--i TTL] [--in interval]");
            sb.AppendLine("                 [--pt type] [--pc code] [--dm] [--4] [--short] [--nocolor] [--ts] [--ti timing]");
            sb.AppendLine("                 target_name");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendLine("     --?             Displays this help message");
            sb.AppendLine("     --t             Ping the target until stopped (Control-C to stop)");
            sb.AppendLine("     --c count       Number of pings to send");
            sb.AppendLine("     --w timeout     Time to wait for reply (in milliseconds)");
            sb.AppendLine("     --m message     Ping packet message");
            sb.AppendLine("     --i ttl         Time To Live");
            sb.AppendLine("     --in interval   Interval between each ping (in milliseconds)");
            sb.AppendLine("     --pt type       Use custom ICMP type");
            sb.AppendLine("     --pc code       Use custom ICMP code value");
            sb.AppendLine("     --dm            Display ICMP messages");
            sb.AppendLine("     --4             Force using IPv4");
            //sb.AppendLine("     --6             Force using IPv6");
            sb.AppendLine("     --sh            Show less detailed replies");
            sb.AppendLine("     --nc            No colour");
            sb.AppendLine("     --ts            Display timestamp");
            sb.AppendLine("     --ti timing     Timing level:");
            sb.AppendLine("                     0 - Paranoid    4 - Nimble");
            sb.AppendLine("                     1 - Sneaky      5 - Speedy");
            sb.AppendLine("                     2 - Quiet       6 - Insane");
            sb.AppendLine("                     3 - Polite");
            sb.AppendLine();
            sb.AppendLine("     --li            Listen for ICMP packets");
            sb.AppendLine("     --fl            Send high volume of pings to address");
            sb.AppendLine("     --g             Graph view");
            sb.AppendLine("     --cg            Compact graph view");
            sb.AppendLine("     --loc           Location info for an address");
            sb.AppendLine("     --whoami        Location info for current host");
            sb.AppendLine();
            sb.AppendLine("Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=");
            sb.AppendLine("Find the project here [https://github.com/Killeroo/PowerPing]");

            // Print string
            Console.WriteLine(sb.ToString());

            // Wait for user input
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

            // Clear builder
            sb.Clear();

            // Construct string
            sb.AppendLine();
            sb.AppendFormat("Pinging {0} ", host);
            if (!String.Equals(host, attrs.Address))
                // Only show resolved address if inputted address and resolved address are different
                sb.AppendFormat("[{0}] ", attrs.Address);
            if (!Short)
                // Only show extra detail when not in Short mode
                sb.AppendFormat("(Packet message \"{0}\") [Type={1} Code={2}] ", attrs.Message, attrs.Type, attrs.Code);
            sb.AppendFormat("[TTL={0}]:", attrs.Ttl);

            // Print string
            Console.WriteLine(sb.ToString());
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
        public static void ReplyPacket(ICMP packet, String address, int index, TimeSpan replyTime, int bytesRead)
        {
            // Display with no colour
            if (NoColor)
            {
                if (Short) // Show short hand reply
                    Console.WriteLine("Reply from: {0} type={1} time={2:0.0}ms", address, packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type], replyTime);
                else
                    Console.WriteLine("Reply from: {0} seq={1} bytes={2} type={3} time={4:0.0}ms", address, index, bytesRead, packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type], replyTime);
                return;
            }

            // Show shortened or normal reply info
            if (Short)
                Console.Write("Reply from: {0} type=", address);
            else
                Console.Write("Reply from: {0} seq={1} bytes={2} type=", address, index, bytesRead);

            // Print coloured type
            Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.Black : typeColors[packet.type];
            switch (packet.type) // Display speific type code values
            {
                case 3:
                    Console.Write(packet.code > destUnreachableCodeValues.Length ? packetTypes[packet.type] : destUnreachableCodeValues[packet.code]);
                    break;
                case 5:
                    Console.Write(packet.code > redirectCodeValues.Length ? packetTypes[packet.type] : redirectCodeValues[packet.code]);
                    break;
                case 11:
                    Console.Write(packet.code > timeExceedCodeValues.Length ? packetTypes[packet.type] : timeExceedCodeValues[packet.code]);
                    break;
                case 12:
                    Console.Write(packet.code > badParameterCodeValues.Length ? packetTypes[packet.type] : badParameterCodeValues[packet.code]);
                    break;
                default:
                    Console.Write(packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type]);
                    break;
            }
            ResetColor();

            // Display ICMP message (if specified)
            if (DisplayMessage)
                Console.Write(" msg=\"{0}\"", new string(Encoding.ASCII.GetString(packet.message).Where(c => !char.IsControl(c)).ToArray()));

            // Print coloured time segment
            Console.Write(" time=");
            if (replyTime <= TimeSpan.FromMilliseconds(100))
                Console.ForegroundColor = ConsoleColor.Green;
            else if (replyTime <= TimeSpan.FromMilliseconds(500))
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0:0.0}ms ", replyTime.TotalMilliseconds);
            ResetColor();

            // Display timestamp
            if (TimeStamp)
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));

            Console.WriteLine();

        }
        /// <summary>
        /// Display information about a captured packet
        /// </summary>
        public static void CapturedPacket(ICMP packet, String address, String timeRecieved, int bytesRead)
        {
            // Display captured packet
            Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.Black : typeColors[packet.type];
            //Console.ForegroundColor = packet.type < 16 ? ConsoleColor.Black : ConsoleColor.Gray;
            Console.WriteLine("{0}: ICMPv4: {1} bytes from {2} [type {3}] [code {4}]", timeRecieved, bytesRead, address, packet.type, packet.code);

            // Reset console colours
            ResetColor();
        }
        /// <summary>
        /// Display results of scan
        /// </summary>
        public static void ScanResults(int scanned, int found, int total, TimeSpan curTime, string curAddr = "---.---.---.---")
        {
            // Check if cursor position is already set
            if (progBarPos.Left != 0)
            {
                // Store original cursor position
                CursorPosition originalPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;

                // Update labels
                curAddrPos.SetToPosition();
                Console.WriteLine(new String(' ', 20));
                curAddrPos.SetToPosition();
                Console.Write("{0}...", curAddr);
                scanInfoPos.SetToPosition();
                Console.WriteLine("Sent: {0} Hosts: {1}", scanned, found);
                //progBarPos.SetToPosition();
                //Console.WriteLine(new String(' ', 20));
                progBarPos.SetToPosition();
                Console.WriteLine(new String('#', (scanned / total) * 20));
                perComplPos.SetToPosition();
                Console.WriteLine("{0}%", (scanned / total) * 100);
                scanTimePos.SetToPosition();
                Console.Write("{0:hh\\:mm\\:ss}", curTime);

                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            }
            else
            {
                // Setup labels
                Console.Write("Pinging ");
                curAddrPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine(curAddr);
                scanInfoPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("Sent: 0 Hosts: 0");
                Console.Write("Scanning [");
                progBarPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("                    ] ");
                perComplPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("0%");
                Console.Write("Ellapsed: ");
                scanTimePos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("12:00:00");
            }
            
        }
        public static void EndScanResults(int scanned, int found, List<string> foundHosts)
        {
            Console.CursorVisible = true;

            Console.WriteLine("Scan complete. {0} addresses scanned. {1} hosts active.", scanned, found);
            if (found != 0)
            {
                foreach (string host in foundHosts)
                {
                    Console.WriteLine("|");
                    Console.WriteLine("-- {0}", host);
                }
            }
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

            ResetColor();

            // Display stats
            double percent = (double)results.Lost / results.Sent;
            percent = Math.Round(percent * 100, 1);
            Console.WriteLine("\nStats for {0}:", attrs.Address);
            Console.WriteLine("------------------------");

            if (NoColor)
            {
                Console.Write("   General: Sent ");
                Console.Write("[ " + results.Sent + " ]");
                Console.Write(", Recieved ");
                Console.Write("[ " + results.Recieved + " ]");
                Console.Write(", Lost ");
                Console.Write("[ " + results.Lost + " ]");
                Console.WriteLine(" (" + percent + "% loss)");
                Console.Write("     Times:");
                Console.WriteLine(" Minimum [ {0}ms ], Maximum [ {1}ms ]", results.MinTime, results.MaxTime);
                Console.Write("   Packets: Good [ {0} ]", results.GoodPackets);
                Console.Write(", Errors [ {0} ]", results.ErrorPackets);
                Console.Write(", Unknown [ {0} ]", results.OtherPackets);
                Console.WriteLine("Total time: {0:hh\\:mm\\:ss\\.f}", results.TotalRunTime);
                Console.WriteLine();
            }
            else
            {
                Console.Write("   General: Sent ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[ " + results.Sent + " ]");
                ResetColor();
                Console.Write(", Recieved ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[ " + results.Recieved + " ]");
                ResetColor();
                Console.Write(", Lost ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ " + results.Lost + " ]");
                ResetColor();
                Console.WriteLine(" (" + percent + "% loss)");
                Console.Write("     Times:");
                Console.WriteLine(" Minimum [ {0}ms ], Maximum [ {1}ms ]", results.MinTime, results.MaxTime);
                Console.Write("   Packets:");
                Console.Write(" Good ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[ {0} ]", results.GoodPackets);
                ResetColor();
                Console.Write(", Errors ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ {0} ]", results.ErrorPackets);
                ResetColor();
                Console.Write(", Unknown ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[ {0} ]", results.OtherPackets);
                ResetColor();
                Console.WriteLine("Total time: {0:hh\\:mm\\:ss\\.f}", results.TotalRunTime);
                Console.WriteLine();
            }

            // Confirm to exit
            PowerPing.Helper.Pause(true);
        }
        /// <summary>
        /// Displays and updates results of an ICMP flood
        /// </summary>
        /// <param name="results"></param>
        public static void FloodResults(PingResults results)
        {
            if (sentPos.Left > 0) // Check if labels have already been drawn
            {
                // Store original cursor position
                CursorPosition originalPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;

                // Update labels
                Console.SetCursorPosition(sentPos.Left, sentPos.Top);
                Console.Write(results.Sent);
                Console.SetCursorPosition(ppsPos.Left, ppsPos.Top);
                Console.Write("          "); // Blank first
                Console.SetCursorPosition(ppsPos.Left, ppsPos.Top);
                Console.Write(results.Sent - sentPings);

                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
                Console.CursorVisible = true;
            }
            else 
            {
                // Draw labels
                Console.WriteLine("Flooding...");
                Console.WriteLine("Threads: 1");
                Console.Write("Sent: ");
                sentPos.Left = Console.CursorLeft;
                sentPos.Top = Console.CursorTop;
                Console.WriteLine("0");
                Console.Write("Pings per Second: ");
                ppsPos.Left = Console.CursorLeft;
                ppsPos.Top = Console.CursorTop;
                Console.WriteLine();
                Console.WriteLine("Press Control-C to stop...");
            }

            sentPings = results.Sent;
        }
        public static void ListenResults(PingResults results)
        {
            Console.WriteLine("Captured Packets:");
            Console.WriteLine("     Caught [ {0} ] Lost [ {1} ]");

            Console.WriteLine("Packet types:");
            Console.Write("     ");
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.Write("[ Good = {0} ]", results.GoodPackets);
            ResetColor();
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("[ Errors = {0} ]", results.ErrorPackets);
            ResetColor();
            Console.Write(" ");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("[ Unknown = {0} ]", results.OtherPackets);
            ResetColor();

            Console.WriteLine("Total elapsed time (HH:MM:SS.FFF): {0:hh\\:mm\\:ss\\.fff}", results.TotalRunTime);
            Console.WriteLine();

            Helper.Pause(true);
        }
        /// <summary>
        /// Display Timeout message
        /// </summary>
        public static void PingTimeout(int seq)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("Request timed out. seq={0} ", seq);

            // Display timestamp
            if (TimeStamp)
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));

            // Make double sure we dont get the red line bug
            ResetColor();
            Console.WriteLine();
        }
        /// <summary>
        /// Display error message
        /// </summary>
        /// <param name="errorMessage">Error message to display</param>
        /// <param name="exit">Whether to exit program after displaying error</param>
        public static void Error(String errorMessage, bool exit = false, bool pause = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Write error message
            Console.WriteLine("ERROR: " + errorMessage);

            // Reset console colours
            ResetColor();

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

        private static void ResetColor()
        {
            Console.BackgroundColor = DefaultBackgroundColor;
            Console.ForegroundColor = DefaultForegroundColor;
        }
    }
}
