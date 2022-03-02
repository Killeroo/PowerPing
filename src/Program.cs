/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing 
 *************************************************************************/

using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace PowerPing
{
    static class Program
    {
        private static readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Main entry point of PowerPing
        /// Parses arguments and runs operations
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args)
        {
            PingAttributes inputtedAttributes = new PingAttributes();
            DisplayConfiguration displayConfiguration = new DisplayConfiguration();

            // Show current version info
            //Display.Version();

            // Check if no arguments
            if (args.Length == 0) {
                ConsoleDisplay.Help();
                return;
            }

            // Parse command line arguments
            if (!CommandLine.Parse(args, ref inputtedAttributes, ref displayConfiguration)) {
                Helper.ErrorAndExit("Problem parsing arguments, use \"PowerPing /help\" or \"PowerPing /?\" for help.");
            }

            Helper.RequireInput = displayConfiguration.RequireInput;

            // Find address/host in arguments
            if (inputtedAttributes.Operation != PingOperation.Whoami &&
                inputtedAttributes.Operation != PingOperation.Listen) {
                if (!CommandLine.FindAddress(args, ref inputtedAttributes)) {
                    Helper.ErrorAndExit("Could not find correctly formatted address, please check and try again");
                }
            }

            // Perform DNS lookup on inputted address
           // inputtedAttributes.ResolvedAddress = Lookup.QueryDNS(inputtedAttributes.InputtedAddress, inputtedAttributes.UseICMPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6);

            // Add Control C event handler 
            if (inputtedAttributes.Operation != PingOperation.Whoami &&
                inputtedAttributes.Operation != PingOperation.Location &&
                inputtedAttributes.Operation != PingOperation.Whois) { 
                Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
            }

            // Set configuration
            ConsoleDisplay.Configuration = displayConfiguration;

            // Add handler to display ping events
            ConsoleMessageHandler consoleHandler = new ConsoleMessageHandler(displayConfiguration, m_CancellationTokenSource.Token);
            
            // Select correct function using opMode 
            Ping p;
            Graph g;
            switch (inputtedAttributes.Operation) {
                case PingOperation.Listen:
                    // If we find an address then pass it to listen, otherwise start it without one
                    if (CommandLine.FindAddress(args, ref inputtedAttributes)) {
                        Listen.Start(m_CancellationTokenSource.Token, inputtedAttributes.InputtedAddress);
                    } else {
                        Listen.Start(m_CancellationTokenSource.Token);
                    }
                    break;
                case PingOperation.Location:
                    Console.WriteLine(Lookup.GetAddressLocationInfo(inputtedAttributes.InputtedAddress, false));
                    if (displayConfiguration.RequireInput)
                    {
                        Helper.WaitForUserInput();
                    }
                    break;
                case PingOperation.Whoami:
                    Console.WriteLine(Lookup.GetAddressLocationInfo("", true));
                    if (displayConfiguration.RequireInput)
                    {
                        Helper.WaitForUserInput();
                    }
                    break;
                case PingOperation.Whois:
                    Lookup.QueryWhoIs(inputtedAttributes.InputtedAddress);
                    break;
                case PingOperation.Graph:
                    g = new Graph(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    g.Start();
                    break;
                case PingOperation.CompactGraph:
                    g = new Graph(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    g.CompactGraph = true;
                    g.Start();
                    break;
                case PingOperation.Flood:
                    Flood f = new Flood();
                    f.Start(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    break;
                case PingOperation.Scan:
                    Scan.Start(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    break;
                case PingOperation.Normal:
                    var addresses = CommandLine.FindAddresses(args);

                    // Send ping normally
                    p = new Ping(inputtedAttributes, m_CancellationTokenSource.Token, consoleHandler);
                    PingResults results = p.Send();
                    
                    break;
                default:
                    Helper.ErrorAndExit("Could not determine ping operation");
                    break;
            }

            // Reset console colour
            ConsoleDisplay.ResetColor();
            try { Console.CursorVisible = true; } catch (Exception) { }
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
            m_CancellationTokenSource.Cancel();

            // Reset colour on exit
            Console.ResetColor();
        }
    }
}
