/*
MIT License - PowerPing 

Copyright (c) 2018 Matthew Carney

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
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace PowerPing
{
    /// <summary>
    /// Class for miscellaneous methods 
    /// </summary>
    public static class Helper
    {
        private static readonly double stopwatchToTimeSpanTicksScale = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        private static readonly double timeSpanToStopwatchTicksScale = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        /// <summary>
        /// Pause program and wait for user input
        /// </summary>
        /// <param name="exit">switch to use word "exit" instead of "continue"</param>
        public static void Pause(bool exit = false)
        {
            Console.Write("Press any key to " + (exit ? "exit" : "continue") + " . . .");
            Console.WriteLine();

            // Work around if readkey isnt supported
            try { Console.ReadKey(); }
            catch (InvalidOperationException) { Console.Read(); }
            
            if (exit) {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Check if long value is between a range
        /// </summary>
        /// <param name="value"></param>
        /// <param name="left">Lower range</param>
        /// <param name="right">Upper range</param>
        /// <returns></returns>
        public static bool IsBetween(long value, long left, long right)
        {
            return value > left && value < right;
        }

        /// <summary>
        /// Produces cryprographically secure string of specified length
        /// </summary>
        /// <param name="len"></param>
        /// <source>https://stackoverflow.com/a/1668371</source>
        /// <returns></returns>
        public static String RandomString(int len = 11)
        {
            string result;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider()) {
                byte[] rngToken = new byte[len + 1];
                rng.GetBytes(rngToken);
               
                result = Convert.ToBase64String(rngToken);
                
            }

            // Remove '=' from end of string
            return result.Remove(result.Length - 1);
        }

        /// <summary>
        /// Produces cryprographically secure int of specified length
        /// </summary>
        /// <param name="len"></param>
        /// <source>http://www.vcskicks.com/code-snippet/rng-int.php</source>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
            int result;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider()) {
                byte[] rngToken = new byte[4];
                rng.GetBytes(rngToken);

                result = BitConverter.ToInt32(rngToken, 0);
            }

            return new Random(result).Next(min, max);
        }

        /// <summary>
        /// Generates a byte array of a given size, used for adding size
        /// to icmp packet
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte[] GenerateByteArray(int size)
        {
            byte[] array = new byte[size];
            for (int i = 0; i < size; i++) {
                array[i] = 0x00;
            }

            return array;
        }

        /// <summary>
        /// Extension method for determining build time
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="target"></param>
        /// <source>http://stackoverflow.com/a/1600990</source>
        /// <returns></returns>
        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        public static ushort GenerateSessionId()
        {
            uint n = (uint)Process.GetCurrentProcess().Id;
            return (ushort)(n ^ (n >> 16));
        }

        public static long StopwatchToTimeSpanTicks(long stopwatchTicks)
        {
            return (long)(stopwatchTicks * stopwatchToTimeSpanTicksScale);
        }

        public static long TimeSpanToStopwatchTicks(long timeSpanTicks)
        {
            return (long)(timeSpanTicks * timeSpanToStopwatchTicksScale);
        }
    }
}
