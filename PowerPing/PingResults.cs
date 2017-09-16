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
        public TimeSpan TotalRunTime { get { return operationTimer.Elapsed; } } // Total ping operation runtime
        public long Sent { get; set; } // Number of sent ping packets
        public long Recieved { get; set; } // Number of recieved packets
        public long Lost { get; set; }  // Amount of lost packets
        public long MaxTime { get; private set; } // Highest ping reply time
        public long MinTime { get; private set; } // Lowest ping reply time
        public long CurTime { get; private set; } // Most recent packet response time
        public long ErrorPackets { get; private set; } // Number of Error packet recieved
        public long GoodPackets { get; private set; } // Number of good replies recieved
        public long OtherPackets { get; private set; } // Number of other packet types recieved
        public static ConcurrentStack<string> ActiveHosts = new ConcurrentStack<string>(); // Stores found hosts during scan

        // Local variables
        private Stopwatch operationTimer = new Stopwatch();

        public PingResults()
        {
            // Default properties
            Sent = 0;
            Recieved = 0;
            Lost = 0;
            MaxTime = 0;
            MinTime = 0;
            CurTime = -1;
            ErrorPackets = 0;
            GoodPackets = 0;
            OtherPackets = 0;

            // Start timing operation
            operationTimer.Start();
        }

        public void SetCurResponseTime(long time)
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

            CurTime = time;
        }
        public void SetPacketType(int type)
        {
            if (type == 0)
                // Successful replies
                GoodPackets++;
            else if (type == 3 || type == 4 || type == 5 || type == 11)
                // Error packet types
                ErrorPackets++;
            else
                // Other packet type
                OtherPackets++;
        }
    }

}