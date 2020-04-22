/*
MIT License - PowerPing 

Copyright (c) 2020 Matthew Carney

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
using System.Net.Sockets;
using System.Threading;

namespace PowerPing 
{  
    /// <summary>
    /// ICMP Flood functionality.
    /// 
    /// Sends a high volume of ICMP pings to a given address
    /// </summary>
    static class Flood
    {
        private static RateLimiter displayUpdateLimiter = new RateLimiter(TimeSpan.FromMilliseconds(500));
        private static ulong previousPingsSent = 0;
        private static string m_Address = "";

        public static void Start(string address, CancellationToken cancellationToken)
        {
            PingAttributes attrs = new PingAttributes();
            m_Address = address;

            // Verify address
            attrs.ResolvedAddress = Lookup.QueryDNS(address, AddressFamily.InterNetwork);

            // Setup ping attributes
            attrs.Interval = 0;
            attrs.Timeout = 100;
            attrs.Message = "R U Dead Yet?";
            // TODO: Option for 'heavy' flood with bloated packet sizes
            attrs.Continous = true;
            
            Ping p = new Ping(attrs, cancellationToken, ResultsUpdateCallback);

            // Disable output for faster speeds
            Display.ShowOutput = false;

            // Start flooding
            PingResults results = p.Send();

            // Display results
            Display.ShowOutput = true;
            Display.PingResults(attrs, results);
        }

        // This callback will run after each ping iteration
        private static void ResultsUpdateCallback(PingResults r)
        {
            // Make sure we're not updating the display too frequently
            if (!displayUpdateLimiter.RequestRun()) {
                return;
            }

            // Calculate pings per second
            double pingsPerSecond = 0;
            if (displayUpdateLimiter.ElapsedSinceLastRun != TimeSpan.Zero) {
                pingsPerSecond = (r.Sent - previousPingsSent) / displayUpdateLimiter.ElapsedSinceLastRun.TotalSeconds;
            }
            previousPingsSent = r.Sent;

            // Update results text
            Display.FloodProgress(r.Sent, (ulong)Math.Round(pingsPerSecond), m_Address);
        }

    }
}
