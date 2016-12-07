using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            bool infinite = false;
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
                        case "--c": // Ping count
                            num = Convert.ToInt16(args[count + 1]);
                            break;
                        case "--t": // Infinitely send
                            infinite = true;
                            break;
                        case "--w": // Timeout
                            timeout = Convert.ToInt16(args[count + 1]);
                            break;
                        case "--m": // Message
                            message = args[count + 1];
                            break;
                        case "--whoami":
                            Macros.whoami();
                            break;
                        case "--location":
                            Macros.getAddressLocation(args[count + 1], true);
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
            p.interval = 1000;
            p.send();

            p.displayStatistics();
        }

        private static void displayHelpMsg()
        {
            Console.WriteLine("PowerPing V1.0 - Advanced Ping");
            Console.WriteLine("\nUsage: PowerPing [--t] [--c count] [--w timeout] [--m message] target_name");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("     --t             Ping the target until stopped (Control-C to stop)");
            Console.WriteLine("     --c count       Number of pings to send");
            Console.WriteLine("     --w timeout     Time to wait for reply (in milliseconds)");
            Console.WriteLine("     --m message     Ping packet message");
            Console.WriteLine("\nWritten by Matthew Carney [matthewcarney64@gmail.com] =^-^=");
        }

        protected static void exitHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Stop ping process and display stats when control c pressed
            p.displayStatistics();
        }
    }
}
