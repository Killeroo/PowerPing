/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    internal class ProgramStrings
    {
        // General messages
        public const string TIMESTAMP_LAYOUT = " @ {0}";
        public const string FULL_TIMESTAMP_LAYOUT = " @ {0} {1}";
        public const string TIMEOUT_TXT = "Request timed out.";
        public const string TIMEOUT_SEQ_TXT = " seq={0}";
        public const string ERROR_TXT = "ERROR: ";

        // Intro messages
        public const string INTRO_ADDR_TXT = "Pinging {0} ";
        public const string INTRO_MSG = "with {0} bytes of data [Type={1} Code={2}] [TTL={3}]";

        // Flood messages
        public const string FLOOD_INTO_TXT = "Flooding {0}...";
        public const string FLOOD_SEND_TXT = "Sent: ";
        public const string FLOOD_PPS_TXT = "Pings per Second: ";
        public const string FLOOD_EXIT_TXT = "Press Control-C to stop...";

        // Listen messages
        public const string LISTEN_INTRO_MSG = "Listening for ICMP Packets on [ {0} ]";
        public const string CAPTURED_PACKET_MSG = "{0}: [{5}] ICMPv4: {1} bytes for {2} [type {3}] [code {4}]";

        // Request messages
        public const string REQUEST_MSG = "Request to: {0}:0 seq={1} bytes={2} type=";
        public const string REQUEST_MSG_SHORT = "Request to: {0}:0 type=";
        public const string REQUEST_CODE_TXT = " code={0}";

        // Reply messages
        public const string REPLY_MSG = "Reply from: {0} seq={1} bytes={2} type=";
        public const string REPLY_MSG_SHORT = "Reply from: {0} type=";
        public const string REPLY_MSG_TXT = " msg=\"{0}\"";
        public const string REPLY_CHKSM_TXT = " chksm={0}";
        public const string REPLY_TIME_TXT = " time=";

        // Scan messages
        public const string SCAN_RANGE_TXT = "Scanning range [ {0} ]...";
        public const string SCAN_HOSTS_TXT = " Found Hosts: {1} Sent: {0} ({2} Pings Per Second) ";
        public const string SCAN_RESULT_MSG = "Scan complete. {0} addresses scanned. {1} hosts active:";
        public const string SCAN_RESULT_ENTRY = "-- {0} [{1:0.0}ms] [{2}]";
        public const string SCAN_CONNECTOR_CHAR = "|";
        public const string SCAN_END_CHAR = @"\";

        // End results messages
        public const string RESULTS_HEADER = "--- Stats for {0} ---";
        public const string RESULTS_GENERAL_TAG = "   General: ";
        public const string RESULTS_TIMES_TAG = "     Times: ";
        public const string RESULTS_TYPES_TAG = "     Types: ";
        public const string RESULTS_SENT_TXT = "Sent ";
        public const string RESULTS_RECV_TXT = ", Recieved ";
        public const string RESULTS_LOST_TXT = ", Lost ";
        public const string RESULTS_PERCENT_LOST_TXT = " ({0}% loss)";
        public const string RESULTS_PKT_GOOD = "Good ";
        public const string RESULTS_PKT_ERR = ", Errors ";
        public const string RESULTS_PKT_UKN = ", Unknown ";
        public const string RESULTS_TIME_TXT = "Min [ {0:0.0}ms ], Max [ {1:0.0}ms ], Avg [ {2:0.0}ms ]";
        public const string RESULTS_START_TIME_TXT = "Started at: {0} (local time)";
        public const string RESULTS_RUNTIME_TXT = "   Runtime: {0:hh\\:mm\\:ss\\.f}";
        public const string RESULTS_INFO_BOX = "[ {0} ]";
        public const string RESULTS_OVERFLOW_MSG =
@"SIDENOTE: I don't know how you've done it but you have caused an overflow somewhere in these ping results.
Just to put that into perspective you would have to be running a normal ping program with default settings for 584,942,417,355 YEARS to achieve this!
Well done brave soul, I don't know your motive but I salute you =^-^=";

        // Characters used in symbol mode
        public const string REPLY_LT_100_MS_SYMBOL_1 = ".";
        public const string REPLY_LT_250_MS_SYMBOL_1 = ".";
        public const string REPLY_LT_500_MS_SYMBOL_1 = ".";
        public const string REPLY_GT_500_MS_SYMBOL_1 = ".";
        public const string REPLY_TIMEOUT_SYMBOL_1 = "!";

        public const string REPLY_LT_100_MS_SYMBOL_2 = "_";
        public const string REPLY_LT_250_MS_SYMBOL_2 = "▄";
        public const string REPLY_LT_500_MS_SYMBOL_2 = "█";
        public const string REPLY_GT_500_MS_SYMBOL_2 = "█";
        public const string REPLY_TIMEOUT_SYMBOL_2 = "!";

        public const string HELP_MSG =
@"__________                         __________.__                
\______   \______  _  __ __________\______   \__| ____    ____  
 |     ___/  _ \ \/ \/ // __ \_  __ \     ___/  |/    \  / ___\ 
 |    |  (  <_> )     /\  ___/|  | \/    |   |  |   |  \/ /_/  >
 |____|   \____/ \/\_/  \___  >__|  |____|   |__|___|  /\___  / 
                            \/                       \//_____/  

Description:
        Advanced ping utility - Provides geoip querying, ICMP packet
        customization, graphs and result colourization.

Ping Arguments:
    --infinite      [--t]            Ping the target until stopped (Ctrl-C to stop)
    --ipv4          [--4]            Force using IPv4
    --random        [--rng]          Generates random ICMP message
    --dontfrag      [--df]           Set 'Don't Fragment' flag
    --buffer        [--rb]  number   Sets recieve buffer size (default is 5096)
    --beep          [--b]   number   Beep on timeout (1) or on reply (2)
    --count         [--c]   number   Number of pings to send
    --timeout       [--w]   number   Time to wait for reply (in milliseconds)
    --ttl           [--i]   number   Time To Live for packet
    --interval      [--in]  number   Interval between each ping (in milliseconds)
    --type          [--pt]  number   Use custom ICMP type
    --code          [--pc]  number   Use custom ICMP code value
    --size          [--s]   number   Set size (in bytes) of packet (overwrites packet message)
    --message       [--m]   message  Ping packet message
    --timing        [--ti]  timing   Timing levels:
                                        0 - Paranoid    4 - Nimble
                                        1 - Sneaky      5 - Speedy
                                        2 - Quiet       6 - Insane
                                        3 - Polite      7 - Random

Display Arguments:
    --shorthand     [--sh]           Show less detailed replies
    --displaymsg    [--dm]           Display ICMP message field contents
    --timestamp     [--ts]           Display timestamps (add 'UTC' for Coordinated Universal Time)
    --fulltimestamp [--fts]          Display full timestamps with localised date and time
    --nocolor       [--nc]           No colour
    --symbols       [--sym]          Renders replies and timeouts as ASCII symbols (add '1' for alt theme)
    --requests      [--r]            Show request packets
    --notimeouts    [--nt]           Don't display timeout messages
    --quiet         [--q]            No output (only affects normal ping)
    --resolve       [--res]          Resolve hostname of response address from DNS
    --inputaddr     [--ia]           Show input address instead of revolved IP address
    --checksum      [--chk]          Display checksum of packet
    --requireinput  [--ri]           Always ask for user input upon completion 
    --limit         [--l]   number   Limits output to just replies(1), requests(2) or summary(3)
    --decimals      [--dp]  number   Num of decimal places to use (0 to 3)

Modes:
    --scan          [--sc]  address  Network scanning, specify range ""127.0.0.1-55""
    --flood         [--fl]  address  Send high volume of pings to address
    --graph         [--g]   address  Graph view
    --location      [--loc] address  Location info for an address
    --listen        [--li]  address  Listen for ICMP packets on specific address
    --listen        [--li]           Listen for ICMP packets on all local network adapters
    --whois                 address  Whois lookup for an address
    --whoami                         Location info for current host

Other:
    --help          [--?]            Displays this help message
    --version       [--v]            Shows version and build information

Written by Matthew Carney [matthewcarney64@gmail.com] =^-^=
Find the project here [https://github.com/Killeroo/PowerPing]

";

    }

    internal static class ICMPStrings
    {
        // ICMP packet types
        public static readonly string[] PacketTypes = new[] 
        {
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

        // Type specific code values
        public static readonly string[] DestinationUnreachableCodeValues = new[] 
        {
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
        public static readonly string[] RedirectCodeValues = new[] 
        {
            "REDIRECT FOR THE NETWORK",
            "REDIRECT FOR THE HOST",
            "REDIRECT FOR THE TOS & NETWORK",
            "REDIRECT FOR THE TOS & HOST"
        };
        public static readonly string[] TimeExceedCodeValues = new[] 
        {
            "TTL EXPIRED IN TRANSIT",
            "FRAGMENT REASSEMBLY TIME EXCEEDED"
        };
        public static readonly string[] BadParameterCodeValues = new[] 
        {
            "IP HEADER POINTER INDICATES ERROR",
            "IP HEADER MISSING AN OPTION",
            "BAD IP HEADER LENGTH"
        };
    }
}
