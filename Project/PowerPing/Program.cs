using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
//using System.Globalization;

namespace PowerPing
{
    class Program
    {
        private static Ping p;

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

            // Add Control C event
            Console.CancelKeyPress += new ConsoleCancelEventHandler(exitHandler);
            Console.WriteLine();

            // Check if no arguments
            if (args.Length == 0)
            {
                displayHelpMsg();
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
                            displayHelpMsg();
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
                Console.WriteLine("ERROR: Incorrect Argument Usage\n");
                displayHelpMsg();
                return;
            }
            catch (FormatException)
            {
                Console.WriteLine("ERROR: Incorrect Argument Format\n");
                displayHelpMsg();
                return;
            }
            catch (Exception)
            {
                Console.WriteLine("ERROR: Invalid Argument or General Error Occured\n");
                displayHelpMsg();
                return;
            }

            // Send ping
            p = new Ping();
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
            if (!p.cancelFlag)
                p.displayStatistics();
        }

        private static void displayHelpMsg()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string version = Assembly.GetExecutingAssembly().GetName().Name + " Version " + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ")";
            Console.WriteLine(version);
            Console.WriteLine("\nDescription:");
            Console.WriteLine("     This advanced ping utility provides geoip querying, ICMP packet info");
            Console.WriteLine("     and result colourization.");
            Console.WriteLine("\nUsage: PowerPing [--?] | [--whoami] | [--location address] | [--t] ");
            Console.WriteLine("                 [--c count] [--w timeout] [--m message] [--i TTL]");
            Console.WriteLine("                 [--in interval] [--4] target_name");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("     --?             Displays this help message");
            Console.WriteLine("     --t             Ping the target until stopped (Control-C to stop)");
            Console.WriteLine("     --c count       Number of pings to send");
            Console.WriteLine("     --w timeout     Time to wait for reply (in milliseconds)");
            Console.WriteLine("     --m message     Ping packet message");
            Console.WriteLine("     --i ttl         Time To Live");
            Console.WriteLine("     --in interval   Interval between each ping (in milliseconds)");
            Console.WriteLine("     --4             Force using IPv4");
            //Console.WriteLine("     --6             Force using IPv6");
            Console.WriteLine();
            Console.WriteLine("     --whoami        Location info for current host");
            Console.WriteLine("     --location addr Location info for an address");
            Console.WriteLine("\nWritten by Matthew Carney [matthewcarney64@gmail.com] =^-^=");
            Console.WriteLine("Find the project here [https://github.com/Killeroo/PowerPing]\n");
            PowerPing.Macros.pause();
        }

        protected static void exitHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Send cancel request
            p.cancelFlag = true;

            // Stop ping process and display stats when control c pressed
            p.displayStatistics();
        }
    }
}
