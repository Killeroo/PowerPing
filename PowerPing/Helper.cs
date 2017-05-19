using System;
using System.Xml;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

/* Macros class */
// Class for miscellaneous methods 

namespace PowerPing
{
    class Helper
    {
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

            try
            {
                using (var objClient = new System.Net.WebClient())
                {
                    var strFile = objClient.DownloadString("http://freegeoip.net/xml/" + addr);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(strFile);
                    XmlNodeList elements = (xmlDoc.DocumentElement).ChildNodes;

                    if (detailed)
                    {
                        Console.WriteLine("Queried address: --{0}--", addr);
                        foreach (XmlElement element in elements)
                            Console.WriteLine(element.Name + ": " + (element.InnerText == "" ? "NA" : element.InnerText));
                        Console.WriteLine("(IP location info by freegeoip.net)");
                    }
                    else
                    {
                        if (elements[2].InnerText != "")
                            loc = "[" + elements[2].InnerText;
                        if (elements[3].InnerText != "")
                            loc = loc + ", " + elements[3].InnerText;
                        if (elements[5].InnerText != "")
                            loc = loc + ", " + elements[5].InnerText;
                        loc += "]";
                    }
                }
            }
            catch (WebException)
            {
                loc = "[Location unavaliable]";
                Console.WriteLine("[Location unavaliable]");
            }

            Console.WriteLine(loc);

            Helper.Pause();

            return loc;
        }

        /// <summary>
        /// Gets location information of current host
        /// </summary>
        public static void whoami()
        {
            GetAddressLocation("", true);
            // TODO: Add some pc information too
        }

        /// <summary>
        /// Pause program and wait for user input
        /// </summary>
        /// <param name="exit">switch to use word "exit" instead of "continue"</param>
        public static void Pause(bool exit = false)
        {
            Console.Write("Press any key to " + (exit ? "exit" : "continue") + " . . .");
            Console.ReadLine();
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
        /// Returns the local hosts IPv4 address
        /// </summary>
        /// <returns>IP address string, if no address found then returns a null</returns>
        public static string GetLocalIPAddress()
        {
            // If not connected to a network return null
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            // Get all addresses assocatied with this computer
            var hostAddress = Dns.GetHostEntry(Dns.GetHostName());

            // Loop through each associated address
            foreach (var address in hostAddress.AddressList)
                // If address is IPv4
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    // Return the address
                    return address.ToString();

            return null;
        }

        /// <summary>
        /// Resolve address string to IP Address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="af"></param>
        /// <returns></returns>
        public static IPAddress VerifyAddress(string address, AddressFamily af)
        {
            IPAddress ipAddr = null;

            // Parse the address to IPAddress
            IPAddress.TryParse(address, out ipAddr);

            try
            {
                // Query DNS for host address
                foreach (IPAddress a in Dns.GetHostEntry(address).AddressList)
                {
                    // Run through addresses until we find one that matches the family we are forcing
                    if (a.AddressFamily == af)
                    {
                        ipAddr = a;
                        break;
                    }
                }
            }
            catch (Exception) { }

            // If no address resolved then exit
            if (ipAddr == null)
                PowerPing.Display.Error("PowerPing could not find the host address [" + address + "]\nCheck address and try again.", true, true);

            return ipAddr;
        }
    }
}
