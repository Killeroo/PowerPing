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
        public static bool NoInput = false;
        public static bool DisplayMessage = false;
        public static bool TimeStamp = false;
        public static int DecimalPlaces = 1;
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
        /// Displays current version number and build date
        /// </summary>
        public static void Version(bool full = false)
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildInfo = Assembly.GetExecutingAssembly().GetLinkerTime();
            string version = Assembly.GetExecutingAssembly().GetName().Name + " Version " + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ")";
            string buildTime = buildInfo.Day + "/" + buildInfo.Month + "/" + buildInfo.Year + " " + buildInfo.TimeOfDay;

            // Clear builder
            sb.Clear();

            // Construct string
            if (full)
                sb.AppendFormat("{0} [Built {1}]", version, buildTime);
            else
                sb.AppendFormat("{0}", version);

            // Print string
            Console.WriteLine(sb.ToString());
        }
        /// <summary>
        /// Displays help message
        /// </summary>
        public static void Help()
        {
            // Reset string builder
            sb.Clear();

            // Add message string
            sb.AppendLine("__________                         __________.__                ");
            sb.AppendLine(@"\______   \______  _  __ __________\______   \__| ____    ____  ");
            sb.AppendLine(@" |     ___/  _ \ \/ \/ // __ \_  __ \     ___/  |/    \  / ___\ ");
            sb.AppendLine(@" |    |  (  <_> )     /\  ___/|  | \/    |   |  |   |  \/ /_/  >");
            sb.AppendLine(@" |____|   \____/ \/\_/  \___  >__|  |____|   |__|___|  /\___  / ");
            sb.AppendLine(@"                            \/                       \//_____/  ");
            sb.AppendLine();
            sb.AppendLine("Description:");
            sb.AppendLine("     Advanced ping utility which provides geoip querying, ICMP packet");
            sb.AppendLine("     customization, graphs and result colourization.");
            sb.AppendLine();
            sb.AppendLine("Usage: PowerPing [--?] | [--li] | [--whoami] | [--loc] | [--g] | [--cg] |");
            sb.AppendLine("                 [--fl] | [--sc] | [--t] [--c count] [--w timeout] [--m \"text\"]");
            sb.AppendLine("                 [--i TTL] [--in interval] [--pt type] [--pc code] [--dm]");
            sb.AppendLine("                 [--4] [--short] [--nocolor] [--ts] [--ti timing] target_name");
            sb.AppendLine();
            sb.AppendLine("Options:");
            sb.AppendLine(" --help       [--?]            Displays this help message");
            sb.AppendLine(" --version    [--v]            Shows version and build information");
            sb.AppendLine(" --examples   [--ex]           Shows example usage");
            sb.AppendLine(" --infinite   [--t]            Ping the target until stopped (Ctrl-C to stop)");
            sb.AppendLine(" --displaymsg [--dm]           Display ICMP messages");
            sb.AppendLine(" --request    [--r]            Show request packets");
            sb.AppendLine(" --ipv4       [--4]            Force using IPv4");
            //sb.AppendLine("     --6             Force using IPv6");
            sb.AppendLine(" --shorthand  [--sh]           Show less detailed replies");
            sb.AppendLine(" --nocolor    [--nc]           No colour");
            sb.AppendLine(" --noinput    [--ni]           Require no user input");
            sb.AppendLine(" --timestamp  [--ts]           Display timestamp");
            sb.AppendLine(" --decimals   [--dp]  number   Num of decimal places to use (0 to 3)");
            sb.AppendLine(" --count      [--c]   number   Number of pings to send");
            sb.AppendLine(" --timeout    [--w]   number   Time to wait for reply (in milliseconds)");
            sb.AppendLine(" --ttl        [--i]   number   Time To Live");
            sb.AppendLine(" --interval   [--in]  number   Interval between each ping (in milliseconds)");
            sb.AppendLine(" --type       [--pt]  number   Use custom ICMP type");
            sb.AppendLine(" --code       [--pc]  number   Use custom ICMP code value");
            sb.AppendLine(" --message    [--m]   message  Ping packet message");
            sb.AppendLine(" --timing     [--ti]  timing   Timing levels:");
            sb.AppendLine("                                   0 - Paranoid    4 - Nimble");
            sb.AppendLine("                                   1 - Sneaky      5 - Speedy");
            sb.AppendLine("                                   2 - Quiet       6 - Insane");
            sb.AppendLine("                                   3 - Polite");
            sb.AppendLine();
            sb.AppendLine("Features:");
            sb.AppendLine(" --scan       [--sc]  address  Network scanning, specify range \"127.0.0.1-55\"");
            sb.AppendLine(" --listen     [--li]  address  Listen for ICMP packets");
            sb.AppendLine(" --flood      [--fl]  address  Send high volume of pings to address");
            sb.AppendLine(" --graph      [--g]   address  Graph view");
            sb.AppendLine(" --compact    [--cg]  address  Compact graph view");
            sb.AppendLine(" --location   [--loc] address  Location info for an address");
            sb.AppendLine(" --whoami                      Location info for current host");
            sb.AppendLine();
            sb.AppendLine("type '--examples' for more");
            sb.AppendLine();
            sb.AppendLine("(Location info provided by http://freegeoip.net)");
            sb.AppendLine("Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=");
            sb.AppendLine("Find the project here [https://github.com/Killeroo/PowerPing]");

            // Print string
            Console.WriteLine(sb.ToString());

            if (!NoInput)
                // Wait for user input
                PowerPing.Helper.Pause();
            
            // Flush string builder
            sb.Reset();
        }
        /// <summary>
        /// Displays example powerping usage
        /// </summary>
        public static void Examples()
        {
            sb.Clear();
            sb.AppendLine();
            sb.AppendLine("PowerPing Examples (Page 1 of 3");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping 8.8.8.8                  |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Send ping to google DNS with default values (3000ms");
            sb.AppendLine("timeout, 5 pings)");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping github.com --w 500 --t   |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Send pings indefinitely to github.com with a 500ms ");
            sb.AppendLine("timeout (Ctrl-C to stop)");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping 127.0.0.1 --m Meow       |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Send ping with packet message \"Meow\"");
            sb.AppendLine("to localhost");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping 127.0.0.1 --pt 3 --pc 2  |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Send ping with ICMP type 3 (dest unreachable)");
            sb.AppendLine("and code 2");
            Console.WriteLine(sb.ToString());
            sb.Clear();
            Helper.Pause();
            sb.AppendLine("PowerPing Examples (Page 2 of 3");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping 127.0.0.1 --pt 3 --pc 2  |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping 8.8.8.8 /c 5 -w 500 --sh |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Different argument switches (/, - or --) can");
            sb.AppendLine("be used in any combination");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping google.com /ti paranoid  |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Sends using the 'Paranoid' timing option");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping google.com /ti 1         |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Same as above");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("| powerping /sc 192.168.1.1-255      |");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Scans for hosts on network range 192.168.1.1");
            sb.AppendLine("through to 192.168.1.255");
            Console.WriteLine(sb.ToString());
            sb.Clear();
            Helper.Pause();
            sb.AppendLine("PowerPing Examples (Page 3 of 3");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("+ powerping --flood 192.168.1.2      +");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("'ICMP flood' sends a high volume of ping");
            sb.AppendLine("packets to 192.168.1.2");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("+ powerping github.com /graph        +");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Sends pings to github.com displays results");
            sb.AppendLine("in graph view. (also shows how address can be");
            sb.AppendLine("specified before or after arguments)");
            sb.AppendLine("");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("+ powerping /loc 84.23.12.4          +");
            sb.AppendLine("--------------------------------------");
            sb.AppendLine("Get location information for 84.23.12.4");
            Console.WriteLine(sb.ToString());
            sb.Clear();

            if (!NoInput)
                // Wait for user input
                PowerPing.Helper.Pause(true);
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
        /// Display ICMP packet that have been sent
        /// </summary>
        public static void RequestPacket(ICMP packet, String address, int index)
        {
            // Display with no colour
            if (NoColor)
            {
                if (Short) // Show short hand reply
                    Console.WriteLine("Request to: {0}:0 type=", address, index, packet.GetBytes().Length);
                else
                    Console.WriteLine("Request to: {0}:0 seq={1} bytes={2} type={3}", address, index, packet.GetBytes().Length, packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type]);
                return;
            }

            // Show shortened info
            if (Short)
                Console.Write("Request to: {0}:0 type=", address);
            else
                Console.Write("Request to: {0}:0 seq={1} bytes={2} type=", address, index, packet.GetBytes().Length);

            // Print coloured type
            Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.Black : typeColors[packet.type];
            Console.ForegroundColor = ConsoleColor.Gray;
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

            Console.Write(" code={0}", packet.code);

            // Display timestamp
            if (TimeStamp)
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));

            Console.WriteLine();

        }
        /// <summary>
        /// Display information about reply ping packet
        /// </summary>
        /// <param name="packet">Reply packet</param>
        /// <param name="address">Reply address</param>
        /// <param name="index">Sequence number</param>
        /// <param name="replyTime">Time taken before reply received in milliseconds</param>
        public static void ReplyPacket(ICMP packet, String address, int index, TimeSpan replyTime, int bytesRead)
        {
            // Display with no colour
            if (NoColor)
            {
                if (Short) // Show short hand reply
                    Console.WriteLine("Reply from: {0} type={1} time={2:0." + new String('0', DecimalPlaces) + "}ms", address, packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type], replyTime);
                else
                    Console.WriteLine("Reply from: {0} seq={1} bytes={2} type={3} time={4:0." + new String('0', DecimalPlaces) + "}ms", address, index, bytesRead, packet.type > packetTypes.Length ? "UNASSIGNED" : packetTypes[packet.type], replyTime);
                return;
            }

            // Show shortened info
            if (Short)
                Console.Write("Reply from: {0} type=", address);
            else
                Console.Write("Reply from: {0} seq={1} bytes={2} type=", address, index, bytesRead);

            // Print coloured type
            Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.Black : typeColors[packet.type];
            Console.ForegroundColor = ConsoleColor.Gray;
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
            Console.Write("{0:0." + new String('0', DecimalPlaces) + "}ms ", replyTime.TotalMilliseconds);
            ResetColor();

            // Display timestamp
            if (TimeStamp)
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));

            Console.WriteLine();

        }
        /// <summary>
        /// Display information about a captured packet
        /// </summary>
        public static void CapturedPacket(ICMP packet, String address, String timeReceived, int bytesRead)
        {
            // Display captured packet
            Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.Black : typeColors[packet.type];
            Console.ForegroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = packet.type < 16 ? ConsoleColor.Black : ConsoleColor.Gray;
            Console.WriteLine("{0}: ICMPv4: {1} bytes from {2} [type {3}] [code {4}]", timeReceived, bytesRead, address, packet.type, packet.code);

            // Reset console colours
            ResetColor();
        }
        /// <summary>
        /// Display results of scan
        /// </summary>
        public static void ScanProgress(int scanned, int found, int total, TimeSpan curTime, string range, string curAddr = "---.---.---.---")
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
                scanInfoPos.SetToPosition();
                Console.WriteLine("Sent: {0} Found Hosts: {1}", scanned, found);
                scanTimePos.SetToPosition();
                Console.Write("{0:hh\\:mm\\:ss}", curTime);
                curAddrPos.SetToPosition();
                Console.Write("{0}...", curAddr);
                progBarPos.SetToPosition();
                double s = scanned;
                double tot = total;
                double blockPercent = (s / tot) * 30;
                Console.WriteLine(new String('#', Convert.ToInt32(blockPercent)));
                perComplPos.SetToPosition();
                Console.WriteLine("{0}%", Math.Round((s / tot) * 100, 0));

                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            }
            else
            {
                // Setup labels
                Console.WriteLine("Scanning range [ {0} ]", range);
                scanInfoPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("Sent: 0 Hosts: 0");
                Console.Write("Pinging ");
                curAddrPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine(curAddr);
                Console.Write("Ellapsed: ");
                scanTimePos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("12:00:00");
                Console.Write("Progress [");
                progBarPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("                              ] ");
                perComplPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("0%");
            }
            
        }
        public static void EndScanResults(int scanned, List<string> foundHosts, List<double> times)
        {
            Console.CursorVisible = true;

            Console.WriteLine();
            Console.WriteLine("Scan complete. {0} addresses scanned. {1} hosts active:", scanned, foundHosts.Count);
            if (foundHosts.Count != 0)
            {
                for (int i = 0; i < foundHosts.Count; i++)//each (string host in foundHosts)
                {
                    Console.WriteLine((i == foundHosts.Count - 1 ? "\\" : "|") + " -- {0} [{1:0.0}ms]", foundHosts[i], times[i]);
                }
            }
            Console.WriteLine();

            if (!NoInput)
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
            Console.WriteLine();
            Console.WriteLine("--- Stats for {0} ---", attrs.Address);

            if (NoColor)
            {
                Console.Write("   General: Sent ");
                Console.Write("[ " + results.Sent + " ]");
                Console.Write(", Received ");
                Console.Write("[ " + results.Received + " ]");
                Console.Write(", Lost ");
                Console.Write("[ " + results.Lost + " ]");
                Console.WriteLine(" (" + percent + "% loss)");
                Console.Write("     Times:");
                Console.WriteLine(" Minimum [ {0:0.0}ms ], Maximum [ {1:0.0}ms ]", results.MinTime, results.MaxTime);
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
                Console.Write(", Received ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[ " + results.Received + " ]");
                ResetColor();
                Console.Write(", Lost ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ " + results.Lost + " ]");
                ResetColor();
                Console.WriteLine(" (" + percent + "% loss)");
                Console.Write("     Times:");
                Console.WriteLine(" Shortest [ {0:0.0}ms ], Longest [ {1:0.0}ms ]", results.MinTime, results.MaxTime);
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

            if (!NoInput)
                // Confirm to exit
                PowerPing.Helper.Pause(true);
        }
        /// <summary>
        /// Displays and updates results of an ICMP flood
        /// </summary>
        /// <param name="results"></param>
        public static void FloodProgress(PingResults results)
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
                //Console.WriteLine("Threads: 1");
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

            if (!NoInput)
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
        public static void Error(String errorMessage, bool exit = false, bool pause = false, bool newline = true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Write error message
            if (newline)
                Console.WriteLine("ERROR: " + errorMessage);
            else
                Console.Write("ERROR: " + errorMessage);

            // Reset console colours
            ResetColor();

            if (pause)
                if (!NoInput)
                    PowerPing.Helper.Pause();

            if (exit)
                Environment.Exit(0);
        }
        /// <summary>
        /// Display a general message
        /// </summary>
        public static void Message(String message, ConsoleColor color = ConsoleColor.DarkGray, bool newline = true)
        {
            if (color == ConsoleColor.DarkGray)
                color = DefaultForegroundColor; // Use default foreground color if gray is being used

            Console.ForegroundColor = color;

            if (newline)
                Console.WriteLine(message);
            else
                Console.Write(message);

            ResetColor();
        }

        public static void ResetColor()
        {
            Console.BackgroundColor = DefaultBackgroundColor;
            Console.ForegroundColor = DefaultForegroundColor;
        }
    }
}
