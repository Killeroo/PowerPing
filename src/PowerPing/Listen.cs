using System;
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

        public static void Start(CancellationToken cancellationToken)
        {
            IPAddress localAddress = null;
            Socket listeningSocket = null;
            PingResults results = new PingResults();
            int bufferSize = 4096;

            // Find local address
            localAddress = IPAddress.Parse(Lookup.GetLocalAddress());

            try {
                // Create listener socket
                listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                listeningSocket.Bind(new IPEndPoint(localAddress, 0));
                listeningSocket.IOControl(IOControlCode.ReceiveAll, new byte[] { 1, 0, 0, 0 }, new byte[] { 1, 0, 0, 0 }); // Set SIO_RCVALL flag to socket IO control
                listeningSocket.ReceiveBufferSize = bufferSize;

                Display.ListenIntroMsg(localAddress.ToString());

                // Listening loop
                while (!cancellationToken.IsCancellationRequested) {
                    byte[] buffer = new byte[bufferSize];

                    // Recieve any incoming ICMPv4 packets
                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    int bytesRead = Helper.RunWithCancellationToken(() => listeningSocket.ReceiveFrom(buffer, ref remoteEndPoint), cancellationToken);
                    ICMP response = new ICMP(buffer, bytesRead);

                    // Display captured packet
                    Display.CapturedPacket(response, remoteEndPoint.ToString(), DateTime.Now.ToString("h:mm:ss.ff tt"), bytesRead);

                    // Store results
                    results.CountPacketType(response.Type);
                    results.Received++;
                }
            }
            catch (OperationCanceledException) {
            }
            catch (SocketException) {
                Display.Error("Could not read packet from socket");
            }
            catch (Exception e) {
                Display.Error($"General exception occured while trying to create listening socket (Exception: {e.GetType().ToString().Split('.').Last()}");
            }

            // Clean up
            listeningSocket.Close();

            // TODO: Implement ListenResults method
            //Display.ListenResults(results);
        }
    }
}
