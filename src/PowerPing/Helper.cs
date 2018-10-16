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
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PowerPing
{
    /// <summary>
    /// Class for miscellaneous methods 
    /// </summary>
    public static class Helper
    {
        // Root Whois server to query addresses against
        private const string ROOT_WHOIS_SERVER = "whois.iana.org";

        /// <summary>
        /// Gets location information about IP Address
        /// IP location info by freegeoip.net
        /// </summary>
        /// <param name="addr">Address to get location info on. Can be in IP or address format.</param>
        /// <param name="detailed">Display detailed or simplified location info</param>
        /// <returns>none detailed information string</returns>
        public static string GetAddressLocation(string addr, bool detailed)
        {
            string loc = null;

            try {

                using (var objClient = new System.Net.WebClient()) {

                    // Download xml data for address
                    var file = objClient.DownloadString("http://freegeoip.net/xml/" + addr);

                    // Load xml file into object
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(file);
                    XmlNodeList elements = (xmlDoc.DocumentElement).ChildNodes;

                    // Print it out
                    if (detailed) {
                        Console.WriteLine("Queried address: --{0}--", addr);
                        foreach (XmlElement element in elements) {
                            Console.WriteLine(element.Name + ": " + (element.InnerText == "" ? "NA" : element.InnerText));
                        }
                        Console.WriteLine(PerformWhoIsLookup("whois.verisign-grs.com", addr));
                    } else {
                        if (elements[2].InnerText != "") {
                            loc = "[" + elements[2].InnerText;
                        }
                        if (elements[3].InnerText != "") {
                            loc = loc + ", " + elements[3].InnerText;
                        }
                        if (elements[5].InnerText != "") {
                            loc = loc + ", " + elements[5].InnerText;
                        }
                        loc += "]";
                    }
                }
            } catch (Exception) {
                loc = "[Location unavaliable]";
                Console.WriteLine("[Location unavaliable]");
            }
            
            Console.WriteLine(loc);

            if (!Display.NoInput) {
                Helper.Pause();
            }

            return loc;
        }

        /// <summary>
        /// Returns the local IP address
        /// </summary>
        /// <bug>There is a currently a bug where the address of a VM interface can be used 
        /// instead of the actual local address</bug>
        /// <returns>IP address string, if no address found then returns a null</returns>
        public static string GetLocalAddress()
        {
            // If not connected to a network return null
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return null;
            }

            // Get all addresses assocatied with this computer
            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());

            // Loop through each associated address
            foreach (var address in hostAddress.AddressList) {
                // If address is IPv4
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return address.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Resolve address string to IP Address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="af"></param>
        /// <returns></returns>
        public static string AddressLookup(string address, AddressFamily af)
        {
            IPAddress ipAddr = null;

            // Check address format
            if (Uri.CheckHostName(address) == UriHostNameType.Unknown) {
                PowerPing.Display.Error("PowerPing could not resolve host [" + address + "] " + Environment.NewLine + "Check address and try again.", true, true);
            }

            // Only resolve address if not already in IP address format
            if (IPAddress.TryParse(address, out ipAddr)) {
                return ipAddr.ToString();
            }

            try {
                // Query DNS for host address
                foreach (IPAddress a in Dns.GetHostEntry(address).AddressList) {
                    // Run through addresses until we find one that matches the family we are forcing
                    if (a.AddressFamily == af) {
                        ipAddr = a;
                        break;
                    }
                }
            } catch (Exception) { } // Silently continue on lookup error

            // If no address resolved then exit
            if (ipAddr == null) {
                PowerPing.Display.Error("PowerPing could not find host [" + address + "] " + Environment.NewLine + "Check address and try again.", true, true);
            }

            return ipAddr.ToString();
        }

        /// <summary>
        /// Performs reverse lookup of address, returning host name from a given
        /// IP address
        /// </summary>
        /// <param name="address"></param>
        /// <source>https://stackoverflow.com/a/716753</source>
        /// <returns></returns>
        public static string ReverseLookup(string address)
        {
            string alias = "";

            try {
                IPAddress hostAddr = IPAddress.Parse(address);
                IPHostEntry hostInfo = Dns.GetHostEntry(hostAddr);
                alias = hostInfo.HostName;
            } catch (Exception) { } // Silently continue on lookup error
            
            return alias;
        }

        /// <summary>
        /// Internal whois function for recursive lookup
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static void WhoIs(string address, bool full = true)
        {
            // Trim the inputted address
            address = address.Split('/')[0];
            string keyword = address.Split('.')[0];
            string tld = address.Split('.').Last();

            // Quick sanity check before we proceed
            if (keyword == "" || tld == "") {
                PowerPing.Display.Error("Incorrectly formatted address, please check format and try again (web addresses only)", true);
            }
            PowerPing.Display.Message("WHOIS [" + address + "]:", ConsoleColor.Yellow);

            // Find appropriate whois for the tld
            PowerPing.Display.Message("QUERYING [" + ROOT_WHOIS_SERVER + "] FOR TLD [" + tld + "]:", ConsoleColor.Yellow, false);
            string whoisRoot = PerformWhoIsLookup(ROOT_WHOIS_SERVER, tld);
            PowerPing.Display.Message(" DONE", ConsoleColor.Yellow);
            if (full) {
                Console.WriteLine(whoisRoot);
            }
            whoisRoot = whoisRoot.Remove(0, whoisRoot.IndexOf("whois:", StringComparison.Ordinal) + 6).TrimStart();
            whoisRoot = whoisRoot.Substring(0, whoisRoot.IndexOf('\r'));
            PowerPing.Display.Message("QUERYING [" + whoisRoot + "] FOR DOMAIN [" + address + "]:", ConsoleColor.Yellow, false);

            // Next query resulting whois for the domain
            string result = PerformWhoIsLookup(whoisRoot, address);
            PowerPing.Display.Message(" DONE", ConsoleColor.Yellow);
            Console.WriteLine(result);
            PowerPing.Display.Message("WHOIS LOOKUP FOR [" + address + "] COMPLETE.", ConsoleColor.Yellow);

            if (!Display.NoInput) {
                Helper.Pause();
            }
        }

        /// <summary>
        /// Queries a whois server for information on a server
        /// (https://en.wikipedia.org/wiki/WHOIS)
        /// </summary>
        /// <param name="whoisServer">Address of whois server to use</param>
        /// <param name="query">Query string to send to server</param>
        /// <source>http://nathanenglert.com/2015/05/25/creating-an-app-to-find-that-domain-youve-always-wanted/</source>
        /// <returns></returns>
        private static string PerformWhoIsLookup(string whoisServer, string query)
        {
            StringBuilder result = new StringBuilder();

            // Connect to whois server
            try {
                using (TcpClient whoisClient = new TcpClient(whoisServer, 43))
                using (NetworkStream netStream = whoisClient.GetStream())
                using (BufferedStream bufferStream = new BufferedStream(netStream)) {

                    // Write request to server
                    StreamWriter sw = new StreamWriter(bufferStream);
                    sw.WriteLine(query);
                    sw.Flush();

                    // Read response from server
                    StreamReader sr = new StreamReader(bufferStream);
                    while (!sr.EndOfStream)
                        result.AppendLine(sr.ReadLine());
                }
            } catch (SocketException) {
                result.AppendLine("SocketException: Connection to host failed");
            }

            return result.ToString();
        }

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

    }
}
