/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Written by Matthew Carney [matthewcarney64@gmail.com]
 * ************************************************************************/

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
            bool addrFound = false;
            PingAttributes attributes = new PingAttributes();
            attributes.Address = "";

            // Setup console
            Display.DefaultForegroundColor = Console.ForegroundColor;
            Display.DefaultBackgroundColor = Console.BackgroundColor;

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
                    continue;

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
                    switch (args[count])
                    { 
                        case "/c":
                        case "-c":
                        case "--c": // Ping count
                            attributes.Count = Convert.ToInt32(args[count + 1]);
                            break;
                        case "/t":
                        case "-t":
                        case "--t": // Infinitely send
                            attributes.Continous = true;
                            break;
                        case "/w":
                        case "-w":
                        case "--w": // Timeout
                            attributes.Timeout = Convert.ToInt32(args[count + 1]);
                            break;
                        case "/m":
                        case "-m":
                        case "--m": // Message
                            if (args[count + 1].Contains("--") || args[count + 1].Contains("//") || args[count + 1].Contains("-"))
                                throw new FormatException();
                            attributes.Message = args[count + 1];
                            /// TODO: Add Length check
                            break;
                        case "/i":
                        case "-i":
                        case "--i": // Time To Live
                            attributes.Ttl = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/in":
                        case "-in":
                        case "--in": // Interval
                            attributes.Interval = Convert.ToInt32(args[count + 1]);
                            break;
                        case "/pt":
                        case "-pt":
                        case "--pt": // Ping type
                            attributes.Type = Convert.ToByte(args[count + 1]);
                            break;
                        case "/pc":
                        case "-pc":
                        case "--pc": // Ping code
                            attributes.Code = Convert.ToByte(args[count + 1]);
                            break;
                        case "/dm":
                        case "-dm":
                        case "--dm": // Display packet message
                            Display.DisplayMessage = true;
                            break;
                        case "/4":
                        case "-4":
                        case "--4": // Force ping with IPv4
                            if (attributes.ForceV6)
                                // Reset IPv4 force if already set (change force both v4 and v6)
                                attributes.ForceV6 = false;
                            attributes.ForceV4 = true;
                            break;
                        case "/6":
                        case "-6":
                        case "--6": // Force ping with IPv6
                            if (attributes.ForceV4)
                                // Reset IPv4 force if already set
                                attributes.ForceV4 = false;
                            attributes.ForceV6 = true;
                            break;
                        case "/?":
                        case "-?":
                        case "--?": // Display help message
                            PowerPing.Display.Help();
                            Environment.Exit(0);
                            break;
                        case "/sh":
                        case "-sh":
                        case "--sh": // Use short hand messages
                            Display.Short = true;
                            break;
                        case "/nc":
                        case "-nc":
                        case "--nc": // No color mode
                            Display.NoColor = true;
                            break;
                        case "/ts":
                        case "-ts":
                        case "--ts": // Display timestamp
                            Display.TimeStamp = true;
                            break;
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
                                    throw new IndexOutOfRangeException();
                            }
                            break;
                        case "/whoami":
                        case "-whoami":
                        case "--whoami": // Current computer location
                            Helper.whoami();
                            Environment.Exit(0);
                            break;
                        case "/loc":
                        case "-loc":
                        case "--loc": // Location lookup
                            if (attributes.Address == "")
                                throw new FormatException();
                            Helper.GetAddressLocation(attributes.Address, true);
                            Environment.Exit(0);
                            break;
                        case "/li":
                        case "-li":
                        case "--li": // Listen for ICMP packets
                            p.Listen();
                            Environment.Exit(0);
                            break;
                        case "/g":
                        case "-g":
                        case "--g": // Graph view
                            if (attributes.Address == "")
                                throw new FormatException();
                            g = new Graph(attributes.Address); 
                            g.Start();
                            Environment.Exit(0);
                            break;
                        case "/cg":
                        case "-cg":
                        case "--cg": // Compact graph view
                            if (attributes.Address == "")
                                throw new FormatException();
                            g = new Graph(attributes.Address);
                            g.CompactGraph = true;
                            g.Start();
                            Environment.Exit(0);
                            break;
                        case "/fl":
                        case "-fl":
                        case "--fl": // Flood
                            if (attributes.Address == "")
                                throw new FormatException();
                            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
                            p.Flood(attributes.Address);
                            Environment.Exit(0);
                            break;
                        case "/sc":
                        case "-sc":
                        case "--sc":
                            //if (attributes.Address == "")
                            //    throw new FormatException();
                            /// Check for presense of -
                            p.Scan(args[count + 1]);
                            Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
                            Environment.Exit(0);
                            break;
                        default:
                            if (args[count].Contains("--") || args[count].Contains("/") || args[count].Contains("-"))
                                throw new Exception();
                            break;
                    }
                }

                if (attributes.Address == "")
                    throw new FormatException();
            }
            catch (IndexOutOfRangeException)
            {
                PowerPing.Display.Error("Incorrect Argument Usage");
                PowerPing.Display.Help();
                return;
            }
            catch (FormatException)
            {
                PowerPing.Display.Error("Incorrect Argument Format");
                PowerPing.Display.Help();
                return;
            }
            catch (Exception)
            {
                PowerPing.Display.Error("Invalid Argument or General Error Occured");
                PowerPing.Display.Help();
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
                g.Stop();

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

        }
    }
}
