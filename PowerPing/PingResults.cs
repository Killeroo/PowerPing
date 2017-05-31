using System;
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
        public int Sent { get; set; } // Number of sent ping packets
        public int Recieved { get; set; } // Number of recieved reply packets
        public int Lost { get; set; }  // Amount of lost packets
        public long MaxTime { get; private set; } // Highest ping reply time
        public long MinTime { get; private set; } // Lowest ping reply time
        public long CurTime { get; private set; }// Most recent packet response time
        // Status (IPStatus enum)

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
    }

}