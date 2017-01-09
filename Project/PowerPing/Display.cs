using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* Display Class */
// Responsible for printing and displaying of ping results and information

namespace PowerPing
{

    class Display
    {
        // Type code values
        private static string[] destUnreachableCodeValues = new string[] {"Network unreachable", "Host unreachable", "Protocol unreachable",
                                                        "Port unreachable", "Fragmentation needed & DF flag set", "Source route failed",
                                                        "Destination network unkown", "Destination host unknown", "Source host isolated",
                                                        "Communication with destination network prohibited", "Communication with destination network prohibited",
                                                        "Network unreachable for ICMP", "Host unreachable for ICMP"};
        private static string[] redirectCodeValues = new string[] {"Packet redirected for the network", "Packet redirected for the host",
                                                 "Packet redirected for the ToS & network", "Packet redirected for the ToS & host"};
        private static string[] timeExceedCodeValues = new string[] { "TTL expired in transit", "Fragment reassembly time exceeded" };
        private static string[] badParameterCodeValues = new string[] { "IP header pointer indicates error", "IP header missing an option", "Bad IP header length" };


        /// <summary>
        /// Display information about reply ping packet
        /// </summary>
        /// <param name="packet">Reply packet</param>
        /// <param name="address">Reply address</param>
        /// <param name="index">Sequence number</param>
        /// <param name="replyTime">Time taken before reply recieved in milliseconds</param>
        public static void displayReply(ICMP packet, String address, int index, long replyTime)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write("Reply from: {0} ", address);
            Console.Write("Seq={0} ", index);
            Console.Write("Type=");
            switch (packet.type)
            {
                case 0:
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write("ECHO REPLY");
                    break;
                case 8:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("ECHO REQUEST");
                    break;
                case 3:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(packet.code > 13 ? "DESTINATION UNREACHABLE" : destUnreachableCodeValues[packet.code].ToUpper());
                    break;
                case 4:
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("SOURCE QUENCH");
                    break;
                case 5:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write(packet.code > 3 ? "PING REDIRECT" : redirectCodeValues[packet.code].ToUpper());
                    break;
                case 9:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("ROUTER ADVERTISEMENT");
                    break;
                case 10:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("ROUTER SOLICITATION");
                    break;
                case 11:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(packet.code > 1 ? "TIME EXCEEDED" : timeExceedCodeValues[packet.code].ToUpper());
                    break;
                case 12:
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.Write(packet.code > 2 ? "PARAMETER PROBLEM" : badParameterCodeValues[packet.code].ToUpper());
                    break;
                case 13:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("TIMESTAMP REQUEST");
                    break;
                case 14:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("TIMESTAMP REPLY");
                    break;
                case 15:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("INFORMATION REQUEST");
                    break;
                case 16:
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.Write("INFORMATION REPLY");
                    break;
                default:
                    Console.BackgroundColor = ConsoleColor.DarkYellow;
                    Console.Write("UNKNOWN TYPE");
                    break;
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" time=");
            if (replyTime <= 100L)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (replyTime <= 500L)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (replyTime > 500L)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("{0}ms", replyTime < 1 ? "<1" : replyTime.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        /// <summary>
        /// Displays statistics for a ping object
        /// </summary>
        public void displayStatistics(Ping ping)
        {
            // Reset console colour
            Console.BackgroundColor = ConsoleColor.Black;

            // Display stats
            double percent = (double)ping.lost / ping.sent;
            percent = Math.Round(percent * 100, 1);
            Console.WriteLine("\nPing statistics for {0}:", ping.address);

            Console.Write("     Packet: Sent ");
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[" + ping.sent + "]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(", Recieved ");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[" + ping.recieved + "]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(", Lost ");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write("[" + ping.lost + "]");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" (" + percent + "% loss)");

            Console.WriteLine("Response times:");
            Console.WriteLine("     Minimum [{0}ms], Maximum [{1}ms]", ping.min, ping.max);

            Console.WriteLine("Total elapsed time (HH:MM:SS.FFF): {0:hh\\:mm\\:ss\\.fff}", ping.operationTime);
            Console.WriteLine();

            // Confirm to exit
            PowerPing.Macros.pause(true);
        }
    }
}
