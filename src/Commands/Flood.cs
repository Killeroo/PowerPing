/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

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
