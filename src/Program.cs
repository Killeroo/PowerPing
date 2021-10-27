/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Written by Matthew Carney [matthewcarney64@gmail.com]
 * ************************************************************************/
/*
MIT License - PowerPing 

Copyright (c) 2021 Matthew Carney

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
            // User inputted attributes
            PingAttributes inputtedAttributes = new PingAttributes();

            // Setup console
            Display.DefaultForegroundColor = Console.ForegroundColor;
            Display.DefaultBackgroundColor = Console.BackgroundColor;

            // Show current version info
            //Display.Version();

            // Check if no arguments
            if (args.Length == 0) {
                Display.Help();
                Helper.WaitForUserInput();
                return;
            }

            // Parse command line arguments
            if (!CommandLine.Parse(args, ref inputtedAttributes)) {
                Helper.ErrorAndExit("Problem parsing arguments, use \"PowerPing /help\" or \"PowerPing /?\" for help.");
            }

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
                    Helper.WaitForUserInput();
                    break;
                case PingOperation.Whoami:
                    Console.WriteLine(Lookup.GetAddressLocationInfo("", true));
                    Helper.WaitForUserInput();
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
                    Flood.Start(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    break;
                case PingOperation.Scan:
                    Scan.Start(inputtedAttributes.InputtedAddress, m_CancellationTokenSource.Token);
                    break;
                case PingOperation.Normal:
                    var addresses = CommandLine.FindAddresses(args);
                    if (addresses.Count == 1) {
                        // Send ping normally
                        p = new Ping(inputtedAttributes, m_CancellationTokenSource.Token);
                        PingResults results = p.Send();
                        Display.PingResults(inputtedAttributes, results);
                    } else {
                        Thread[] pingThreads = new Thread[addresses.Count];
                        Display.MultiThreaded = true;
                        
                        for (int i = 0; i < 2; i++) {
                            Console.WriteLine(i);
                        }

                        for (int i = 0; i < addresses.Count; i++) {
                            pingThreads[i] = new Thread(() => {
                                PingAttributes attrs = new PingAttributes(inputtedAttributes);
                                attrs.InputtedAddress = addresses[i];
                                Ping pp = new Ping(attrs, m_CancellationTokenSource.Token);
                                PingResults results = pp.Send();
                                Display.PingResults(attrs, results);
                            });
                            pingThreads[i].Start();
                        }

                    }


                    break;
                default:
                    Helper.ErrorAndExit("Could not determine ping operation");
                    break;
            }

            // Reset console colour
            Display.ResetColor();
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
