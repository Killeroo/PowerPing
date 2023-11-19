/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2023 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

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
        private readonly long _minimumRunIntervalTicks;
        private long _lastRunTimestamp;

        public TimeSpan ElapsedSinceLastRun { get; private set; }

        public RateLimiter(TimeSpan minimumRunInterval)
        {
            _minimumRunIntervalTicks = Helper.TimeSpanToStopwatchTicks(minimumRunInterval.Ticks);
        }

        public bool RequestRun()
        {
            long currentTimestamp = Stopwatch.GetTimestamp();
            long elapsed = _lastRunTimestamp == 0 ? 0 : (currentTimestamp - _lastRunTimestamp); // Will be 0 on first call to this method
            if (elapsed == 0 || elapsed >= _minimumRunIntervalTicks)
            {
                ElapsedSinceLastRun = new TimeSpan(Helper.StopwatchToTimeSpanTicks(elapsed));
                _lastRunTimestamp = currentTimestamp;
                return true;
            }
            return false;
        }
    }
}