/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 * ************************************************************************/

using System.Diagnostics;

namespace PowerPing
{
    /// <summary>
    /// Stores the results of a ping operation
    /// </summary>
    public class PingResults
    {
        // Properties
        public DateTime StartTime { get; private set; }                             // Time operation started at
        public DateTime EndTime { get; private set; }                               // Time operation finished    
        public TimeSpan TotalRunTime { get { return _operationTimer.Elapsed; } }    // Total ping operation runtime
        public ulong Sent { get; private set; }                                     // Number of sent ping packets
        public ulong Received { get; private set; }                                 // Number of received packets
        public ulong Lost { get; private set; }                                     // Amount of lost packets
        public double MaxTime { get; private set; }                                 // Highest ping reply time
        public double MinTime { get; private set; }                                 // Lowest ping reply time
        public double AvgTime { get; private set; }                                 // Average reply time
        public double CurrTime { get; private set; }                                // Most recent packet response time
        public ulong ErrorPackets { get; private set; }                             // Number of Error packet received
        public ulong GoodPackets { get; private set; }                              // Number of good replies received
        public ulong OtherPackets { get; private set; }                             // Number of other packet types received
        public bool HasOverflowed { get; set; }                                     // Specifies if any of the results have overflowed
        public bool ScanWasCanceled { get; set; }                                   // Whether the scan was canceled early

        private readonly Stopwatch _operationTimer = new Stopwatch();              // Used to time total time spent doing operation
        private double _responseTimeSum = 0;                                       // Sum of all reply times (used to work out general average

        public PingResults()
        {
            // Default properties
            Sent = 0;
            Received = 0;
            Lost = 0;
            MaxTime = 0;
            MinTime = 0;
            AvgTime = 0;
            CurrTime = -1;
            ErrorPackets = 0;
            GoodPackets = 0;
            OtherPackets = 0;

            HasOverflowed = false;
        }

        public void Start()
        {
            // Get local start time
            StartTime = DateTime.Now;

            // Start timing operation
            _operationTimer.Start();
        }

        public void Stop()
        {
            EndTime = DateTime.Now;

            _operationTimer.Stop();
        }

        public void SaveResponseTime(double time)
        {
            if (time == -1f)
            {
                CurrTime = 0;
                return;
            }

            // BUG: Converting from long to double might be causing precisson loss
            // Check response time against current max and min
            if (time > MaxTime)
            {
                MaxTime = time;
            }

            if (time < MinTime || MinTime == 0)
            {
                MinTime = time;
            }

            try
            {
                // Work out average
                _responseTimeSum += time;
                AvgTime = _responseTimeSum / Received; // Avg = Total / Count
            }
            catch (OverflowException)
            {
                HasOverflowed = true;
            }
            CurrTime = time;
        }

        public void CountPacketType(int type)
        {
            try
            {
                if (type == 0)
                {
                    GoodPackets++;
                }
                else if (type == 3 || type == 4 || type == 5 || type == 11)
                {
                    ErrorPackets++;
                }
                else
                {
                    OtherPackets++;
                }
            }
            catch (OverflowException)
            {
                HasOverflowed = true;
            }
        }

        public void IncrementSentPackets()
        {
            try
            {
                Sent++;
            }
            catch (OverflowException)
            {
                HasOverflowed = true;
            }
        }

        public void IncrementReceivedPackets()
        {
            try
            {
                Received++;
            }
            catch (OverflowException)
            {
                HasOverflowed = true;
            }
        }

        public void IncrementLostPackets()
        {
            try
            {
                Lost++;
            }
            catch (OverflowException)
            {
                HasOverflowed = true;
            }
        }
    }
}