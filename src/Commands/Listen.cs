/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
    static class Listen 
    {
        private static Thread[] listenThreads;

        public static void Start(CancellationToken cancellationToken, string address = "")
        {
            IPAddress[] addresses = new IPAddress[0];

            if (address == "") {
                // If no address is given then listen on all local addresses
                addresses = GetLocalAddresses();
                if (addresses == null) {
                    return;
                }
            } else {
                // Otherwise just listen on the address we are given
                addresses = new IPAddress[] { IPAddress.Parse(address) };
            }

            // Start a listening thread for each ipv4 local address
            int size = addresses.Length;
            listenThreads = new Thread[size];
            for (int x = 0; x < addresses.Length; x++)
            {
                int index = x;
                listenThreads[index] = new Thread(() => {
                    ListenForICMPOnAddress(addresses[index]);
                });
                listenThreads[index].IsBackground = true;
                listenThreads[index].Start();
            }

            // Wait for cancellation signal
            while(true) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }
                Thread.Sleep(50);
            }

            // TODO: Implement ListenResults method
            //Display.ListenResults(results);
        }

        private static IPAddress[] GetLocalAddresses()
        {
            IPHostEntry hostAddress = null;

            // Get all addresses assocatied with this computer
            try {
                hostAddress = Dns.GetHostEntry(Dns.GetHostName());
            } catch (Exception e) {
                Display.Error($"Could not fetch local addresses ({e.GetType().ToString().Split('.').Last()})");
            }

            // Only get IPv4 address
            List<IPAddress> addresses = new List<IPAddress>();
            foreach (IPAddress address in hostAddress.AddressList) {
                if (address.AddressFamily == AddressFamily.InterNetwork) {
                    addresses.Add(address);
                }
            }

            return addresses.ToArray();
        }

        public static void ListenForICMPOnAddress(IPAddress address)
        {
            Socket listeningSocket = null;
            PingResults results = new PingResults();
            int bufferSize = 4096;

            // Create listener socket
            try
            {
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                listeningSocket.Bind(new IPEndPoint(address, 0));
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 }); // Set SIO_RCVALL flag to socket IO control
                listeningSocket.ReceiveBufferSize = bufferSize;
            } catch (Exception e) {
                Display.Error($"Exception occured while trying to create listening socket for {address.ToString()} ({e.GetType().ToString().Split('.').Last()})");
                return;
            }

            Display.ListenIntroMsg(address.ToString());

            // Listening loop
            while (true)
            {
                byte[] buffer = new byte[bufferSize];


                try {
                    // Recieve any incoming ICMPv4 packets
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display captured packet
                    Display.CapturedPacket(address.ToString(), response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.CountPacketType(response.Type);
                    results.IncrementReceivedPackets();
                } catch (OperationCanceledException) {
                } catch (SocketException) {
                    Display.Error("Could not read packet from socket");
                }
            }



            listeningSocket.Close();

        }


    }
}
