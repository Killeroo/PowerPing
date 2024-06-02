/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2024 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System.Net;
using System.Net.Sockets;

namespace PowerPing
{
    /// <summary>
    /// Listens for all ICMPv4 activity on localhost.
    ///
    /// Does this by setting a raw socket to SV_IO_ALL which
    /// will recieve all packets and filters to just show
    /// ICMP packets. Runs until ctrl-c or exit
    /// </summary>
    /// <source>https://stackoverflow.com/a/9174392</source>
    internal static class Listen
    {
        private static Thread[]? _listenThreads;

        public static void Start(CancellationToken cancellationToken, string address = "")
        {
            if (System.Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Helper.ErrorAndExit("Listen mode is not supported on a non windows platform.");
                return;
            }

            // Look up local addresses to listen on
            IPAddress[]? localAddresses;
            if (address == "")
            {
                // If no address is given then listen on all local addresses
                localAddresses = GetLocalAddresses();

            }
            else
            {
                // Otherwise just listen on the address we are given
                localAddresses = new IPAddress[] { IPAddress.Parse(address) };
            }

            if (localAddresses == null)
            {
                Helper.ErrorAndExit("Could not find local addresses to listen on.");
                return;
            }

            // Start a listening thread for each ipv4 local address
            int size = localAddresses.Length;
            _listenThreads = new Thread[size];
            for (int x = 0; x < localAddresses.Length; x++)
            {
                int index = x;
                _listenThreads[index] = new(() =>
                {
                    ListenForICMPOnAddress(localAddresses[index]);
                });
                _listenThreads[index].IsBackground = true;
                _listenThreads[index].Start();
            }

            // Wait for cancellation signal
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                Thread.Sleep(50);
            }

            // TODO: Implement ListenResults method
            //Display.ListenResults(results);
        }

        private static IPAddress[]? GetLocalAddresses()
        {
            IPHostEntry hostAddress;

            // Get all addresses assocatied with this computer
            try
            {
                hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            }
            catch (Exception e)
            {
                ConsoleDisplay.Error($"Could not fetch local addresses ({e.GetType().ToString().Split('.').Last()})");
                return null;
            }

            // Only get IPv4 address
            List<IPAddress> addresses = new();
            foreach (IPAddress address in hostAddress.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    addresses.Add(address);
                }
            }

            return addresses.ToArray();
        }

        public static void ListenForICMPOnAddress(IPAddress address)
        {
            Socket listeningSocket;
            PingResults results = new();
            int bufferSize = 4096;

            // Create listener socket
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                listeningSocket.Bind(new IPEndPoint(address, 0));
#pragma warning disable CA1416 // Validate platform compatibility - We shouldn't be running listen mode on non windows platform
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 }); // Set SIO_RCVALL flag to socket IO control
#pragma warning restore CA1416 // Validate platform compatibility
                listeningSocket.ReceiveBufferSize = bufferSize;
            }
            catch (Exception e)
            {
                ConsoleDisplay.Error($"Exception occured while trying to create listening socket for {address.ToString()} ({e.GetType().ToString().Split('.').Last()})");
                return;
            }

            ConsoleDisplay.ListenIntroMsg(address.ToString());

            // Listening loop
            while (true)
            {
                byte[] buffer = new byte[bufferSize];

                try
                {
                    // Recieve any incoming ICMPv4 packets
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    IPv4 ipHeader = new IPv4(buffer, bytesRead);
                    ICMP response = new(buffer, bytesRead, ipHeader.HeaderLength);
                    string remoteEndPointIp = remoteEndPoint.ToString() ?? string.Empty;

                    // Display captured packet
                    ConsoleDisplay.CapturedPacket(address.ToString(), response, remoteEndPointIp, DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.CountPacketType(response.Type);
                    results.IncrementReceivedPackets();
                }
                catch (OperationCanceledException)
                {
                }
                catch (SocketException)
                {
                    ConsoleDisplay.Error("Could not read packet from socket");
                }
            }

            // No nice way to cancel from listening and close socket. Just to got to hope/assume runtime does it when the thread is killed.
            //listeningSocket.Close();
        }
    }
}