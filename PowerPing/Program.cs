/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Written by Matthew Carney [matthewcarney64@gmail.com]
 * ************************************************************************/
/*
MIT License

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

// dsipaly Powerping version at start

namespace PowerPing
{
    class Program
    {
        private static Ping p = new Ping();
        private static Graph g = null;

        static void Main(string[] args)
        {
            // Local variables 
            int curArg = 0;
            bool addrFound = false;
            PingAttributes attributes = new PingAttributes();
            attributes.Address = "";

            // Setup console
            Display.DefaultForegroundColor = Console.ForegroundColor;
            Display.DefaultBackgroundColor = Console.BackgroundColor;

            // Show current version info
            Display.Version();

            // Check if no arguments
            if (args.Length == 0)
            {
                PowerPing.Display.Help();
                return;
            }

            // Find address first
            for (int count = 0; count < args.Length; count++)
            {
                if (args[count].Contains("--") || args[count].Contains("/") || args[count].Contains("-"))
                {
                    // Scan address will contain '-' so ignore it if scan argument is present
                    if (args[0] == "/sc" || args[0] == "-sc" || args[0] == "--sc" || args[0] == "/scan" || args[0] == "-scan" || args[0] == "--scan" ||
                        args[args.Length - 1] == "/sc" || args[args.Length - 1] == "-sc" || args[args.Length - 1] == "--sc" || args[args.Length - 1] == "/scan" || args[args.Length - 1] == "-scan" || args[args.Length - 1] == "--scan")
                    {
                        attributes.Address = args[count];
                        addrFound = true;
                    }
                    else
                    {
                        continue;
                    }
                }

                if ((count == args.Length - 1 || count == 0) && !addrFound)
                { // Assume first or last argument is address
                    attributes.Address = args[count];
                    addrFound = true;
                }
            }

            // Loop through other arguments
            try
            {
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
                            Display.Version();
                            Environment.Exit(0);
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
                            if (args[count + 1].Contains("--") || args[count + 1].Contains("//") || args[count + 1].Contains("-"))
                                throw new IndexOutOfRangeException();
                            attributes.Message = args[count + 1];
                            /// TODO: Add Length check
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
                            break;
                        case "/type":
                        case "-type":
                        case "--type":
                        case "/pt":
                        case "-pt":
                        case "--pt": // Ping type
                            attributes.Type = Convert.ToByte(args[count + 1]);
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
                            Display.DisplayMessage = true;
                            break;
                        case "/ipv4":
                        case "-ipv4":
                        case "--ipv4":
                        case "/4":
                        case "-4":
                        case "--4": // Force ping with IPv4
                            if (attributes.ForceV6)
                                // Reset IPv4 force if already set (change force both v4 and v6)
                                attributes.ForceV6 = false;
                            attributes.ForceV4 = true;
                            break;
                        case "/ipv6":
                        case "-ipv6":
                        case "--ipv6":
                        case "/6":
                        case "-6":
                        case "--6": // Force ping with IPv6
                            if (attributes.ForceV4)
                                // Reset IPv4 force if already set
                                attributes.ForceV4 = false;
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
                        case "/noinput":
                        case "-noinput":
                        case "--noinput":
                        case "/ni":
                        case "-ni":
                        case "--ni": // No input mode
                            Display.NoInput = true;
                            break;
                        case "/timestamp":
                        case "-timestamp":
                        case "--timestamp":
                        case "/ts":
                        case "-ts":
                        case "--ts": // Display timestamp
                            Display.TimeStamp = true;
                            break;
                        case "/timing":
                        case "-timing":
                        case "--timing":
                        case "/ti":
                        case "-ti":
                        case "--ti": // Timing option
                            switch (args[count + 1 ].ToLower()) //Convert.ToInt16(args[count + 1]))
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
                                default: // Unknown timing type
                                    throw new FormatException();
                            }
                            break;
                        case "/whoami":
                        case "-whoami":
                        case "--whoami": // Current computer location
                            Helper.whoami();
                            Environment.Exit(0);
                            break;
                        case "/location":
                        case "-location":
                        case "--location":
                        case "/loc":
                        case "-loc":
                        case "--loc": // Location lookup
                            if (attributes.Address == "")
                                throw new ApplicationException();
                            Helper.GetAddressLocation(attributes.Address, true);
                            Environment.Exit(0);
                            break;
                        case "/listen":
                        case "-listen":
                        case "--listen":
                        case "/li":
                        case "-li":
                        case "--li": // Listen for ICMP packets
                            p.Listen();
                            Environment.Exit(0);
                            break;
                        case "/graph":
                        case "-graph":
                        case "--graph":
                        case "/g":
                        case "-g":
                        case "--g": // Graph view
                            if (attributes.Address == "")
                                throw new ApplicationException();
                            g = new Graph(attributes.Address); 
                            g.Start();
                            Environment.Exit(0);
                            break;
                        case "/compact":
                        case "-compact":
                        case "--compact":
                        case "/cg":
                        case "-cg":
                        case "--cg": // Compact graph view
                            if (attributes.Address == "")
                                throw new ApplicationException();
                            g = new Graph(attributes.Address);
                            g.CompactGraph = true;
                            g.Start();
                            Environment.Exit(0);
                            break;
                        case "/flood":
                        case "-flood":
                        case "--flood":
                        case "/fl":
                        case "-fl":
                        case "--fl": // Flood
                            if (attributes.Address == "")
                                throw new ApplicationException();
                            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
                            p.Flood(attributes.Address);
                            Environment.Exit(0);
                            break;
                        case "/scan":
                        case "-scan":
                        case "--scan":
                        case "/sc":
                        case "-sc":
                        case "--sc": // Scan
                            if (attributes.Address == "" || !attributes.Address.Contains("-"))
                                throw new ApplicationException();//FormatException();
                            p.Scan(args[count + 1]);
                            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
                            Environment.Exit(0);
                            break;
                        default:
                            if (args[count].Contains("--") || args[count].Contains("/") || args[count].Contains("-"))
                                throw new ArgumentException();//Exception();
                            break;
                    }
                }

                if (attributes.Address == "")
                    throw new FormatException();
            }
            catch (IndexOutOfRangeException)
            {
                PowerPing.Display.Error("Missing Argument Parameter", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for help.");
                Helper.Pause();
                return;
            }
            catch (OverflowException)
            {
                PowerPing.Display.Error("Overflow while Converting", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing " + args[curArg] + ">>>" + args[curArg + 1] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for help.");
                Helper.Pause();
                return;
            }
            catch (ArgumentException)
            {
                PowerPing.Display.Error("Invalid Argument", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for more info.");
                Helper.Pause();
                return;
            }
            catch (FormatException)
            {
                PowerPing.Display.Error("Incorrect Argument Usage", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing " + args[curArg] + " >>>" + args[curArg + 1] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for help.");
                Helper.Pause();
                return;
            }
            catch (ApplicationException)
            {
                PowerPing.Display.Error("No Address Provided", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for help.");
                Helper.Pause();
                return;
            }
            catch (Exception)
            {
                PowerPing.Display.Error("A General Error Occured", false, false, false);
                PowerPing.Display.Message(" @ \"PowerPing >>>" + args[curArg] + "<<<\"", ConsoleColor.Red);
                PowerPing.Display.Message("Use \"PowerPing /help \" or \"PowerPing /? \" for more info.");
                Helper.Pause();
                return;
            }


            // Control C event handler 
            // (So statistics can still be displayed when ping interupted)
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);

            // Send ping
            p.Send(attributes);
        }

        protected static void ExitHandler(object sender, ConsoleCancelEventArgs args) // Event handler for control - c
        {
            // Cancel termination
            args.Cancel = true;

            // Stop ping
            p.Stop();

            // Stop graph if it is running
            if (g != null)
            {
                g.Stop();
                Console.CursorTop = g.EndCursorPosY;
            }

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

        }
    }
}
