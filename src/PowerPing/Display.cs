/*
MIT License - PowerPing 

Copyright (c) 2017 Matthew Carney

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace PowerPing
{
    /// <summary>
    /// Display class, responsible for displaying all output from PowerPing
    /// (except for graph output) designed for console output (and also stdio)
    /// </summary>
    public static class Display
    {
        // Properties
        public static bool Short = false;
        public static bool NoColor = false;
        public static bool NoInput = false;
        public static bool UseSymbols = false;
        public static bool ShowOutput = true;
        public static bool ShowMessages = false;
        public static bool ShowTimeStamp = false;
        public static bool ShowTimeouts = true;
        public static bool ShowRequests = false;
	    public static bool ShowReplies = true;
        public static bool UseInputtedAddress = false;
        public static bool UseResolvedAddress = false;
        public static int DecimalPlaces = 1;
        public static ConsoleColor DefaultForegroundColor;
        public static ConsoleColor DefaultBackgroundColor;

        const string REPLY_SYMBOL = ".";
        const string TIMEOUT_SYMBOL = "!";

        // Stores console cursor position, used for updating text at position
        // (IEquatable used for performance comparison)
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

        // ICMP packet types
        private static string[] packetTypes = new [] {
            "ECHO REPLY", /* 0 */
            "[1] UNASSIGNED [RESV]", /* 1 */ 
            "[2] UNASSIGNED [RESV]", /* 2 */
            "DESTINATION UNREACHABLE", /* 3 */
            "SOURCE QUENCH [DEPR]", /* 4 */
            "PING REDIRECT", /* 5 */
            "ALTERNATE HOST ADDRESS [DEPR]", /* 6 */
            "[7] UNASSIGNED [RESV]", /* 7 */
            "ECHO REQUEST", /* 8 */
            "ROUTER ADVERTISEMENT", /* 9 */
            "ROUTER SOLICITATION", /* 10 */
            "TIME EXCEEDED", /* 11 */
            "PARAMETER PROBLEM", /* 12 */
            "TIMESTAMP REQUEST", /* 13 */
            "TIMESTAMP REPLY", /* 14 */
            "INFORMATION REQUEST [DEPR]", /* 15 */
            "INFORMATION REPLY [DEPR]", /* 16 */
            "ADDRESS MASK REQUEST [DEPR]", /* 17 */
            "ADDRESS MASK REPLY [DEPR]", /* 18 */
            "[19] SECURITY [RESV]", /* 19 */
            "[20] ROBUSTNESS EXPERIMENT [RESV]", /* 20 */
            "[21] ROBUSTNESS EXPERIMENT [RESV]", /* 21 */
            "[22] ROBUSTNESS EXPERIMENT [RESV]", /* 22 */
            "[23] ROBUSTNESS EXPERIMENT [RESV]", /* 23 */
            "[24] ROBUSTNESS EXPERIMENT [RESV]", /* 24 */
            "[25] ROBUSTNESS EXPERIMENT [RESV]", /* 25 */
            "[26] ROBUSTNESS EXPERIMENT [RESV]", /* 26 */
            "[27] ROBUSTNESS EXPERIMENT [RESV]", /* 27 */
            "[28] ROBUSTNESS EXPERIMENT [RESV]", /* 28 */
            "[29] ROBUSTNESS EXPERIMENT [RESV]", /* 29 */
            "TRACEROUTE [DEPR]", /* 30 */
            "DATAGRAM CONVERSATION ERROR [DEPR]", /* 31 */ 
            "MOBILE HOST REDIRECT [DEPR]", /* 32 */
            "WHERE-ARE-YOU [DEPR]", /* 33 */
            "HERE-I-AM [DEPR]", /* 34 */
            "MOBILE REGISTRATION REQUEST [DEPR]", /* 35 */ 
            "MOBILE REGISTRATION REPLY [DEPR]", /* 36 */
            "DOMAIN NAME REQUEST [DEPR]", /* 37 */
            "DOMAIN NAME REPLY [DEPR]", /* 38 */
            "SKIP DISCOVERY PROTOCOL [DEPR]", /* 39 */ 
            "PHOTURIS PROTOCOL", /* 40 */
            "EXPERIMENTAL MOBILITY PROTOCOLS", /* 41 */
            "[42] UNASSIGNED [RESV]" /* 42+ */
        };
        // Packet type colours
        private static ConsoleColor[] typeColors = new [] {
            ConsoleColor.Green, /* 0 */
            ConsoleColor.White, /* 1 */
            ConsoleColor.White, /* 2 */
            ConsoleColor.Red, /* 3 */
            ConsoleColor.Yellow, /* 4 */
            ConsoleColor.Blue, /* 5 */
            ConsoleColor.Yellow, /* 6 */
            ConsoleColor.White, /* 7 */
            ConsoleColor.Cyan, /* 8 */ 
            ConsoleColor.DarkCyan, /* 9 */
            ConsoleColor.Cyan, /* 10 */
            ConsoleColor.Red, /* 11 */
            ConsoleColor.Red, /* 12 */
            ConsoleColor.DarkBlue, /* 13 */
            ConsoleColor.Blue, /* 14 */
            ConsoleColor.DarkYellow, /* 15 */
            ConsoleColor.Yellow, /* 16 */
            ConsoleColor.DarkYellow, /* 17 */
            ConsoleColor.Yellow, /* 18 */
            ConsoleColor.White, /* 19 */
            ConsoleColor.White, /* 20 */
            ConsoleColor.White, /* 21 */
            ConsoleColor.White, /* 22 */
            ConsoleColor.White, /* 23 */
            ConsoleColor.White, /* 24 */
            ConsoleColor.White, /* 25 */
            ConsoleColor.White, /* 26 */
            ConsoleColor.White, /* 27 */
            ConsoleColor.White, /* 28 */
            ConsoleColor.White, /* 29 */ 
            ConsoleColor.Yellow, /* 30 */
            ConsoleColor.Yellow, /* 31 */
            ConsoleColor.Yellow, /* 32 */
            ConsoleColor.Cyan, /* 33 */
            ConsoleColor.Green, /* 34 */
            ConsoleColor.DarkYellow, /* 35 */
            ConsoleColor.Yellow, /* 36 */
            ConsoleColor.DarkYellow, /* 37 */
            ConsoleColor.Yellow, /* 38 */
            ConsoleColor.Yellow, /* 39 */
            ConsoleColor.Blue, /* 40 */
            ConsoleColor.Blue, /* 41 */
            ConsoleColor.White /* 41 */
        };

        // Type specific code values
        private static string[] destUnreachableCodeValues = new [] {
            "NETWORK UNREACHABLE", 
            "HOST UNREACHABLE", 
            "PROTOCOL UNREACHABLE",
            "PORT UNREACHABLE", 
            "FRAGMENTATION NEEDED & DF FLAG SET", 
            "SOURCE ROUTE FAILED",
            "DESTINATION NETWORK UNKOWN", 
            "DESTINATION HOST UNKNOWN", 
            "SOURCE HOST ISOLATED",
            "COMMUNICATION WITH DESTINATION NETWORK PROHIBITED", 
            "COMMUNICATION WITH DESTINATION NETWORK PROHIBITED",
            "NETWORK UNREACHABLE FOR ICMP", 
            "HOST UNREACHABLE FOR ICMP"
        };
        private static string[] redirectCodeValues = new [] {
            "REDIRECT FOR THE NETWORK",
            "REDIRECT FOR THE HOST",
            "REDIRECT FOR THE TOS & NETWORK",
            "REDIRECT FOR THE TOS & HOST"
        };
        private static string[] timeExceedCodeValues = new [] { 
            "TTL EXPIRED IN TRANSIT", 
            "FRAGMENT REASSEMBLY TIME EXCEEDED" 
        };
        private static string[] badParameterCodeValues = new [] { 
            "IP HEADER POINTER INDICATES ERROR", 
            "IP HEADER MISSING AN OPTION", 
            "BAD IP HEADER LENGTH" 
        };
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
        private static ulong sentPings = 0; 

        /// <summary>
        /// Displays current version number and build date
        /// </summary>
        public static void Version(bool date = false)
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildInfo = Assembly.GetExecutingAssembly().GetLinkerTime();
            string version = Assembly.GetExecutingAssembly().GetName().Name + " v" + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ")";
            string buildTime = buildInfo.Day + "/" + buildInfo.Month + "/" + buildInfo.Year + " " + buildInfo.TimeOfDay;

            // Clear builder
            sb.Clear();

            // Construct string
            if (date) {
                sb.AppendFormat("[Built {0}]", buildTime);
            } else {
                sb.AppendFormat(version);
            }

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
            sb.AppendLine("                 [--fl] | [--sc] | [--t] [--c count] [--w timeout] [--dm]");
            sb.AppendLine("                 ([--m \"text\"] | [--rng]) [--l num] [--s] [--r] [--dp places]");
            sb.AppendLine("                 [--i TTL] [--in interval] [--pt type] [--pc code] [--b level]");
            sb.AppendLine("                 [--4] [--short] [--nocolor] [--ts] [--ti timing] [--nt] target_name");
            sb.AppendLine();
            sb.AppendLine("Ping Options:");
            sb.AppendLine(" --help       [--?]            Displays this help message");
            sb.AppendLine(" --version    [--v]            Shows version and build information");
            sb.AppendLine(" --examples   [--ex]           Shows example usage");
            sb.AppendLine(" --infinite   [--t]            Ping the target until stopped (Ctrl-C to stop)");
            sb.AppendLine(" --displaymsg [--dm]           Display ICMP messages");
            sb.AppendLine(" --ipv4       [--4]            Force using IPv4");
            sb.AppendLine(" --random     [--rng]          Generates random ICMP message");
            sb.AppendLine(" --beep       [--b]   number   Beep on timeout (1) or on reply (2)");
            sb.AppendLine(" --count      [--c]   number   Number of pings to send");
            sb.AppendLine(" --timeout    [--w]   number   Time to wait for reply (in milliseconds)");
            sb.AppendLine(" --ttl        [--i]   number   Time To Live for packet");
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
            sb.AppendLine("Display Options:");
            sb.AppendLine(" --shorthand  [--sh]           Show less detailed replies");
            sb.AppendLine(" --timestamp  [--ts]           Display timestamp");
            sb.AppendLine(" --nocolor    [--nc]           No colour");
            sb.AppendLine(" --noinput    [--ni]           Require no user input");
            sb.AppendLine(" --symbols    [--s]            Renders replies and timeouts as ASCII symbols");
            sb.AppendLine(" --request    [--r]            Show request packets");
            sb.AppendLine(" --notimeouts [--nt]           Don't display timeout messages");
            sb.AppendLine(" --quiet      [--q]            No output, only shows summary upon completion or exit");
            sb.AppendLine(" --resolve    [--res]          Display hostname from DNS");
            sb.AppendLine(" --inputaddr  [--ia]           Show input address instead of revolved IP address");
            sb.AppendLine(" --limit      [--l]   number   Limits output to just replies (0) or requests (1)");
            sb.AppendLine(" --decimals   [--dp]  number   Num of decimal places to use (0 to 3)");


            sb.AppendLine();
            sb.AppendLine("Features:");
            sb.AppendLine(" --scan       [--sc]  address  Network scanning, specify range \"127.0.0.1-55\"");
            sb.AppendLine(" --listen     [--li]  address  Listen for ICMP packets ");
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

            if (!NoInput) {
                // Wait for user input
                PowerPing.Helper.Pause();
            }

            // Flush string builder
            sb.Clear();
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

            if (!NoInput) {
                // Wait for user input
                PowerPing.Helper.Pause(true);
            }
        }
        /// <summary>
        /// Display Initial ping message to screen, declaring simple info about the ping
        /// </summary>
        /// <param name="host">Resolved host address</param>
        /// <param name="ping">Ping object</param>
        public static void PingIntroMsg(String host, PingAttributes attrs)
        {
            if (!Display.ShowOutput) {
                return;
            }

            // Clear builder
            sb.Clear();

            // Construct string
            sb.AppendLine();
            sb.AppendFormat("Pinging {0} ", host);
            if (!String.Equals(host, attrs.Address)) {
                // Only show resolved address if inputted address and resolved address are different
                sb.AppendFormat("[{0}] ", attrs.Address);
            }

            if (!Short) {
                if (attrs.RandomMsg) {
                    sb.AppendFormat("(*Random packet messages*) [Type={0} Code={1}] ", attrs.Type, attrs.Code);
                } else {
                    // Only show extra detail when not in Short mode
                    sb.AppendFormat("(Packet message \"{0}\") [Type={1} Code={2}] ", attrs.Message, attrs.Type, attrs.Code);
                }
            }

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
            if (!Display.ShowOutput)
                return;

            // Show shortened info
            if (Short) {
                Console.Write("Request to: {0}:0 type=", address);
            } else {
                Console.Write("Request to: {0}:0 seq={1} bytes={2} type=", address, index, packet.GetBytes().Length);
            }

            // Print coloured type
            PacketType(packet);
            Console.Write(" code={0}", packet.code);

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write(" @ {0}", DateTime.Now.ToString("HH:mm:ss"));
            }

            // End line
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
            if (!Display.ShowOutput) {
                return;
            }

            // If drawing symbols
            if (UseSymbols) {
                if (packet.type == 0x00) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(REPLY_SYMBOL);
                    ResetColor();
                } else {
                    Timeout(0);
                }
                return;
            }

            // Show shortened info
            if (Short) {
                Console.Write("Reply from: {0} type=", address);
            } else {
                Console.Write("Reply from: {0} seq={1} bytes={2} type=", address, index, bytesRead);
            }

            // Print icmp packet type
            PacketType(packet);

            // Display ICMP message (if specified)
            if (ShowMessages) {
                Console.Write(" msg=\"{0}\"", new string(Encoding.ASCII.GetString(packet.message).Where(c => !char.IsControl(c)).ToArray()));
            }

            // Print coloured time segment
            Console.Write(" time=");
            if (!NoColor) {
                if (replyTime <= TimeSpan.FromMilliseconds(100)) {
                    Console.ForegroundColor = ConsoleColor.Green;
                } else if (replyTime <= TimeSpan.FromMilliseconds(500)) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                } else {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            }
            Console.Write("{0:0." + new String('0', DecimalPlaces) + "}ms ", replyTime.TotalMilliseconds);
            ResetColor();

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));
            }

            // End line
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
            if (progBarPos.Left != 0) {

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

            } else {

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
                Console.Write("Progress: [ ");
                perComplPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("     ][");
                progBarPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine("..............................]");
            }
            
        }
        public static void EndScanResults(int scanned, List<string> foundHosts, List<double> times)
        {
            Console.CursorVisible = true;

            Console.WriteLine();
            Console.WriteLine("Scan complete. {0} addresses scanned. {1} hosts active:", scanned, foundHosts.Count);
            if (foundHosts.Count != 0) {
                for (int i = 0; i < foundHosts.Count; i++) {
                    Console.WriteLine((i == foundHosts.Count - 1 ? "\\" : "|") + " -- {0} [{1:0.0}ms]", foundHosts[i], times[i]);
                }
            }
            Console.WriteLine();

            if (!NoInput) {
                PowerPing.Helper.Pause();
            }
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
            percent = Math.Round(percent * 100, 2);
            Console.WriteLine();
            Console.WriteLine("--- Stats for {0} ---", attrs.Address);

            if (NoColor) {
                Console.WriteLine("   General: Sent [ {0} ], Recieved [ {1} ], Lost [ {2} ] ({3}% loss)", results.Sent, results.Received, results.Lost, percent);
                Console.WriteLine("     Times: Min [ {0:0.0}ms ] Max [ {1:0.0}ms ], Avg [ {2:0.0}ms ]", results.MinTime, results.MaxTime, results.AvgTime);
                Console.WriteLine("     Types: Good [ {0} ], Errors [ {1} ], Unknown [ {2} ]", results.GoodPackets, results.ErrorPackets, results.OtherPackets);
                Console.WriteLine("Started at: {0} (local time)", results.StartTime);
                Console.WriteLine("   Runtime: {0:hh\\:mm\\:ss\\.f}", results.TotalRunTime);
                Console.WriteLine();
            } else {
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
                Console.WriteLine(" Min [ {0:0.0}ms ], Max [ {1:0.0}ms ], Avg [ {2:0.0}ms ]", results.MinTime, results.MaxTime, results.AvgTime);
                Console.Write("ICMP Types:");
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
                Console.WriteLine("Started at: {0} (local time)", results.StartTime);
                Console.WriteLine("   Runtime: {0:hh\\:mm\\:ss\\.f}", results.TotalRunTime);
                Console.WriteLine();
            }

            if (results.HasOverflowed) {
                Console.WriteLine("SIDENOTE: I don't know how you've done it but you have caused an overflow somewhere in these ping results.");
                Console.WriteLine("Just to put that into perspective you would have to be running a normal ping program with default settings for 584,942,417,355 YEARS to achieve this!");
                Console.WriteLine("Well done brave soul, I don't know your motive but I salute you =^-^=");
                Console.WriteLine();
            }

            if (!NoInput) {
                // Confirm to exit
                PowerPing.Helper.Pause(true);
            }
        }
        /// <summary>
        /// Displays and updates results of an ICMP flood
        /// </summary>
        /// <param name="results"></param>
        public static void FloodProgress(PingResults results, string target)
        {
            if (sentPos.Left > 0) { // Check if labels have already been drawn

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

            } else {

                // Draw labels
                Console.WriteLine("Flooding {0}...", target);
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

            if (!NoInput) {
                Helper.Pause(true);
            }
        }
        /// <summary>
        /// Display Timeout message
        /// </summary>
        public static void Timeout(int seq)
        {
            if (!Display.ShowOutput || !Display.ShowTimeouts) {
                return;
            }

            // If drawing symbols
            if (UseSymbols) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(TIMEOUT_SYMBOL);
                ResetColor();
                return;
            }

            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("Request timed out.");

            // Short hand
            if (!Short) {
                Console.Write(" seq={0} ", seq);
            }

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write("@ {0}", DateTime.Now.ToString("HH:mm:ss"));
            }

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
            if (newline) {
                Console.WriteLine("ERROR: " + errorMessage);
            } else {
                Console.Write("ERROR: " + errorMessage);
            }

            // Reset console colours
            ResetColor();

            if (pause && !NoInput) {
                PowerPing.Helper.Pause();
            }

            if (exit) {
                Environment.Exit(1);
            }
        }
        /// <summary>
        /// Display a general message
        /// </summary>
        public static void Message(String message, ConsoleColor color = ConsoleColor.DarkGray, bool newline = true)
        {
            if (color == ConsoleColor.DarkGray) {
                color = DefaultForegroundColor; // Use default foreground color if gray is being used
            }

            Console.ForegroundColor = color;

            if (newline) {
                Console.WriteLine(message);
            } else {
                Console.Write(message);
            }

            ResetColor();
        }
        public static void PacketType(ICMP packet)
        {
            // Apply colour rules
            if (!NoColor) {
                Console.BackgroundColor = packet.type > typeColors.Length ? ConsoleColor.White : typeColors[packet.type];
                Console.ForegroundColor = ConsoleColor.Black;
            }

            // Print packet type
            switch (packet.type) {
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
                    Console.Write(packet.type > packetTypes.Length ? "[" + packet.type + "] UNASSIGNED " : packetTypes[packet.type]);
                    break;
            }

            ResetColor();
        }

        public static void ResetColor()
        {
            Console.BackgroundColor = DefaultBackgroundColor;
            Console.ForegroundColor = DefaultForegroundColor;
        }
    }
}
