/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Written by Matthew Carney [matthewcarney64@gmail.com]
 * ************************************************************************/
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
using System.Linq;
using System.Threading;

namespace PowerPing
{
    static class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Main entry point of PowerPing
        /// Parses arguments and runs operations
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args)
        {
            // Local variables 
            int curArg = 0;
            string opMode = ""; 
            PingAttributes attributes = new PingAttributes();
            attributes.Address = "";

            // Setup console
            Display.DefaultForegroundColor = Console.ForegroundColor;
            Display.DefaultBackgroundColor = Console.BackgroundColor;

            // Show current version info
            //Display.Version();

            // Check if no arguments
            if (args.Length == 0) {
                PowerPing.Display.Help();
                return;
            }

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
                            case "/sym":
                            case "-sym":
                            case "--sym":
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
                            case "/chk":
                            case "-chk":
                            case "--chk":
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
                            case "/size":
                            case "-size":
                            case "--size":
                            case "/s":
                            case "-s":
                            case "--s":
                                int size = Convert.ToInt32(args[count + 1]);
                                if (size < 100000) {
                                    attributes.Size = size;
                                } else {
                                    throw new ArgumentFormatException();
                                }
                                break;
                            case "/whois":
                            case "-whois":
                            case "--whois":
                                opMode = "whois";
                                break;
                            case "/whoami":
                            case "-whoami":
                            case "--whoami": // Current computer location
                                opMode = "whoami";
                                break;
                            case "/location":
                            case "-location":
                            case "--location":
                            case "/loc":
                            case "-loc":
                            case "--loc": // Location lookup
                                opMode = "location";
                                break;
                            case "/listen":
                            case "-listen":
                            case "--listen":
                            case "/li":
                            case "-li":
                            case "--li": // Listen for ICMP packets
                                opMode = "listening";
                                break;
                            case "/graph":
                            case "-graph":
                            case "--graph":
                            case "/g":
                            case "-g":
                            case "--g": // Graph view
                                opMode = "graphing";
                                break;
                            case "/compact":
                            case "-compact":
                            case "--compact":
                            case "/cg":
                            case "-cg":
                            case "--cg": // Compact graph view
                                opMode = "compactgraph";
                                break;
                            case "/flood":
                            case "-flood":
                            case "--flood":
                            case "/fl":
                            case "-fl":
                            case "--fl": // Flood
                                opMode = "flooding";
                                break;
                            case "/scan":
                            case "-scan":
                            case "--scan":
                            case "/sc":
                            case "-sc":
                            case "--sc": // Scan
                                opMode = "scanning";
                                break;
                            default:
                                // Check for invalid argument 
                                if ((args[count].Contains("--") || args[count].Contains("/") || args[count].Contains("-"))
                                    && !opMode.Equals("scanning") // (ignore if scanning)
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
                return;
            } catch (OverflowException) {
                PowerPing.Display.Error("Overflow while converting", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing " + args[curArg] + ">>>" + args[curArg + 1] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for help.");
                Helper.Pause();
                return;
            } catch (ArgumentFormatException) {
                PowerPing.Display.Error("Invalid argument or incorrect parameter", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for help.");
                Helper.Pause();
                return;
            } catch (Exception e) {
                PowerPing.Display.Error("A " + e.GetType().ToString().Split('.').Last() + " Exception Occured", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help\" or \"PowerPing /?\" for more info.");
                Helper.Pause();
                return;
            }

            // Find address
            if (opMode.Equals("") || opMode.Equals("flooding") || opMode.Equals("graphing") 
                || opMode.Equals("compactGraph") || opMode.Equals("location") || opMode.Equals("whois")) {

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

            // Add Control C event handler 
            if (opMode.Equals("") || opMode.Equals("flooding") || opMode.Equals("graphing") 
                || opMode.Equals("compactgraph") || opMode.Equals("scanning") || opMode.Equals("listening")) {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
            }

            // Select correct function using opMode 
            Ping p = new Ping(cancellationTokenSource.Token);
            Graph g;
            switch (opMode) {
                case "listening":
                    p.Listen();
                    break;
                case "location":
                    PowerPing.Lookup.AddressLocation(attributes.Host, true);
                    break;
                case "whoami":
                    PowerPing.Lookup.AddressLocation("", true);
                    break;
                case "whois":
                    PowerPing.Lookup.QueryWhoIs(attributes.Host);
                    break;
                case "graphing":
                    g = new Graph(attributes.Host, cancellationTokenSource.Token);
                    g.Start();
                    break;
                case "compactgraph":
                    g = new Graph(attributes.Host, cancellationTokenSource.Token);
                    g.CompactGraph = true;
                    g.Start();
                    break;
                case "flooding":
                    p.Flood(attributes.Host);
                    break;
                case "scanning":
                    p.Scan(args.Last());
                    break;
                default:
                    // Send ping normally
                    p.Send(attributes);
                    break;
            }

            // Reset console colour
            Display.ResetColor();
            Console.CursorVisible = true;
        }

        /// <summary>
        /// Runs when Exit or Cancel event fires (normally when Ctrl-C)
        /// is pressed. Used to clean up and stop operations when exiting
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ExitHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Cancel termination
            args.Cancel = true;

            // Request currently running job to finish up
            cancellationTokenSource.Cancel();
        }
    }
}
