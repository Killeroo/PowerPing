using System;

namespace PowerPing
{
    class Program
    {
        // Global variable setup
        private static Ping p = new Ping();

        static void Main(string[] args)
        {
            // Local variables 
            bool addrFound = false;

            Console.WriteLine();

            p.Scan();

            Console.Read();

            // Check if no arguments
            if (args.Length == 0)
            {
                PowerPing.Display.displayHelpMsg();
                return;
            }

            // Loop through arguments
            try
            {
                for (int count = 0; count < args.Length; count++)
                {
                    switch (args[count])
                    { 
                        case "/c":
                        case "-c":
                        case "--c": // Ping count
                            p.count = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/t":
                        case "-t":
                        case "--t": // Infinitely send
                            p.continous = true;
                            break;
                        case "/w":
                        case "-w":
                        case "--w": // Timeout
                            p.timeout = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/m":
                        case "-m":
                        case "--m": // Message
                            if (args[count + 1].Contains("--"))
                                throw new Exception();
                            p.message = args[count + 1];
                            break;
                        case "/i":
                        case "-i":
                        case "--i": // Time To Live
                            p.ttl = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/in":
                        case "-in":
                        case "--in":
                            p.interval = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/4":
                        case "-4":
                        case "--4": // Force ping with IPv4
                            if (p.forceV6)
                                // Reset IPv4 force if already set (change force both v4 and v6)
                                p.forceV6 = false;
                            p.forceV4 = true;
                            break;
                        case "/6":
                        case "-6":
                        case "--6": // Force ping with IPv6
                            if (p.forceV4)
                                // Reset IPv4 force if already set
                                p.forceV4 = false;
                            p.forceV6 = true;
                            break;
                        case "/?":
                        case "-?":
                        case "--?": // Display help message
                            PowerPing.Display.displayHelpMsg();
                            Environment.Exit(0);
                            break;
                        case "/whoami":
                        case "-whoami":
                        case "--whoami": // Current computer location
                            Macros.whoami();
                            Environment.Exit(0);
                            break;
                        case "/location":
                        case "-location":
                        case "--location": // Location lookup
                            Macros.getAddressLocation(args[count + 1], true);
                            Environment.Exit(0);
                            break;
                        case "/listen":
                        case "-listen":
                        case "--listen": // Listen for ICMP packets
                            p.Listen();
                            Environment.Exit(0);
                            break;
                        case "/graph":
                        case "-graph":
                        case "--graph": // Graph view
                            Graph g = new Graph(args[count + 1]);
                            g.start();
                            break;
                        default:
                            if ((count == args.Length - 1 || count == 0) && !addrFound)
                            { // Assume first or last argument is address
                                p.address = args[count];
                                addrFound = true;
                            }
                            if (args[count].Contains("--"))
                                throw new Exception();
                            break;
                    }
                }
            }
            catch (IndexOutOfRangeException)
            {
                PowerPing.Display.displayError("Incorrect Argument Usage");
                PowerPing.Display.displayHelpMsg();
                return;
            }
            catch (FormatException)
            {
                PowerPing.Display.displayError("Incorrect Argument Format");
                PowerPing.Display.displayHelpMsg();
                return;
            }
            catch (Exception)
            {
                PowerPing.Display.displayError("Invalid Argument or General Error Occured");
                PowerPing.Display.displayHelpMsg();
                return;
            }

            // only add Control C event handler when sending standard ping
            // (So statistics can still be displayed when ping interupted)
            Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);

            // Send ping
            p.Send();
        }

        /// <summary>
        /// Event handler for control - c
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected static void exitHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Cancel termination
            args.Cancel = true;

            // Stop ping
            p.Stop();

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

        }
    }
}
