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
