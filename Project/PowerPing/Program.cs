using System;

namespace PowerPing
{
    class Program
    {
        // Global variable setup
        private static Ping p = new Ping();
        private static bool sendFlag = false; // Indicates if a normal ping operation is being performed
        private static bool cancelled = false; // Indicates if cancel event has been called

        static void Main(string[] args)
        {
            // Local variables 
            bool addrFound = false;

            //Graph g = new Graph("8.8.8.8");
            //g.start();

            //Console.Read();

            // Add Control C event handler
            Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
            Console.WriteLine();

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
                            p.listen();
                            Environment.Exit(0);
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

            // Send ping
            sendFlag = true;
            p.send();

            // Display stats after ping(s) has been sent
            if (!cancelled)
                PowerPing.Display.displayStatistics(p);
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

            // Set cancel flag
            cancelled = true;

            // Stop ping
            p.stop();

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

            // Display stats if a normal ping is being sent
            if (sendFlag)
                PowerPing.Display.displayStatistics(p);
        }
    }
}
