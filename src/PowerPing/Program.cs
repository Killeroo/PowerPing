/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Written by Matthew Carney [matthewcarney64@gmail.com]
 * ************************************************************************/
/*
MIT License - PowerPing 

Copyright (c) 2019 Matthew Carney

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
using System.Threading;

namespace PowerPing
{
    static class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Main entry point of PowerPing
        /// Parses arguments and runs operations
        /// </summary>
        /// <param name="args">Program arguments</param>
        static void Main(string[] args)
        {
            // Local variables 
            PingAttributes attributes = new PingAttributes();
            attributes.Address = "";

            // Setup console
            Display.DefaultForegroundColor = Console.ForegroundColor;
            Display.DefaultBackgroundColor = Console.BackgroundColor;

            // Show current version info
            //Display.Version();

            // Check if no arguments
            if (args.Length == 0) {
                PowerPing.Display.Help();
                return;
            }

            // Parse command line arguments
            if (!CommandLine.Parse(args, ref attributes)) {
                Helper.ErrorAndExit("Problem parsing arguments, use \"PowerPing /help\" or \"PowerPing /?\" for help.");
            }

            // Find address/host in arguments
            if (attributes.Operation != PingOperation.Whoami &&
                attributes.Operation != PingOperation.Listen) {
                if (!CommandLine.FindAddress(args, ref attributes)) {
                    Helper.ErrorAndExit("Could not find correctly formatted address, please check and try again");
                }
            }

            // Add Control C event handler 
            if (attributes.Operation != PingOperation.Whoami &&
                attributes.Operation != PingOperation.Location &&
                attributes.Operation != PingOperation.Whois) { 
                Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitHandler);
            }

            // Select correct function using opMode 
            Ping p = new Ping(cancellationTokenSource.Token);
            Graph g;
            switch (attributes.Operation) {
                case PingOperation.Listen:
                    p.Listen();
                    break;
                case PingOperation.Location:
                    PowerPing.Lookup.AddressLocation(attributes.InputtedAddress, true);
                    break;
                case PingOperation.Whoami:
                    PowerPing.Lookup.AddressLocation("", true);
                    break;
                case PingOperation.Whois:
                    PowerPing.Lookup.QueryWhoIs(attributes.InputtedAddress);
                    break;
                case PingOperation.Graph:
                    g = new Graph(attributes.InputtedAddress, cancellationTokenSource.Token);
                    g.Start();
                    break;
                case PingOperation.CompactGraph:
                    g = new Graph(attributes.InputtedAddress, cancellationTokenSource.Token);
                    g.CompactGraph = true;
                    g.Start();
                    break;
                case PingOperation.Flood:
                    PowerPing.Flood.Start(attributes.InputtedAddress, cancellationTokenSource.Token);
                    break;
                case PingOperation.Scan:
                    PowerPing.Scan.Start(attributes.InputtedAddress, cancellationTokenSource.Token);
                    break;
                case PingOperation.Normal:
                    // Send ping normally
                    p.Send(attributes);
                    break;
                default:
                    Helper.ErrorAndExit("Could not determine ping operation");
                    break;
            }

            // Reset console colour
            Display.ResetColor();
            Console.CursorVisible = true;
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
            cancellationTokenSource.Cancel();
        }
    }
}
