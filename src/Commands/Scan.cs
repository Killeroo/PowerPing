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
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Linq;

namespace PowerPing 
{
    /// <summary>
    /// ICMP scanning functionality.
    ///
    /// Uses pings to scan a IP address range and identify hosts
    /// that are active.
    ///
    /// Range should be in format 192.0.0.1-255, where - denotes the range
    /// This can be specified at any octlet of the address (192.0.1-100.1.255)
    /// </summary>
    static class Scan 
    {
        private const int THREAD_COUNT = 20;

        /// <summary>
        /// Used to store information on hosts found in scan
        /// </summary>
        public class HostInformation 
        {
            public string Address { get; set; }
            public string HostName { get; set; }
            public double ResponseTime { get; set; }
        }

        private static volatile bool canceled = false;

        public static void Start(string range, CancellationToken cancellationToken)
        {
            List<string> addresses = new List<string>();
            List<HostInformation> activeHosts = new List<HostInformation>();
            Stopwatch timer = new Stopwatch();

            Display.ShowOutput = false;
            

            // Get addresses to scan from range
            addresses = ParseRange(range);

            // Setup addresses and threads
            var splitAddresses = Helper.PartitionList(addresses, THREAD_COUNT);
            Thread[] threads = new Thread[THREAD_COUNT];
            object lockObject = new object();
            int scanned = 0;

            // Run the threads
            timer.Start();
            for (int i = 0; i < THREAD_COUNT; i++) {
                List<string> addrs = splitAddresses[i];
                threads[i] = new Thread(() => {

                    PingAttributes attrs = new PingAttributes();
                    attrs.InputtedAddress = "127.0.0.1"; 
                    attrs.Timeout = 500;
                    attrs.Interval = 0;
                    attrs.Count = 1;

                    Ping ping = new Ping(attrs, cancellationToken);
                    
                    try {
                        foreach (string host in addrs) {

                            // Send ping
                            PingResults results = ping.Send(host);
                            if (results.ScanWasCanceled) {
                                // Cancel was requested during scan
                                throw new OperationCanceledException();
                            }

                            Interlocked.Increment(ref scanned);

                            if (results.Lost == 0 && results.ErrorPackets != 1) {
                                // If host is active, add to list
                                lock (lockObject) {
                                    activeHosts.Add(new HostInformation {
                                        Address = host,
                                        HostName = "",
                                        ResponseTime = results.CurrTime
                                    });
                                }
                            }
                        }
                    } catch (OperationCanceledException) {
                        canceled = true;
                    }
                });

                threads[i].IsBackground = true;
                threads[i].Start();
            }

            // Wait for all threads to exit
            int lastSent = 0 , pingsPerSecond = 0;
            int lastSpeedCheck = 0;
            while (threads.Where(x => x.IsAlive).ToList().Count > 0) {
                int count = 0;
                lock (lockObject) {
                    count = activeHosts.Count;
                }
                
                if (lastSpeedCheck == 5) {
                    pingsPerSecond = Math.Abs((scanned - lastSent));
                    lastSent = scanned;
                    lastSpeedCheck = 0;
                }
                Display.ScanProgress(
                    scanned, 
                    activeHosts.Count, 
                    addresses.Count,
                    pingsPerSecond,
                    timer.Elapsed, 
                    range);

                lastSpeedCheck++;
                Thread.Sleep(200);
            }

            // Display one last time so the bar actually completes 
            // (scan could have completed while the main thread was sleeping)
            Display.ScanProgress(
                scanned,
                activeHosts.Count,
                addresses.Count,
                pingsPerSecond,
                timer.Elapsed,
                range);

            // Exit out when the operation has been canceled
            if (canceled) {
                Display.ScanResults(scanned, false, activeHosts);
                return;
            }

            // Lookup host's name
            Console.WriteLine();
            Console.Write("Looking up host names, one sec...");
            Console.CursorLeft = 0;
            foreach (HostInformation host in activeHosts) {
                string hostName = Helper.RunWithCancellationToken(() => Lookup.QueryHost(host.Address), cancellationToken);
                host.HostName = hostName;
            }
            Console.WriteLine("                                    ");
            Console.CursorTop--;

            Display.ScanResults(scanned, !cancellationToken.IsCancellationRequested, activeHosts);
        }

        /// <summary>
        /// Creates an array of all possible ip addresses from range (range format example: 192.168.1-255)
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private static List<string> ParseRange(string range)
        {
            List<string> scanList = new List<string>(); // List of addresses to scan
            String[] ipSegments = range.Split('.');

            // Holds the ranges for each ip segment
            int[] segLower = new int[4];
            int[] segUpper = new int[4];

            // Work out upper and lower ranges for each segment
            try {
                for (int y = 0; y < 4; y++) {
                    string[] ranges = ipSegments[y].Split('-');
                    segLower[y] = Convert.ToInt16(ranges[0]);
                    segUpper[y] = (ranges.Length == 1) ? segLower[y] : Convert.ToInt16(ranges[1]);
                }
            }
            catch (FormatException) {
                Helper.ErrorAndExit("Scan - Incorrect format [" + range + "], must be specified in format: 192.168.1.1-254");
            }

            // Build list of addresses from ranges
            for (int seg1 = segLower[0]; seg1 <= segUpper[0]; seg1++) {
                for (int seg2 = segLower[1]; seg2 <= segUpper[1]; seg2++) {
                    for (int seg3 = segLower[2]; seg3 <= segUpper[2]; seg3++) {
                        for (int seg4 = segLower[3]; seg4 <= segUpper[3]; seg4++) {
                            scanList.Add(new IPAddress(new byte[] { (byte)seg1, (byte)seg2, (byte)seg3, (byte)seg4 }).ToString());
                        }
                    }
                }
            }

            return scanList;
        }

    }
}
