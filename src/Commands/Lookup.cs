/*
MIT License - PowerPing 

Copyright (c) 2020 Matthew Carney

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
using System.Linq;
using System.Text;

namespace PowerPing
{
    /// <summary>
    /// Static function class for methods related to geoip, dnslookup and whois methods
    /// </summary>
    public static class Lookup
    {
        // Root Whois server to query addresses against
        private const string ROOT_WHOIS_SERVER = "whois.iana.org";

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
                return string.Empty;
            }

            // Get all addresses assocatied with this computer
            IPHostEntry hostAddress = Dns.GetHostEntry(Dns.GetHostName());

            // Loop through each associated address
            foreach (IPAddress address in hostAddress.AddressList) {
                // If address is IPv4
                if (address.AddressFamily == AddressFamily.InterNetwork) {
                    return address.ToString();
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Prints location information about IP Address
        /// </summary>
        /// <param name="addr">Address to get location info on. Can be in IP or address format.</param>
        /// <param name="detailed">Display detailed or simplified location info</param>
        public static string GetAddressLocationInfo(string addr, bool detailed)
        {
            string result = "";

            try {

                using (WebClient objClient = new WebClient()) {

                    string key = "";

                    // Download xml data for address
                    string downloadedText = objClient.DownloadString(
                        $"http://api.ipstack.com/{addr}?access_key={key}&output=xml");
                    
                    // Load xml file into object
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(downloadedText);
                    XmlNodeList elements = (xmlDoc.DocumentElement).ChildNodes;

                    if (elements == null) {
                        throw new Exception();
                    }

                    // Print it out
                    if (detailed) {
                        foreach (XmlElement element in elements) {
                            if (element.Name != "location") {
                                result += element.Name + ": " + (element.InnerText == "" ? "N/A" : element.InnerText) + Environment.NewLine;
                            }
                        }
                    } else {
                        result += "[";
                        if (elements[7].InnerText != "") {
                            result += elements[7].InnerText + ", ";
                        }
                        if (elements[4].InnerText != "") {
                            result += elements[4].InnerText;
                        }
                        result += "]";
                    }
                }
            } catch (Exception) {
                result = "[Location unavaliable]";
            }

            return result;
        }

        /// <summary>
        /// Resolve address string to IP Address by querying DNS server
        /// </summary>
        /// <param name="address"></param>
        /// <param name="af"></param>
        /// <returns></returns>
        public static string QueryDNS(string address, AddressFamily af)
        {
            IPAddress ipAddr = null;

            // Check address format
            if (Uri.CheckHostName(address) == UriHostNameType.Unknown) {
                Helper.ErrorAndExit("PowerPing could not resolve host [" + address + "] " + Environment.NewLine + "Check address and try again.");
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
                Helper.ErrorAndExit("PowerPing could not find host [" + address + "] " + Environment.NewLine + "Check address and try again.");
            }

            return ipAddr.ToString();
        }

        /// <summary>
        /// Performs reverse lookup of host, returning hostname from a given
        /// IP address
        /// </summary>
        /// <param name="address"></param>
        /// <source>https://stackoverflow.com/a/716753</source>
        /// <returns></returns>
        public static string QueryHost(string address)
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
        public static void QueryWhoIs(string address, bool full = true)
        {
            // Trim the inputted address
            address = address.Split('/')[0];
            string keyword = address.Split('.')[0];
            string tld = address.Split('.').Last();

            // Quick sanity check before we proceed
            if (keyword == "" || tld == "") {
                Helper.ErrorAndExit("Incorrectly formatted address, please check format and try again");
            }
            Display.Message("WHOIS [" + address + "]:", ConsoleColor.Yellow);

            // Find appropriate whois for the tld
            Display.Message("QUERYING [" + ROOT_WHOIS_SERVER + "] FOR TLD [" + tld + "]:", ConsoleColor.Yellow, false);
            string whoisRoot = PerformWhoIsLookup(ROOT_WHOIS_SERVER, tld);
            Display.Message(" DONE", ConsoleColor.Yellow);
            if (full) {
                Console.WriteLine(whoisRoot);
            }
            whoisRoot = whoisRoot.Remove(0, whoisRoot.IndexOf("whois:", StringComparison.Ordinal) + 6).TrimStart();
            whoisRoot = whoisRoot.Substring(0, whoisRoot.IndexOf('\r'));
            Display.Message("QUERYING [" + whoisRoot + "] FOR DOMAIN [" + address + "]:", ConsoleColor.Yellow, false);

            // Next query resulting whois for the domain
            string result = PerformWhoIsLookup(whoisRoot, address);
            Display.Message(" DONE", ConsoleColor.Yellow);
            Console.WriteLine(result);
            Display.Message("WHOIS LOOKUP FOR [" + address + "] COMPLETE.", ConsoleColor.Yellow);

            Helper.WaitForUserInput();
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
    }
}
