/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing 
 *************************************************************************/

using System;
using System.Diagnostics;

namespace PowerPing
{
    /// <summary>
    /// Ensures that a job can't run more often than a specified interval. For example,
    /// in a tight loop that needs to write status updates to the console periodically,
    /// this can be used to limit the rate at which the status updates happen.
    /// </summary>
    public class RateLimiter
    {
        private readonly long m_MinimumRunIntervalTicks;
        private long m_LastRunTimestamp;

        public TimeSpan ElapsedSinceLastRun { get; private set; }

        public RateLimiter(TimeSpan minimumRunInterval)
        {
            m_MinimumRunIntervalTicks = Helper.TimeSpanToStopwatchTicks(minimumRunInterval.Ticks);
        }

        public bool RequestRun()
        {
            long currentTimestamp = Stopwatch.GetTimestamp();
            long elapsed = m_LastRunTimestamp == 0 ? 0 : (currentTimestamp - m_LastRunTimestamp); // Will be 0 on first call to this method
            if (elapsed == 0 || elapsed >= m_MinimumRunIntervalTicks) {
                ElapsedSinceLastRun = new TimeSpan(Helper.StopwatchToTimeSpanTicks(elapsed));
                m_LastRunTimestamp = currentTimestamp;
                return true;
            }
            return false;
        }
    }
}
