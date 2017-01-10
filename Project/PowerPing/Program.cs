using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing
{
    class Program
    {
        // Global variable setup
        private static Ping p = new Ping();
        private static bool listening = false;

        static void Main(string[] args)
        {
            // Local variables 
            bool addrFound = false;

            // Default ping values
            int num = 5;
            int timeout = 3000;
            int ttl = 255;
            int interval = 1000;
            bool infinite = false;
            bool forceV4 = false;
            bool forceV6 = false;
            string address = "";
            string message = "R U Alive?";

            Graph g = new Graph();
            g.start();

            Console.Read();

            // Add Control C event
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
                            num = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/t":
                        case "-t":
                        case "--t": // Infinitely send
                            infinite = true;
                            break;
                        case "/w":
                        case "-w":
                        case "--w": // Timeout
                            timeout = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/m":
                        case "-m":
                        case "--m": // Message
                            message = args[count + 1];
                            break;
                        case "/i":
                        case "-i":
                        case "--i": // Time To Live
                            ttl = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/in":
                        case "-in":
                        case "--in":
                            interval = Convert.ToInt16(args[count + 1]);
                            break;
                        case "/4":
                        case "-4":
                        case "--4": // Force ping with IPv4
                            if (forceV6)
                                // Reset IPv4 force if already set (change force both v4 and v6)
                                forceV6 = false;
                            forceV4 = true;
                            break;
                        case "/6":
                        case "-6":
                        case "--6": // Force ping with IPv6
                            if (forceV4)
                                // Reset IPv4 force if already set
                                forceV4 = false;
                            forceV6 = true;
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
                            listening = true;
                            p.listen();
                            Environment.Exit(0);
                            break;
                        default:
                            if ((count == args.Length - 1 || count == 0) && !addrFound)
                            { // Assume first or last argument is address
                                address = args[count];
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
            p.address = address;
            p.count = num;
            p.message = message;
            p.timeout = timeout;
            p.continous = infinite;
            p.interval = interval;
            p.ttl = ttl;
            p.forceV4 = forceV4;
            p.forceV6 = forceV6;
            p.send();

            // Only display stats if ping hasn't been cancelled
            //if (!p.cancelFlag)
                //p.displayStatistics();
        }

        protected static void exitHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Stop ping
            p.stop();

            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;

            // Stop ping process and display stats when control c pressed
            //if (!listening)
                //p.displayStatistics();
        }
    }
}
