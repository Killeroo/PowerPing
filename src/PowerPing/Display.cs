/*
MIT License - PowerPing 

Copyright (c) 2018 Matthew Carney

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
    class Display
    {
        // Properties
        public static bool Short { get; set; } = false;
        public static bool NoColor { get; set; } = false;
        public static bool NoInput { get; set; } = true;
        public static bool UseSymbols { get; set; } = false;
        public static bool ShowOutput { get; set; } = true;
        public static bool ShowMessages { get; set; } = false;
        public static bool ShowTimeStamp { get; set; } = false;
        public static bool ShowTimeouts { get; set; } = true;
        public static bool ShowRequests { get; set; } = false;
	    public static bool ShowReplies { get; set; } = true;
        public static bool ShowChecksum { get; set; } = false;
        public static bool UseInputtedAddress { get; set; } = false;
        public static bool UseResolvedAddress { get; set; } = false;
        public static int DecimalPlaces { get; set; } = 1;
        public static ConsoleColor DefaultForegroundColor { get; set; }
        public static ConsoleColor DefaultBackgroundColor { get; set; }

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

        #region Strings

        // General messages
        const string TIMESTAMP_LAYOUT = " @ {0}";
        const string TIMEOUT_TXT = "Request timed out.";
        const string TIMEOT_SEQ_TXT = " seq={0} ";
        const string ERROR_TXT = "ERROR: ";

        // Intro messages
        const string INTRO_ADDR_TXT = "Pinging {0} ";
        const string INTRO_MSG = "(Packet message \"{0}\") [Type={1} Code={2}] [TTL={3}]";
        const string INTRO_RNG_MSG = "(*Random packet messages*) [Type={0} Code={1}] [TTL={2}]";

        // Flood messages
        const string FLOOD_INTO_TXT = "Flooding {0}...";
        const string FLOOD_SEND_TXT = "Sent: ";
        const string FLOOD_PPS_TXT = "Pings per Second: ";
        const string FLOOD_EXIT_TXT = "Press Control-C to stop...";

        // Listen messages
        const string LISTEN_INTRO_MSG = "Listening for ICMP Packets ...";
        const string CAPTURED_PACKET_MSG = "{0}: ICMPv4: {1} bytes from {2} [type {3}] [code {4}]";

        // Request messages
        const string REQUEST_MSG = "Request to: {0}:0 seq={1} bytes={2} type=";
        const string REQUEST_MSG_SHORT = "Request to: {0}:0 type=";
        const string REQUEST_CODE_TXT = " code={0}";

        // Reply messages
        const string REPLY_MSG = "Reply from: {0} seq={1} bytes={2} type=";
        const string REPLY_MSG_SHORT = "Reply from: {0} type=";
        const string REPLY_MSG_TXT = " msg=\"{0}\"";
        const string REPLY_CHKSM_TXT = " chksm={0}";
        const string REPLY_TIME_TXT = " time=";

        // Scan messages
        const string SCAN_RANGE_TXT = "Scanning range [ {0} ] . . . ";
        const string SCAN_HOSTS_TXT = " Sent: {0} Found Hosts: {1}";
        const string SCAN_CUR_ADDR_TXT = " Pinging ";
        const string SCAN_RESULT_MSG = "Scan complete. {0} addresses scanned. {1} hosts active:";
        const string SCAN_RESULT_ENTRY = "-- {0} [{1:0.0}ms] [{2}]";
        const string SCAN_CONNECTOR_CHAR = "|";
        const string SCAN_END_CHAR = @"\";

        // End results messages
        const string RESULTS_HEADER = "--- Stats for {0} ---";
        const string RESULTS_GENERAL_TAG = "   General: ";
        const string RESULTS_TIMES_TAG = "     Times: ";
        const string RESULTS_TYPES_TAG = "     Types: ";
        const string RESULTS_SENT_TXT = "Sent ";
        const string RESULTS_RECV_TXT = ", Recieved ";
        const string RESULTS_LOST_TXT = ", Lost ";
        const string RESULTS_PERCENT_LOST_TXT = " ({0}% loss)";
        const string RESULTS_PKT_GOOD = "Good ";
        const string RESULTS_PKT_ERR = ", Errors ";
        const string RESULTS_PKT_UKN = ", Unknown ";
        const string RESULTS_TIME_TXT = "Min [ {0:0.0}ms ], Max [ {1:0.0}ms ], Avg [ {2:0.0}ms ]";
        const string RESULTS_START_TIME_TXT = "Started at: {0} (local time)";
        const string RESULTS_RUNTIME_TXT = "   Runtime: {0:hh\\:mm\\:ss\\.f}";
        const string RESULTS_INFO_BOX = "[ {0} ]";
        const string RESULTS_OVERFLOW_MSG = 
@"SIDENOTE: I don't know how you've done it but you have caused an overflow somewhere in these ping results.
Just to put that into perspective you would have to be running a normal ping program with default settings for 584,942,417,355 YEARS to achieve this!
Well done brave soul, I don't know your motive but I salute you =^-^=";

        // Symbol characters
        const string REPLY_SYMBOL = ".";
        const string TIMEOUT_SYMBOL = "!";

        const string HELP_MSG =
@"__________                         __________.__                
\______   \______  _  __ __________\______   \__| ____    ____  
 |     ___/  _ \ \/ \/ // __ \_  __ \     ___/  |/    \  / ___\ 
 |    |  (  <_> )     /\  ___/|  | \/    |   |  |   |  \/ /_/  >
 |____|   \____/ \/\_/  \___  >__|  |____|   |__|___|  /\___  / 
                            \/                       \//_____/  

Description:
        Advanced ping utility - Provides geoip querying, ICMP packet
        customization, graphs and result colourization.

Usage: 
    PowerPing [--?] | [--ex] | [--li] | [--whoami] | [--whois] [--loc] | [--fl] |
              [--sc] | [--g] | [--cg] | [--t] [--4] [--rng] [--df] [--rb number] 
              [--b number] [--c number] [--w number] [-i number] [--in number]
              [--pt number] [--pc number] [--s number] [--m message] [--ti timing] 
              [--sh] [--dm] [--ts] [--nc] [--input] [--sym] [--r] [--nt] [--q] [--res]
              [--ia] [--chk] [--l number] [dp number] target_name | target_address

Ping Options:
    --infinite   [--t]            Ping the target until stopped (Ctrl-C to stop)
    --ipv4       [--4]            Force using IPv4
    --random     [--rng]          Generates random ICMP message
    --dontfrag   [--df]           Set 'Don't Fragment' flag
    --buffer     [--rb]  number   Sets recieve buffer size (default is 5096)
    --beep       [--b]   number   Beep on timeout(1) or on reply(2)
    --count      [--c]   number   Number of pings to send
    --timeout    [--w]   number   Time to wait for reply (in milliseconds)
    --ttl        [--i]   number   Time To Live for packet
    --interval   [--in]  number   Interval between each ping (in milliseconds)
    --type       [--pt]  number   Use custom ICMP type
    --code       [--pc]  number   Use custom ICMP code value
    --size       [--s]   number   Set size of packet (overwrites packet message)
    --message    [--m]   message  Ping packet message
    --timing     [--ti]  timing   Timing levels:
                                    0 - Paranoid    4 - Nimble
                                    1 - Sneaky      5 - Speedy
                                    2 - Quiet       6 - Insane
                                    3 - Polite      7 - Random

Display Options:
    --shorthand  [--sh]           Show less detailed replies
    --displaymsg [--dm]           Display ICMP message field contents
    --timestamp  [--ts]           Display timestamp
    --nocolor    [--nc]           No colour
    --input                       Require user input
    --symbols    [--sym]          Renders replies and timeouts as ASCII symbols
    --request    [--r]            Show request packets
    --notimeouts [--nt]           Don't display timeout messages
    --quiet      [--q]            No output, only shows summary upon exit
    --resolve    [--res]          Resolve hostname of address from DNS
    --inputaddr  [--ia]           Show input address instead of revolved IP address
    --checksum   [--chk]           Display checksum of packet
    --limit      [--l]   number   Limits output to just replies(0) or requests(1)
    --decimals   [--dp]  number   Num of decimal places to use(0 to 3)



Features:
    --scan       [--sc]  address  Network scanning, specify range ""127.0.0.1-55""
    --listen     [--li]  address  Listen for ICMP packets
    --flood      [--fl]  address  Send high volume of pings to address
    --graph      [--g]   address  Graph view
    --compact    [--cg]  address  Compact graph view
    --location   [--loc] address  Location info for an address
    --whois              address  Whois lookup for an address
    --whoami                      Location info for current host

Other:
    --help       [--?]            Displays this help message
    --version    [--v]            Shows version and build information
    --examples   [--ex]           Displays some example usage

type '--examples' for more

(Location info provided by http://freegeoip.net)
Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=
Find the project here[https://github.com/Killeroo/PowerPing]";

        const string EXAMPLE_MSG_PAGE_1 =
@"
PowerPing Examples(Page 1 of 3)
--------------------------------------
| powerping 8.8.8.8                  |
--------------------------------------
Send ping to google DNS with default values (3000ms)
timeout, 5 pings)

--------------------------------------
| powerping github.com --w 500 --t   |
--------------------------------------
Send pings indefinitely to github.com with a 500ms 
timeout (Ctrl-C to stop)

--------------------------------------
| powerping 127.0.0.1 --m Meow       |
--------------------------------------
Send ping with packet message ""Meow""
to localhost.

--------------------------------------
| powerping 127.0.0.1 --pt 3 --pc 2  |
--------------------------------------
Send ping with ICMP type 3 (dest unreachable)
and code 2.";

        const string EXAMPLE_MSG_PAGE_2 =
@"
PowerPing Examples(Page 2 of 3)
--------------------------------------
| powerping 8.8.8.8 /c 5 -w 500 --sh |
--------------------------------------
Different argument switches (/, - or --) can
be used in any combination.

--------------------------------------
| powerping google.com /ti paranoid  |
--------------------------------------
Sends using the 'Paranoid' timing option.

--------------------------------------
| powerping google.com /ti 1         |
--------------------------------------
Same as above

--------------------------------------
| powerping /sc 192.168.1.1-255      |
--------------------------------------
Scans for hosts on network range 192.168.1.1
through to 192.168.1.255.";

        const string EXAMPLE_MSG_PAGE_3 =
@"
PowerPing Examples(Page 3 of 3)
--------------------------------------
| powerping --flood 192.168.1.2      |
--------------------------------------
ICMP flood sends a high volume of ping
packets to 192.168.1.2.

--------------------------------------
| powerping github.com /graph        |
--------------------------------------
Sends pings to github.com displays results
in graph view. (also shows how address can be
specified before or after arguments).

--------------------------------------
| powerping /loc 84.23.12.4          |
--------------------------------------
Get location information for 84.23.12.4";

        #endregion

        #region ICMP type and code strings (and colour rules)

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

        #endregion

        #region Cursor position variables

        private static CursorPosition sentPos = new CursorPosition(0, 0);
        private static CursorPosition ppsPos = new CursorPosition(0, 0);
        private static CursorPosition progBarPos = new CursorPosition(0, 0);
        private static CursorPosition scanInfoPos = new CursorPosition(0, 0);
        private static CursorPosition curAddrPos = new CursorPosition(0, 0);
        private static CursorPosition scanTimePos = new CursorPosition(0, 0);
        private static CursorPosition perComplPos = new CursorPosition(0, 0);

        #endregion

        /// <summary>
        /// Displays current version number and build date
        /// </summary>
        public static void Version(bool date = false)
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime buildInfo = Assembly.GetExecutingAssembly().GetLinkerTime();
            string version = Assembly.GetExecutingAssembly().GetName().Name + " v" + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ") ";
            string buildTime = buildInfo.Day + "/" + buildInfo.Month + "/" + buildInfo.Year + " " + buildInfo.TimeOfDay;

            // Write version
            Console.WriteLine(version + (date ? "[Built " + buildTime + "]" : ""));
        }
        /// <summary>
        /// Displays help message
        /// </summary>
        public static void Help()
        {
            // Print help message
            Version();
            Console.WriteLine(HELP_MSG);

            if (!NoInput) {
                // Wait for user input
                PowerPing.Helper.Pause();
            }
        }
        /// <summary>
        /// Displays example powerping usage
        /// </summary>
        public static void Examples()
        {
            Console.WriteLine(EXAMPLE_MSG_PAGE_1);

            Helper.Pause();
            Console.WriteLine(EXAMPLE_MSG_PAGE_2);

            Helper.Pause();
            Console.WriteLine(EXAMPLE_MSG_PAGE_3);


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
        public static void PingIntroMsg(PingAttributes attrs)
        {
            if (!Display.ShowOutput) {
                return;
            }

            // Construct string
            Console.WriteLine();
            Console.Write(INTRO_ADDR_TXT, attrs.Host);
            if (!String.Equals(attrs.Host, attrs.Address)) {
                // Only show resolved address if inputted address and resolved address are different
                Console.Write("[{0}] ", attrs.Address);
            }

            if (!Short) { // Only show extra detail when not in Short mode
                if (attrs.RandomMsg) {
                    Console.Write(INTRO_RNG_MSG, attrs.Type, attrs.Code, attrs.Ttl);
                } else {
                    Console.Write(INTRO_MSG, attrs.Message, attrs.Type, attrs.Code, attrs.Ttl);
                }
            }

            // Print string
            Console.WriteLine(":");
        }
        /// <summary>
        /// Display initial listening message
        /// </summary>
        public static void ListenIntroMsg()
        {
            Console.WriteLine(LISTEN_INTRO_MSG);
        }
        /// <summary>
        /// Display ICMP packet that have been sent
        /// </summary>
        public static void RequestPacket(ICMP packet, String address, int index)
        {
            if (!Display.ShowOutput) {
                return;
            }

            // Show shortened info
            if (Short) {
                Console.Write(REQUEST_MSG_SHORT, address);
            } else {
                Console.Write(REQUEST_MSG, address, index, packet.GetBytes().Length);
            }

            // Print coloured type
            PacketType(packet);
            Console.Write(REQUEST_CODE_TXT, packet.code);

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write(TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
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
                    if (replyTime <= TimeSpan.FromMilliseconds(100)) {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("_");
                    } else if (replyTime <= TimeSpan.FromMilliseconds(500)) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("▄");
                    } else {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("█");
                    }
                    ResetColor();
                } else {
                    Timeout(0);
                }
                return;
            }

            // Show shortened info
            if (Short) {
                Console.Write(REPLY_MSG_SHORT, address);
            } else {
                Console.Write(REPLY_MSG, address, index, bytesRead);
            }

            // Print icmp packet type
            PacketType(packet);

            // Display ICMP message (if specified)
            if (ShowMessages) {
                string messageWithoutHeader = Encoding.ASCII.GetString(packet.message, 4, packet.message.Length - 4);
                Console.Write(REPLY_MSG_TXT, new string(messageWithoutHeader.Where(c => !char.IsControl(c)).ToArray()));
            }

            // Print coloured time segment
            Console.Write(REPLY_TIME_TXT);
            if (!NoColor) {
                if (replyTime <= TimeSpan.FromMilliseconds(100)) {
                    Console.ForegroundColor = ConsoleColor.Green;
                } else if (replyTime <= TimeSpan.FromMilliseconds(500)) {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                } else {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
            }
            Console.Write("{0:0." + new String('0', DecimalPlaces) + "}ms", replyTime.TotalMilliseconds);
            ResetColor();

            // Display checksum
            if (ShowChecksum) {
                Console.Write(REPLY_CHKSM_TXT, packet.checksum);
            }

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write(TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
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
            Console.WriteLine(CAPTURED_PACKET_MSG, timeReceived, bytesRead, address, packet.type, packet.code);

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
                Console.WriteLine(SCAN_HOSTS_TXT, scanned, found);
                scanTimePos.SetToPosition();
                Console.Write("{0:hh\\:mm\\:ss}", curTime);
                curAddrPos.SetToPosition();
                Console.Write(curAddr);
                progBarPos.SetToPosition();
                double s = scanned;
                double tot = total;
                double blockPercent = (s / tot) * 30;
                Console.WriteLine(new String('=', Convert.ToInt32(blockPercent)) + ">");
                perComplPos.SetToPosition();
                Console.WriteLine("{0}%", Math.Round((s / tot) * 100, 0));

                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);

            } else {

                // Setup labels
                Console.WriteLine(SCAN_RANGE_TXT, range);
                Console.Write(SCAN_CUR_ADDR_TXT);
                curAddrPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine(curAddr);
                scanInfoPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine();
                Console.Write(" ");
                scanTimePos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.Write("00:00:00 [");
                progBarPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
				Console.Write("                               ] ");
                perComplPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.WriteLine();
            }
            
        }
        public static void ScanResults(int scanned, bool ranToEnd, List<Ping.ActiveHost> foundHosts)
        {
            Console.CursorVisible = true;

            Console.WriteLine();
            Console.Write("Scan " + (ranToEnd ? "complete" : "aborted") + ". ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(scanned);
            Console.ForegroundColor = DefaultForegroundColor;
            Console.Write(" addresses scanned. ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(foundHosts.Count);
            Console.ForegroundColor = DefaultForegroundColor;
            Console.WriteLine(" hosts found.");
            if (foundHosts.Count != 0) {
                for (int i = 0; i < foundHosts.Count; i++) {
                    Ping.ActiveHost entry = foundHosts[i];
                    Console.WriteLine((i == foundHosts.Count - 1 ? SCAN_END_CHAR : SCAN_CONNECTOR_CHAR) + SCAN_RESULT_ENTRY, entry.Address, entry.ResponseTime, entry.HostName != "" ? entry.HostName : "UNAVAILABLE");
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
        public static void PingResults(PingAttributes attrs, PingResults results)
        {
            ResetColor();

            // Display stats
            double percent = (double)results.Lost / results.Sent;
            percent = Math.Round(percent * 100, 2);
            Console.WriteLine();
            Console.WriteLine(RESULTS_HEADER, attrs.Address);

            //   General: Sent [ 0 ], Recieved [ 0 ], Lost [ 0 ] (0% loss)
            Console.Write(RESULTS_GENERAL_TAG + RESULTS_SENT_TXT);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.Sent);
            } else {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(RESULTS_INFO_BOX, results.Sent);
                ResetColor();
            }
            Console.Write(RESULTS_RECV_TXT);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.Received);
            } else {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(RESULTS_INFO_BOX, results.Received);
                ResetColor();
            }
            Console.Write(RESULTS_LOST_TXT);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.Lost);
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(RESULTS_INFO_BOX, results.Lost);
                ResetColor();
            }
            Console.WriteLine(RESULTS_PERCENT_LOST_TXT, percent);

            //     Times: Min [ 0ms ] Max [ 0ms ] Avg [ 0ms ]
            Console.WriteLine(RESULTS_TIMES_TAG + RESULTS_TIME_TXT, results.MinTime, results.MaxTime, results.AvgTime);

            //     Types: Good [ 0 ], Errors [ 0 ], Unknown [ 0 ]
            Console.Write(RESULTS_TYPES_TAG);
            Console.Write(RESULTS_PKT_GOOD);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.GoodPackets);
            } else {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(RESULTS_INFO_BOX, results.GoodPackets);
                ResetColor();
            }
            Console.Write(RESULTS_PKT_ERR);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.GoodPackets);
            } else {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(RESULTS_INFO_BOX, results.ErrorPackets);
                ResetColor();
            }
            Console.Write(RESULTS_PKT_UKN);
            if (NoColor) {
                Console.Write(RESULTS_INFO_BOX, results.GoodPackets);
            } else {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(RESULTS_INFO_BOX, results.OtherPackets);
                ResetColor();
            }

            // Started at: 0:00:00 (local time)
            Console.WriteLine(RESULTS_START_TIME_TXT, results.StartTime);

            //Runtime: hh:mm:ss.f
            Console.WriteLine(RESULTS_RUNTIME_TXT, results.TotalRunTime);
            Console.WriteLine();

            if (results.HasOverflowed) {
                Console.WriteLine(RESULTS_OVERFLOW_MSG);
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
        public static void FloodProgress(ulong totalPings, ulong pingsPerSecond, string target)
        {
            // Check if labels have already been drawn
            if (sentPos.Left > 0) { 

                // Store original cursor position
                CursorPosition originalPos = new CursorPosition(Console.CursorLeft, Console.CursorTop);
                Console.CursorVisible = false;

                // Update labels
                Console.SetCursorPosition(sentPos.Left, sentPos.Top);
                Console.Write(totalPings);
                Console.SetCursorPosition(ppsPos.Left, ppsPos.Top);
                Console.Write("          "); // Blank first
                Console.SetCursorPosition(ppsPos.Left, ppsPos.Top);
                Console.Write(pingsPerSecond);
                // Reset to original cursor position
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
                Console.CursorVisible = true;

            } else {

                // Draw labels
                Console.WriteLine(FLOOD_INTO_TXT, target);
                Console.Write(FLOOD_SEND_TXT);
                sentPos.Left = Console.CursorLeft;
                sentPos.Top = Console.CursorTop;
                Console.WriteLine("0");
                Console.Write(FLOOD_PPS_TXT);
                ppsPos.Left = Console.CursorLeft;
                ppsPos.Top = Console.CursorTop;
                Console.WriteLine();
                Console.WriteLine(FLOOD_EXIT_TXT);
            }
        }
        public static void ListenResults(PingResults results)
        {
            throw new NotImplementedException();
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
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(TIMEOUT_SYMBOL);
                ResetColor();
                return;
            }

            if (!NoColor) {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.Write(TIMEOUT_TXT);

            // Short hand
            if (!Short) {
                Console.Write(TIMEOT_SEQ_TXT, seq);
            }

            // Display timestamp
            if (ShowTimeStamp) {
                Console.Write(TIMESTAMP_LAYOUT, DateTime.Now.ToString("HH:mm:ss"));
            }

            // Make double sure we dont get the red line bug
            ResetColor();
            Console.WriteLine();
        }
        /// <summary>
        /// Display error message
        /// </summary>
        /// <param name="errMsg">Error message to display</param>
        /// <param name="exit">Whether to exit program after displaying error</param>
        public static void Error(String errMsg, bool exit = false, bool pause = false, bool newline = true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            // Write error message
            if (newline) {
                Console.WriteLine(ERROR_TXT + errMsg);
            } else {
                Console.Write(ERROR_TXT + errMsg);
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
        public static void Message(String msg, ConsoleColor color = ConsoleColor.DarkGray, bool newline = true)
        {
            if (color == ConsoleColor.DarkGray) {
                color = DefaultForegroundColor; // Use default foreground color if gray is being used
            }

            Console.ForegroundColor = color;

            if (newline) {
                Console.WriteLine(msg);
            } else {
                Console.Write(msg);
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
