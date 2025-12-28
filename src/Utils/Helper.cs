/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2026 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PowerPing
{
    /// <summary>
    /// Class for miscellaneous methods
    /// </summary>
    public static class Helper
    {
        public static bool RequireInput = false;

        private static readonly string _ipv4Regex = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}";
        private static readonly string _urlRegex = @"[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?";
        private static readonly string _validScanRangeRegex = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|\-|$)){5}";

        private static readonly double _stopwatchToTimeSpanTicksScale = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;
        private static readonly double _timeSpanToStopwatchTicksScale = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

        /// <summary>
        /// Pause program and wait for user input
        /// </summary>
        public static void WaitForUserInput()
        {
            // Don't wait for output when the output of the program is being redirected
            // (to say a file or something)
            if (Console.IsOutputRedirected)
                return;

            Console.Write("Press any key to continue...");
            Console.WriteLine();

            // Work around if readkey isnt supported
            try { Console.ReadKey(); }
            catch (InvalidOperationException) { Console.Read(); }
        }

        /// <summary>
        /// Prints and error message and then exits with exit code 1
        /// </summary>
        /// <param name="msg">Error message to print</param>
        /// <param name="pause">Wait for user input before exitingt</param>
        public static void ErrorAndExit(string msg)
        {
            ConsoleDisplay.Error(msg);

            if (RequireInput)
            {
                Helper.WaitForUserInput();
            }

            Environment.Exit(1);
        }

        /// <summary>
        /// Exits the application with an error code
        /// </summary>
        public static void ExitWithError()
        {
            Environment.Exit(1);
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
        /// <returns></returns>
        public static String RandomString(int len = 11)
        {
            RandomNumberGenerator.Create();
            byte[] randomBytes = RandomNumberGenerator.GetBytes(len);
            string result = Convert.ToBase64String(randomBytes);

            // Remove '=' from end of string
            return result.Remove(result.Length - 1);
        }

        /// <summary>
        /// Produces cryprographically secure int of specified length
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
            return RandomNumberGenerator.GetInt32(min, max);
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
            for (int i = 0; i < size; i++)
            {
                array[i] = 0x00;
            }

            return array;
        }

        /// <summary>
        /// Checks if a string is a valid IPv4 address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static bool IsIPv4Address(string address)
        {
            return Regex.Match(address, _ipv4Regex).Success;
        }

        /// <summary>
        /// Checks if a string is a valid url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsURL(string url)
        {
            return Regex.Match(url, _urlRegex).Success;
        }

        /// <summary>
        /// Checks if a string is a valid range.
        /// Range looks like with normal IP address with dash to specify range to scan:
        /// EG 192.168.1.1-255 to scan every address between 192.168.1.1-192.168.1.255
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        /// TODO: Allow for scan range to specified in any segment
        public static bool IsValidScanRange(string range)
        {
            return Regex.Match(range, _validScanRangeRegex).Success;
        }

        /// <summary>
        /// Runs a function inside a Task instead of on the current thread. This allows for use of a cancellation
        /// token to resume the current thread (by throwing OperationCanceledException) before the function finishes.
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static T RunWithCancellationToken<T>(Func<T> func, CancellationToken cancellationToken)
        {
            return Task.Run<T>(func, cancellationToken).WaitForResult(cancellationToken);
        }

        /// <summary>
        /// Generates session id to store in packet using underlying process id
        /// </summary>
        /// <returns></returns>
        public static ushort GenerateSessionId()
        {
            uint n = (uint)Process.GetCurrentProcess().Id;
            return (ushort)(n ^ (n >> 16));
        }

        /// <summary>
        /// Split list into x equally sized lists
        /// </summary>
        /// <source>https://stackoverflow.com/a/3893011</source>
        public static List<T>[] PartitionList<T>(List<T> list, int partitionCount)
        {
            if (list == null)
                throw new ArgumentNullException();

            if (partitionCount < 1)
                throw new ArgumentOutOfRangeException();

            List<T>[] partitions = new List<T>[partitionCount];

            int maxSize = (int)Math.Ceiling((double)(list.Count / (double)partitionCount));
            int currentOffset = 0;

            for (int i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (int j = currentOffset; j < currentOffset + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;

                    partitions[i].Add(list[j]);
                }

                currentOffset += maxSize;
            }

            return partitions;
        }

        public static long StopwatchToTimeSpanTicks(long stopwatchTicks)
        {
            return (long)(stopwatchTicks * _stopwatchToTimeSpanTicksScale);
        }

        public static long TimeSpanToStopwatchTicks(long timeSpanTicks)
        {
            return (long)(timeSpanTicks * _timeSpanToStopwatchTicksScale);
        }

        /// <summary>
        /// Gets the version this assembly
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            string version = "";
            Version? v = Assembly.GetExecutingAssembly().GetName().Version;

            if (v != null)
            {
                version = "v" + v.Major + "." + v.Minor + "." + v.Build + " (r" + v.Revision + ") " + (Program.BETA ? "[BETA]" : "");
            }
            else
            {
                version = "";
            }

            return version;
        }

        /// <summary>
        /// Checks if the file path already exists, if so then a (1) is added the end of the filename
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string CheckForDuplicateFile(string filepath)
        {
            string filename = Path.GetFileNameWithoutExtension(filepath);
            string extension = Path.GetExtension(filepath);

            string? outDir = Path.GetDirectoryName(filepath);
            string? path = string.IsNullOrEmpty(outDir) ? Path.GetPathRoot(filepath) : outDir;

            string currentFilePath = filepath;
            bool goodFilename = false;
            int counter = 0;

            // Loop through possible names till we find one that doesn't already exist
            while (goodFilename == false)
            {
                if (File.Exists(currentFilePath))
                {
                    counter++;
                    currentFilePath = Path.Combine(path ?? string.Empty, $"{filename}({counter}){extension}");
                }
                else
                {
                    goodFilename = true;
                }
            }

            return currentFilePath;
        }
    }
}