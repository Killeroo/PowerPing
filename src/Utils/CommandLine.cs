/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

namespace PowerPing
{
    /// <summary>
    /// Responsible parsing all commandline arguments and working out ping operation and attributes
    /// </summary>
    internal class CommandLine
    {
        /// <summary>
        /// Pases command line arguments and store properties in attributes and display object
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="attributes">PingAttributes object to store propterties from arguments</param>
        /// <returns>bool based on if parsing was successful or not</returns>
        public static bool Parse(string[] args, ref PingAttributes attributes, ref DisplayConfiguration displayConfig)
        {
            int curArg = 0;

            // Loop through arguments
            try
            {
                // Check for any input arguments first because some of the arguments can early out the program before the
                // arguments get set.
                if (args.Contains("/ri") || args.Contains("-ri") || args.Contains("--ri") ||
                    args.Contains("/requireinput") || args.Contains("-requireinput") || args.Contains("--requireinput"))
                {
                    displayConfig.RequireInput = true;
                }

                checked
                {
                    // Loop through and parse all other arguments
                    for (int count = 0; count < args.Length; count++)
                    {
                        curArg = count;

                        switch (args[count])
                        {
                            case "/version":
                            case "-version":
                            case "--version":
                            case "/v":
                            case "-v":
                            case "--v":
                                ConsoleDisplay.Version();
                                Helper.CheckRecentVersion();
                                if (displayConfig.RequireInput)
                                {
                                    Helper.WaitForUserInput();
                                }
                                Environment.Exit(0);
                                break;

                            case "/beep":
                            case "-beep":
                            case "--beep":
                            case "/b":
                            case "-b":
                            case "--b":
                                attributes.BeepMode = 1;

                                // ignore proceeding argument or if is last argument
                                if (args.Length < count + 1 || count + 1 == args.Length)
                                {
                                    continue;
                                }
                                if (IsArgument(args[count + 1])
                                    || args[count + 1].Contains("."))
                                {
                                    continue;
                                }

                                int level = Convert.ToInt32(args[count + 1]);

                                if (level > 2 || level < 1)
                                {
                                    ConsoleDisplay.Message("Invalid beep level, please use a number between 0 & 2");
                                    throw new ArgumentFormatException();
                                }
                                attributes.BeepMode = level;
                                break;

                            case "/count":
                            case "-count":
                            case "--count":
                            case "/c":
                            case "-c":
                            case "--c": // Ping count
                                attributes.Count = Convert.ToInt32(args[count + 1]);
                                break;

                            case "/infiite":
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
                                if (args[count + 1].Contains("--") || args[count + 1].Contains('/') || args[count + 1].Contains("-"))
                                {
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
                                int ttl = Convert.ToInt16(args[count + 1]);
                                if (ttl > 255)
                                {
                                    ConsoleDisplay.Message("TTL has to be between 0 and 255");
                                    throw new ArgumentFormatException();
                                }
                                attributes.Ttl = ttl;
                                break;

                            case "/interval":
                            case "-interval":
                            case "--interval":
                            case "/in":
                            case "-in":
                            case "--in": // Interval
                                attributes.Interval = Convert.ToInt32(args[count + 1]);
                                if (attributes.Interval < 1)
                                {
                                    ConsoleDisplay.Message("Ping interval cannot be less than 1ms");
                                    throw new ArgumentFormatException();
                                }
                                break;

                            case "/type":
                            case "-type":
                            case "--type":
                            case "/pt":
                            case "-pt":
                            case "--pt": // Ping type
                                var type = Convert.ToByte(args[count + 1]);
                                if (type > 255)
                                {
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

                            case "/displayConfigmsg":
                            case "-displayConfigmsg":
                            case "--displayConfigmsg":
                            case "/dm":
                            case "-dm":
                            case "--dm": // displayConfig packet message
                                displayConfig.ShowMessages = true;
                                break;

                            case "/ipv4":
                            case "-ipv4":
                            case "--ipv4":
                            case "/4":
                            case "-4":
                            case "--4": // Force ping with IPv4
                                if (attributes.UseICMPv6)
                                {
                                    // Reset IPv6 force if already set
                                    attributes.UseICMPv6 = false;
                                }
                                attributes.UseICMPv4 = true;
                                break;

                            case "/ipv6":
                            case "-ipv6":
                            case "--ipv6":
                            case "/6":
                            case "-6":
                            case "--6": // Force ping with IPv6
                                if (attributes.UseICMPv4)
                                {
                                    // Reset IPv4 force if already set
                                    attributes.UseICMPv4 = false;
                                }
                                attributes.UseICMPv6 = true;
                                break;

                            case "/help":
                            case "-help":
                            case "--help":
                            case "/?":
                            case "-?":
                            case "--?": // displayConfig help message
                                ConsoleDisplay.Help();

                                if (displayConfig.RequireInput)
                                {
                                    Helper.WaitForUserInput();
                                }

                                Environment.Exit(0);
                                break;

                            case "/shorthand":
                            case "-shorthand":
                            case "--shorthand":
                            case "/sh":
                            case "-sh":
                            case "--sh": // Use short hand messages
                                displayConfig.Short = true;
                                break;

                            case "/nocolor":
                            case "-nocolor":
                            case "--nocolor":
                            case "/nc":
                            case "-nc":
                            case "--nc": // No color mode
                                displayConfig.NoColor = true;
                                break;

                            case "/decimals":
                            case "-decimals":
                            case "--decimals":
                            case "/dp":
                            case "-dp":
                            case "--dp": // Decimal places
                                if (Convert.ToInt32(args[count + 1]) > 3 || Convert.ToInt32(args[count + 1]) < 0)
                                {
                                    throw new ArgumentFormatException();
                                }
                                displayConfig.DecimalPlaces = Convert.ToInt32(args[count + 1]);
                                break;

                            case "/symbols":
                            case "-symbols":
                            case "--symbols":
                            case "/sym":
                            case "-sym":
                            case "--sym":
                                displayConfig.UseSymbols = true;
                                ConsoleDisplay.SetAsciiReplySymbolsTheme(0);

                                // ignore proceeding argument or if is last argument
                                if (args.Length < count + 1 || count + 1 == args.Length)
                                {
                                    continue;
                                }
                                if (IsArgument(args[count + 1])
                                    || args[count + 1].Contains("."))
                                {
                                    continue;
                                }

                                // Change symbols theme if an argument is present
                                int theme = Convert.ToInt32(args[count + 1]);
                                ConsoleDisplay.SetAsciiReplySymbolsTheme(theme);
                                break;

                            case "/random":
                            case "-random":
                            case "--random":
                            case "/rng":
                            case "-rng":
                            case "--rng":
                                attributes.RandomMessage = true;
                                break;

                            case "/limit":
                            case "-limit":
                            case "--limit":
                            case "/l":
                            case "-l":
                            case "--l":
                                if (Convert.ToInt32(args[count + 1]) == 1)
                                {
                                    displayConfig.ShowSummary = false;
                                    displayConfig.ShowIntro = false;
                                }
                                else if (Convert.ToInt32(args[count + 1]) == 2)
                                {
                                    displayConfig.ShowSummary = false;
                                    displayConfig.ShowIntro = false;
                                    displayConfig.ShowReplies = false;
                                    displayConfig.ShowRequests = true;
                                }
                                else if (Convert.ToInt32(args[count + 1]) == 3)
                                {
                                    displayConfig.ShowReplies = false;
                                    displayConfig.ShowRequests = false;
                                }
                                else
                                {
                                    throw new ArgumentFormatException();
                                }
                                break;

                            case "/notimeout":
                            case "-notimeout":
                            case "--notimeout":
                            case "/nt":
                            case "-nt":
                            case "--nt":
                                displayConfig.ShowTimeouts = false;
                                break;

                            case "/timestamp":
                            case "-timestamp":
                            case "--timestamp":
                            case "/ts":
                            case "-ts":
                            case "--ts": // displayConfig timestamp
                                if (count + 1 >= args.Length
                                    || IsArgument(args[count + 1]))
                                {
                                    displayConfig.ShowFullTimeStamp = true;
                                }
                                else if (args[count + 1].ToLower() == "utc")
                                {
                                    displayConfig.ShowtimeStampUTC = true;
                                }
                                else
                                {
                                    displayConfig.ShowTimeStamp = true;
                                }
                                break;

                            case "/fulltimestamp":
                            case "-fulltimestamp":
                            case "--fulltimestamp":
                            case "/fts":
                            case "-fts":
                            case "--fts": // displayConfig timestamp with date
                                if (count + 1 >= args.Length
                                    || IsArgument(args[count + 1]))
                                {
                                    displayConfig.ShowFullTimeStamp = true;
                                }
                                else if (args[count + 1].ToLower() == "utc")
                                {
                                    displayConfig.ShowFullTimeStampUTC = true;
                                }
                                else
                                {
                                    displayConfig.ShowFullTimeStamp = true;
                                }
                                break;

                            case "/timing":
                            case "-timing":
                            case "--timing":
                            case "/ti":
                            case "-ti":
                            case "--ti": // Timing option
                                switch (args[count + 1].ToLowerInvariant())
                                {
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
                                    case "rimble":
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
                                        attributes.RandomMessage = true;
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
                            case "/requests":
                            case "-requests":
                            case "--requests":
                            case "/r":
                            case "-r":
                            case "--r":
                                displayConfig.ShowRequests = true;
                                break;

                            case "/quiet":
                            case "-quiet":
                            case "--quiet":
                            case "/q":
                            case "-q":
                            case "--q":
                                displayConfig.ShowOutput = false;
                                break;

                            case "/resolve":
                            case "-resolve":
                            case "--resolve":
                            case "/res":
                            case "-res":
                            case "--res":
                                displayConfig.UseResolvedAddress = true;
                                break;

                            case "/inputaddr":
                            case "-inputaddr":
                            case "--inputaddr":
                            case "/ia":
                            case "-ia":
                            case "--ia":
                                displayConfig.UseInputtedAddress = true;
                                break;

                            case "/buffer":
                            case "-buffer":
                            case "--buffer":
                            case "/rb":
                            case "-rb":
                            case "--rb":
                                int recvbuff = Convert.ToInt32(args[count + 1]);
                                if (recvbuff < 65000)
                                {
                                    attributes.ReceiveBufferSize = recvbuff;
                                }
                                else
                                {
                                    throw new ArgumentFormatException();
                                }
                                break;

                            case "/checksum":
                            case "-checksum":
                            case "--checksum":
                            case "/chk":
                            case "-chk":
                            case "--chk":
                                displayConfig.ShowChecksum = true;
                                break;

                            case "/dontfrag":
                            case "-dontfrag":
                            case "--dontfrag":
                            case "/df":
                            case "-df":
                            case "--df":
                                attributes.DontFragment = true;
                                break;

                            case "/log":
                            case "-log":
                            case "--log":
                            case "/f":
                            case "-f":
                            case "--f":
                                attributes.EnableLogging = true;
                                attributes.LogFilename = LogFile.GenerateLogFileName();
                                break;

                            case "/size":
                            case "-size":
                            case "--size":
                            case "/s":
                            case "-s":
                            case "--s":
                                int size = Convert.ToInt32(args[count + 1]);
                                if (size < 100000)
                                {
                                    attributes.ArtificalMessageSize = size;
                                }
                                else
                                {
                                    throw new ArgumentFormatException();
                                }
                                break;

                            case "/whois":
                            case "-whois":
                            case "--whois": // Whois lookup
                                attributes.Operation = PingOperation.Whois;
                                break;

                            case "/whoami":
                            case "-whoami":
                            case "--whoami": // Current computer location
                                attributes.Operation = PingOperation.Whoami;
                                break;

                            case "/location":
                            case "-location":
                            case "--location":
                            case "/loc":
                            case "-loc":
                            case "--loc": // Location lookup
                                attributes.Operation = PingOperation.Location;
                                break;

                            case "/listen":
                            case "-listen":
                            case "--listen":
                            case "/li":
                            case "-li":
                            case "--li": // Listen for ICMP packets
                                attributes.Operation = PingOperation.Listen;
                                break;

                            case "/graph":
                            case "-graph":
                            case "--graph":
                            case "/g":
                            case "-g":
                            case "--g": // Graph view
                                attributes.Operation = PingOperation.Graph;
                                break;

                            case "/flood":
                            case "-flood":
                            case "--flood":
                            case "/fl":
                            case "-fl":
                            case "--fl": // Flood
                                attributes.Operation = PingOperation.Flood;
                                break;

                            case "/scan":
                            case "-scan":
                            case "--scan":
                            case "/sc":
                            case "-sc":
                            case "--sc": // Scan
                                attributes.Operation = PingOperation.Scan;
                                break;

                            default:
                                // Check for invalid argument
                                if (IsArgument(args[count])
                                    && attributes.Operation != PingOperation.Scan // (ignore if scanring) // TODO: Change this
                                    && (!Helper.IsURL(args[count]) && !Helper.IsIPv4Address(args[count])))
                                {
                                    throw new InvalidArgumentException();
                                }
                                break;
                        }
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                ConsoleDisplay.Error($"Missing argument parameter @ \"PowerPing >>>{args[curArg]}<<<\"");
                return false;
            }
            catch (OverflowException)
            {
                ConsoleDisplay.Error($"Overflow while converting @ \"PowerPing {args[curArg]} >>>{args[curArg + 1]}<<<\"");
                return false;
            }
            catch (InvalidArgumentException)
            {
                ConsoleDisplay.Error($"Invalid argument @ \"PowerPing >>>{args[curArg]}<<<\"");
                return false;
            }
            catch (ArgumentFormatException)
            {
                ConsoleDisplay.Error($"Incorrect parameter for [{args[curArg]}] @ \"PowerPing {args[curArg]} >>>{args[curArg + 1]}<<<\"");
                return false;
            }
            catch (Exception e)
            {
                ConsoleDisplay.Error($"An {e.GetType().ToString().Split('.').Last()} exception occured @ \"PowerPing >>>{args[curArg]}<<<\"");
                return false;
            }
            return true;
        }

        public static List<string> FindAddresses(string[] args)
        {
            List<string> addresses = new List<string>();
            foreach (string arg in args)
            {
                if (Helper.IsIPv4Address(arg))
                {
                    addresses.Add(arg);
                }

                if (Helper.IsURL(arg))
                {
                    addresses.Add(arg);
                }
            }

            return addresses;
        }

        /// <summary>
        /// Search for host can be IPv4 or URL, stores the results in the PingAttributes object
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="attributes">PingAttributes object to store propterties from arguments</param>
        /// <returns>Returns if the address was found or not</returns>
        public static bool FindAddress(string[] args, ref PingAttributes attributes)
        {
            // Skip first argument, if it's the name of the program
            // (this is a problematic line)
            if (args.First().ToLower() == "powerping")
                args = args.Skip(1).ToArray();

            // Look for valid scan address (slightly different format than normal address)
            if (attributes.Operation == PingOperation.Scan)
            {
                if (Helper.IsValidScanRange(args.First()))
                {
                    attributes.InputtedAddress = args.First();
                    return true;
                }
                if (Helper.IsValidScanRange(args.Last()))
                {
                    attributes.InputtedAddress = args.Last();
                    return true;
                }

                // Didn't find one..
                return false;
            }

            // First check first and last arguments for IPv4 address
            if (Helper.IsIPv4Address(args.Last()))
            {
                attributes.InputtedAddress = args.Last();
                return true;
            }
            if (Helper.IsIPv4Address(args.First()))
            {
                attributes.InputtedAddress = args.First();
                return true;
            }

            // Then check for URLs
            if (Helper.IsURL(args.Last()))
            {
                attributes.InputtedAddress = args.Last();
                return true;
            }
            if (Helper.IsURL(args.First()))
            {
                attributes.InputtedAddress = args.First();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a string is a valid argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private static bool IsArgument(string arg)
        {
            if (arg.Contains("--") || arg.Contains("/") || arg.Contains("-"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}