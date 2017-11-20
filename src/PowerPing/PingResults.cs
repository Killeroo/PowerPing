using System;
using System.Collections.Concurrent;
using System.Diagnostics;

/// <summary>
/// Stores the results of a ping operation
/// </summary>

namespace PowerPing
{

    public class PingResults
    {
        // Properties
        public DateTime StartTime { get; private set; } // Time operation started at 
        public TimeSpan TotalRunTime { get { return operationTimer.Elapsed; } } // Total ping operation runtime
        public long Sent { get; set; } // Number of sent ping packets
        public long Received { get; set; } // Number of received packets
        public long Lost { get; set; }  // Amount of lost packets
        public double MaxTime { get; private set; } // Highest ping reply time
        public double MinTime { get; private set; } // Lowest ping reply time
        public double AvgTime { get; private set; } // Average reply time
        public double CurTime { get; private set; } // Most recent packet response time
        public long ErrorPackets { get; private set; } // Number of Error packet received
        public long GoodPackets { get; private set; } // Number of good replies received
        public long OtherPackets { get; private set; } // Number of other packet types received
        //public static ConcurrentStack<string> ActiveHosts = new ConcurrentStack<string>(); // Stores found hosts during scan

        // Local variables
        private Stopwatch operationTimer = new Stopwatch();
        private long sum  = 0; // Sum off all reply times

        public PingResults()
        {
            // Default properties
            Sent = 0;
            Received = 0;
            Lost = 0;
            MaxTime = 0;
            MinTime = 0;
            AvgTime = 0;
            CurTime = -1;
            ErrorPackets = 0;
            GoodPackets = 0;
            OtherPackets = 0;

            // Get local start time
            StartTime = DateTime.Now;

            // Start timing operation
            operationTimer.Start();
        }

        public void SetCurResponseTime(double time)
        {
            if (time == -1)
            {
                CurTime = 0;
                return;
            }

            // Check response time against current max and min
            if (time > MaxTime)
                MaxTime = time;

            if (time < MinTime || MinTime == 0)
                MinTime = time;

            // Work out average
            sum += (long) time;
            AvgTime = (double) sum / Received; // Avg = Total / Count

            CurTime = time;
        }
        public void SetPacketType(int type)
        {
            if (type == 0)
                GoodPackets++;
            else if (type == 3 || type == 4 || type == 5 || type == 11)
                ErrorPackets++;
            else
                OtherPackets++;
        }
    }

}