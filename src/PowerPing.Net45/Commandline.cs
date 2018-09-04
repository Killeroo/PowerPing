using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing
{
    /// <summary>
    /// Contains command line utilities (such as commands parsing etc)
    /// </summary>
    class Commandline
    {

        public static Attributes ParseArguments(string[] args)
        {
            Attributes attributes = new Attributes();
            int curArg = 0;

            // Loop through arguments
            try {
                checked {

                    for (int count = 0; count < args.Length; count++) {
                        curArg = count;

                        switch (args[count]) {
                            case "/version":
                            case "-version":
                            case "--version":
                            case "/v":
                            case "-v":
                            case "--v":
                                Display.Version(true);
                                Environment.Exit(0);
                                break;
                            case "/beep":
                            case "-beep":
                            case "--beep":
                            case "/b":
                            case "-b":
                            case "--b":
                                int level = Convert.ToInt32(args[count + 1]);
                                if (level > 2) {
                                    PowerPing.Display.Error("Invalid beep level, please use a number between 0 & 2", true, true);
                                }
                                attributes.BeepLevel = level;
                                break;
                            case "/count":
                            case "-count":
                            case "--count":
                            case "/c":
                            case "-c":
                            case "--c": // Ping count
                                attributes.Count = Convert.ToInt32(args[count + 1]);
                                break;
                            case "/infinite":
                            case "-infinite":
                            case "--infinite":
                            case "/t":
                            case "-t":
                            case "--t": // Infinitely send
                                attributes.Continous = true;
                                break;
                            case "/timeout":
                            case "-timeout":
                            case "--timeout":
                            case "/w":
                            case "-w":
                            case "--w": // Timeout
                                attributes.Timeout = Convert.ToInt32(args[count + 1]);
                                break;
                            case "/message":
                            case "-message":
                            case "--message":
                            case "/m":
                            case "-m":
                            case "--m": // Message
                                if (args[count + 1].Contains("--") || args[count + 1].Contains("//") || args[count + 1].Contains("-")) {
                                    throw new ArgumentFormatException();
                                }
                                attributes.Message = args[count + 1];
                                break;
                            case "/ttl":
                            case "-ttl":
                            case "--ttl":
                            case "/i":
                            case "-i":
                            case "--i": // Time To Live
                                attributes.Ttl = Convert.ToInt16(args[count + 1]);
                                break;
                            case "/interval":
                            case "-interval":
                            case "--interval":
                            case "/in":
                            case "-in":
                            case "--in": // Interval
                                attributes.Interval = Convert.ToInt32(args[count + 1]);
                                if (attributes.Interval < 1) {
                                    PowerPing.Display.Error("Ping interval cannot be less than 1ms", true, true);
                                }
                                break;
                            case "/type":
                            case "-type":
                            case "--type":
                            case "/pt":
                            case "-pt":
                            case "--pt": // Ping type
                                var type = Convert.ToByte(args[count + 1]);
                                if (type > 255) {
                                    throw new ArgumentFormatException();
                                }
                                attributes.Type = type;
                                break;
                            case "/code":
                            case "-code":
                            case "--code":
                            case "/pc":
                            case "-pc":
                            case "--pc": // Ping code
                                attributes.Code = Convert.ToByte(args[count + 1]);
                                break;
                            case "/displaymsg":
                            case "-displaymsg":
                            case "--displaymsg":
                            case "/dm":
                            case "-dm":
                            case "--dm": // Display packet message
                                Display.ShowMessages = true;
                                break;
                            case "/ipv4":
                            case "-ipv4":
                            case "--ipv4":
                            case "/4":
                            case "-4":
                            case "--4": // Force ping with IPv4
                                if (attributes.ForceV6) {
                                    // Reset IPv6 force if already set
                                    attributes.ForceV6 = false;
                                }
                                attributes.ForceV4 = true;
                                break;
                            case "/ipv6":
                            case "-ipv6":
                            case "--ipv6":
                            case "/6":
                            case "-6":
                            case "--6": // Force ping with IPv6
                                if (attributes.ForceV4) {
                                    // Reset IPv4 force if already set
                                    attributes.ForceV4 = false;
                                }
                                attributes.ForceV6 = true;
                                break;
                            case "/help":
                            case "-help":
                            case "--help":
                            case "/?":
                            case "-?":
                            case "--?": // Display help message
                                PowerPing.Display.Help();
                                Environment.Exit(0);
                                break;
                            case "/examples":
                            case "-examples":
                            case "--examples":
                            case "/ex":
                            case "-ex":
                            case "--ex": // Displays examples
                                PowerPing.Display.Examples();
                                break;
                            case "/shorthand":
                            case "-shorthand":
                            case "--shorthand":
                            case "/sh":
                            case "-sh":
                            case "--sh": // Use short hand messages
                                Display.Short = true;
                                break;
                            case "/nocolor":
                            case "-nocolor":
                            case "--nocolor":
                            case "/nc":
                            case "-nc":
                            case "--nc": // No color mode
                                Display.NoColor = true;
                                break;
                            case "/input":
                            case "-input":
                            case "--input":// No input mode
                                Display.NoInput = false;
                                break;
                            case "/decimals":
                            case "-decimals":
                            case "--decimals":
                            case "/dp":
                            case "-dp":
                            case "--dp": // Decimal places
                                if (Convert.ToInt32(args[count + 1]) > 3 || Convert.ToInt32(args[count + 1]) < 0) {
                                    throw new ArgumentFormatException();
                                }
                                Display.DecimalPlaces = Convert.ToInt32(args[count + 1]);
                                break;
                            case "/symbols":
                            case "-symbols":
                            case "--symbols":
                            case "/s":
                            case "-s":
                            case "--s":
                                Display.UseSymbols = true;
                                break;
                            case "/random":
                            case "-random":
                            case "--random":
                            case "/rng":
                            case "-rng":
                            case "--rng":
                                attributes.RandomMsg = true;
                                break;
                            case "/limit":
                            case "-limit":
                            case "--limit":
                            case "/l":
                            case "-l":
                            case "--l":
                                if (Convert.ToInt32(args[count + 1]) == 0) {
                                    Display.ShowReplies = true;
                                    Display.ShowRequests = false;
                                } else if (Convert.ToInt32(args[count + 1]) == 1) {
                                    Display.ShowReplies = false;
                                    Display.ShowRequests = true;
                                } else {
                                    throw new ArgumentFormatException();
                                }
                                break;
                            case "/notimeout":
                            case "-notimeout":
                            case "--notimeout":
                            case "/nt":
                            case "-nt":
                            case "--nt":
                                Display.ShowTimeouts = false;
                                break;
                            case "/timestamp":
                            case "-timestamp":
                            case "--timestamp":
                            case "/ts":
                            case "-ts":
                            case "--ts": // Display timestamp
                                Display.ShowTimeStamp = true;
                                break;
                            case "/timing":
                            case "-timing":
                            case "--timing":
                            case "/ti":
                            case "-ti":
                            case "--ti": // Timing option
                                switch (args[count + 1].ToLowerInvariant()) {
                                    case "0":
                                    case "paranoid":
                                        attributes.Timeout = 10000;
                                        attributes.Interval = 300000;
                                        break;
                                    case "1":
                                    case "sneaky":
                                        attributes.Timeout = 5000;
                                        attributes.Interval = 120000;
                                        break;
                                    case "2":
                                    case "quiet":
                                        attributes.Timeout = 5000;
                                        attributes.Interval = 30000;
                                        break;
                                    case "3":
                                    case "polite":
                                        attributes.Timeout = 3000;
                                        attributes.Interval = 3000;
                                        break;
                                    case "4":
                                    case "nimble":
                                        attributes.Timeout = 2000;
                                        attributes.Interval = 750;
                                        break;
                                    case "5":
                                    case "speedy":
                                        attributes.Timeout = 1500;
                                        attributes.Interval = 500;
                                        break;
                                    case "6":
                                    case "insane":
                                        attributes.Timeout = 750;
                                        attributes.Interval = 100;
                                        break;
                                    case "7":
                                    case "random":
                                        attributes.RandomTiming = true;
                                        attributes.RandomMsg = true;
                                        attributes.Interval = Helper.RandomInt(5000, 100000);
                                        attributes.Timeout = 15000;
                                        break;
                                    default: // Unknown timing type
                                        throw new ArgumentFormatException();
                                }
                                break;
                            case "/request":
                            case "-request":
                            case "--request":
                            case "/r":
                            case "-r":
                            case "--r":
                                Display.ShowRequests = true;
                                break;
                            case "/quiet":
                            case "-quiet":
                            case "--quiet":
                            case "/q":
                            case "-q":
                            case "--q":
                                Display.ShowOutput = false;
                                break;
                            case "/resolve":
                            case "-resolve":
                            case "--resolve":
                            case "/res":
                            case "-res":
                            case "--res":
                                Display.UseResolvedAddress = true;
                                break;
                            case "/inputaddr":
                            case "-inputaddr":
                            case "--inputaddr":
                            case "/ia":
                            case "-ia":
                            case "--ia":
                                Display.UseInputtedAddress = true;
                                break;
                            case "/cookies":
                            case "-cookies":
                            case "--cookies":
                            case "/co":
                            case "-co":
                            case "--co":
                                attributes.UsePingCookies = true;
                                break;
                            case "/buffer":
                            case "-buffer":
                            case "--buffer":
                            case "/rb":
                            case "-rb":
                            case "--rb":
                                int recvbuff = Convert.ToInt32(args[count + 1]);
                                if (recvbuff < 65000) {
                                    attributes.RecieveBufferSize = recvbuff;
                                } else {
                                    throw new ArgumentFormatException();
                                }
                                break;
                            case "/checksum":
                            case "-checksum":
                            case "--checksum":
                            case "/ch":
                            case "-ch":
                            case "--ch":
                                Display.ShowChecksum = true;
                                break;
                            case "/dontfrag":
                            case "-dontfrag":
                            case "--dontfrag":
                            case "/df":
                            case "-df":
                            case "--df":
                                attributes.DontFragment = true;
                                break;
                            case "/whois":
                            case "-whois":
                            case "--whois":
                                attributes.OpType = "whois";
                                break;
                            case "/whoami":
                            case "-whoami":
                            case "--whoami": // Current computer location
                                attributes.OpType = "whoami";
                                break;
                            case "/location":
                            case "-location":
                            case "--location":
                            case "/loc":
                            case "-loc":
                            case "--loc": // Location lookup
                                attributes.OpType = "location";
                                break;
                            case "/listen":
                            case "-listen":
                            case "--listen":
                            case "/li":
                            case "-li":
                            case "--li": // Listen for ICMP packets
                                attributes.OpType = "listening";
                                break;
                            case "/graph":
                            case "-graph":
                            case "--graph":
                            case "/g":
                            case "-g":
                            case "--g": // Graph view
                                attributes.OpType = "graphing";
                                break;
                            case "/compact":
                            case "-compact":
                            case "--compact":
                            case "/cg":
                            case "-cg":
                            case "--cg": // Compact graph view
                                attributes.OpType = "compactgraph";
                                break;
                            case "/flood":
                            case "-flood":
                            case "--flood":
                            case "/fl":
                            case "-fl":
                            case "--fl": // Flood
                                attributes.OpType = "flooding";
                                break;
                            case "/scan":
                            case "-scan":
                            case "--scan":
                            case "/sc":
                            case "-sc":
                            case "--sc": // Scan
                                attributes.OpType = "scanning";
                                break;
                            default:
                                // Check for invalid argument 
                                if ((args[count].Contains("--") || args[count].Contains("/") || args[count].Contains("-"))
                                    && !attributes.OpType.Equals("scanning") // (ignore if scanning)
                                    && args[count].Length < 7) { // Ignore args under 7 chars (assume they are an address)
                                    throw new ArgumentFormatException();
                                }
                                break;
                        }
                    }
                }
            } catch (IndexOutOfRangeException) {
                PowerPing.Display.Error("Missing argument parameter", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for help.");
                Helper.Pause();
                Environment.Exit(1);
            } catch (OverflowException) {
                PowerPing.Display.Error("Overflow while converting", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing " + args[curArg] + ">>>" + args[curArg + 1] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for help.");
                Helper.Pause();
                Environment.Exit(1);
            } catch (ArgumentFormatException) {
                PowerPing.Display.Error("Invalid argument or incorrect parameter", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for help.");
                Helper.Pause();
                Environment.Exit(1);
            } catch (Exception e) {
                PowerPing.Display.Error("A " + e.GetType().ToString().Split('.').Last() + " Exception Occured", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for more info.");
                Helper.Pause();
                Environment.Exit(1);
            }
        }

        // Set address for command (flood, listen etc)
        public static Attributes FindAddress(Attributes attrs)
        {
            if (attributes.OpType.Equals("") || attributes.OpType.Equals("flooding") || attributes.OpType.Equals("graphing")
                || attributes.OpType.Equals("compactGraph") || attributes.OpType.Equals("location") || attributes.OpType.Equals("whois")) {

                if (Uri.CheckHostName(args.First()) == UriHostNameType.Unknown
                    && Uri.CheckHostName(args.Last()) == UriHostNameType.Unknown) {
                    PowerPing.Display.Error("Unknown address format. \nIf address is a url do not include any trailing '/'s, for example use: google.com NOT google.com/test.html", true, true);
                }

                if (Uri.CheckHostName(args.First()) == UriHostNameType.Unknown) {
                    attributes.Host = args.Last();
                } else {
                    attributes.Host = args.First();
                }
            }
        }
    }
}
