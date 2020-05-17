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
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PowerPing
{
    /// <summary>
    /// Ping Class, used for constructing and sending ICMP packets.
    /// </summary>
    class Ping {
        public Action<PingResults> OnPingResultsUpdateCallback = null;

        private PingAttributes m_PingAttributes = null;
        private PingResults m_PingResults = null;

        private Socket m_Socket = null;
        private ICMP m_Packet = null;
        private int m_PacketSize = 0;
        private IPEndPoint m_RemoteEndpoint = null;
        private readonly CancellationToken m_CancellationToken;
        private bool m_Debug = false;

        private ushort m_CurrentSequenceNumber = 0;
        private int m_CurrentReceiveTimeout = 0;

        private static readonly ushort m_SessionId = Helper.GenerateSessionId();

        public Ping(PingAttributes attributes, CancellationToken cancellationToken, Action<PingResults> resultsUpdateCallback = null) 
        {
            m_PingAttributes = attributes;
            m_PingResults = new PingResults();
            m_CancellationToken = cancellationToken;
            OnPingResultsUpdateCallback = resultsUpdateCallback;

            Setup();
        }
        ~Ping()
        {
            Cleanup();
        }

        public PingResults Send(PingAttributes attributes)
        {
            if (attributes != m_PingAttributes) {
                // Replace attributes if they are different
                m_PingAttributes = attributes;

                // Setup everything again
                Setup();
            }

            return Send();
        }
        public PingResults Send(string address)
        {
            m_PingAttributes.InputtedAddress = address;
            m_PingAttributes.ResolvedAddress = ""; 

            // If we get a new address not only do we have to force a lookup
            // Do this by leaving ResolvedAddress blank, we also need to recreate the
            // remove endpoint that is based on that returned address
            ResolveAddress();
            CreateRemoteEndpoint();

            return Send();
        }
        public PingResults Send()
        {
            // Reset some properties so they are readyfor pinging
            Reset();

            Display.PingIntroMsg(m_PingAttributes);

            // CLEANUP: I think this part is bullshit, check later
            if (Display.UseResolvedAddress) {
                try {
                    m_PingAttributes.InputtedAddress = Helper.RunWithCancellationToken(() => Lookup.QueryHost(m_PingAttributes.ResolvedAddress), m_CancellationToken);
                }
                catch (OperationCanceledException) {
                    return new PingResults();
                }
                if (m_PingAttributes.InputtedAddress == "") {
                    // If reverse lookup fails just display whatever is in the address field
                    m_PingAttributes.InputtedAddress = m_PingAttributes.ResolvedAddress;
                }
            }

            // Peroform ping operation
            SendPacket();

            return m_PingResults;
        }

        private void Setup()
        {
            CreateRawSocket();
            SetupSocketOptions();
            ResolveAddress();
            CreateRemoteEndpoint();
            ConstructPacket();
            UpdatePacketChecksum();
        }
        private void Cleanup()
        {
            // On deconstruction
            m_Socket.Close();
            m_Packet = null;
            m_PingAttributes = null;
            m_PingResults = null;
            OnPingResultsUpdateCallback = null;
        }
        private void Reset()
        {
            m_CurrentSequenceNumber = 0;
            m_CurrentReceiveTimeout = 0;

            // Wipe any previous results 
            m_PingResults = new PingResults();
        }

        private void CreateRawSocket()
        {
            // Determine what address family we are using
            AddressFamily family;
            ProtocolType protocol;
            if (m_PingAttributes.UseICMPv4) {
                family = AddressFamily.InterNetwork;
                protocol = ProtocolType.Icmp;
            }
            else {
                family = AddressFamily.InterNetworkV6;
                protocol = ProtocolType.IcmpV6;
            }

            // Create the raw socket
            try {
                m_Socket = new Socket(family, SocketType.Raw, protocol);
            }
            catch (SocketException) {
                Display.Message("PowerPing uses raw sockets which require Administrative rights to create." + Environment.NewLine +
                                "(You can find more info at https://github.com/Killeroo/PowerPing/issues/110)", ConsoleColor.Cyan);
                Helper.ErrorAndExit("Socket cannot be created, make sure you are running as an Administrator and try again.");
            }
        }
        private void SetupSocketOptions()
        {
            m_Socket.Ttl = (short)m_PingAttributes.Ttl;
            m_Socket.DontFragment = m_PingAttributes.DontFragment;
            m_Socket.ReceiveBufferSize = m_PingAttributes.ReceiveBufferSize;
        }
        private void ResolveAddress()
        {
            // If we have not been given a resolved address, perform dns query for inputted address
            if (m_PingAttributes.ResolvedAddress == "") {
                AddressFamily family = m_PingAttributes.UseICMPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
                m_PingAttributes.ResolvedAddress = Lookup.QueryDNS(m_PingAttributes.InputtedAddress, family);
            }
        }
        private void CreateRemoteEndpoint()
        {
            // Convert our resolved address
            IPAddress addr = IPAddress.Parse(m_PingAttributes.ResolvedAddress);

            // Create the endpoint we are going to recieve from
            m_RemoteEndpoint = new IPEndPoint(addr, 0);
        }
        private void SetSocketReceiveTimeout(int timeout)
        {
            if (timeout != m_CurrentReceiveTimeout) {
                m_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                m_CurrentReceiveTimeout = timeout;
            }
        }

        private void ConstructPacket()
        {
            m_Packet = new ICMP();
            m_Packet.Type = m_PingAttributes.Type;
            m_Packet.Code = m_PingAttributes.Code;

            // Work out what our intial payload will be and add to packet
            byte[] payload;
            if (m_PingAttributes.ArtificalMessageSize != -1) {
                payload = Helper.GenerateByteArray(m_PingAttributes.ArtificalMessageSize);
            } else {
                payload = Encoding.ASCII.GetBytes(m_PingAttributes.Message);
            }
            UpdatePacketMessage(payload);

            Buffer.BlockCopy(BitConverter.GetBytes(m_SessionId), 0, m_Packet.Message, 0, 2); // Add identifier to ICMP message
        }
        private void UpdatePacketSequenceNumber(int sequence)
        {
            m_CurrentSequenceNumber = (ushort)sequence;
            Buffer.BlockCopy(BitConverter.GetBytes(m_CurrentSequenceNumber), 0, m_Packet.Message, 2, 2);
        }
        private void UpdatePacketMessage(byte[] message)
        {
            // Copy into packet
            // (NOTE: the offset is where the sequence number and packet identier are stored)
            Buffer.BlockCopy(message, 0, m_Packet.Message, 4, message.Length);

            // Update message size
            // TOD: WATCH THIS, IF IT FAILS REMOVE CHECK
            if (message.Length + 4 != m_PacketSize) {
                m_Packet.MessageSize = message.Length + 4;
                m_PacketSize = m_Packet.MessageSize + 4;
            }
        }
        private void UpdatePacketChecksum()
        {
            m_Packet.Checksum = 0;
            UInt16 chksm = m_Packet.GetChecksum();
            m_Packet.Checksum = chksm;
        }

        private void SendPacket()
        {
            byte[] receiveBuffer = new byte[m_PingAttributes.ReceiveBufferSize]; // Ipv4Header.length + IcmpHeader.length + attrs.recievebuffersize
            int bytesRead = 0;

            // Sending loop
            for (int index = 1; m_PingAttributes.Continous || index <= m_PingAttributes.Count; index++) {

                if (index != 1) {
                    // Wait for set interval before sending again or cancel if requested
                    if (m_CancellationToken.WaitHandle.WaitOne(m_PingAttributes.Interval)) {
                        break;
                    }

                    // Generate random interval when RandomTiming flag is set
                    if (m_PingAttributes.RandomTiming) {
                        m_PingAttributes.Interval = Helper.RandomInt(5000, 100000);
                    }
                }

                // Update packet before sending
                UpdatePacketSequenceNumber(index);
                if (m_PingAttributes.RandomMessage) {
                    UpdatePacketMessage(Encoding.ASCII.GetBytes(Helper.RandomString()));
                }
                UpdatePacketChecksum();

                try {

                    // Show request packet
                    if (Display.ShowRequests) {
                        Display.RequestPacket(m_Packet, Display.UseInputtedAddress | Display.UseResolvedAddress ? m_PingAttributes.InputtedAddress : m_PingAttributes.ResolvedAddress, index);
                    }

                    // If there were extra responses from a prior request, ignore them
                    while (m_Socket.Available != 0) {
                        bytesRead = m_Socket.Receive(receiveBuffer);
                    }

                    // Send ping request
                    m_Socket.SendTo(m_Packet.GetBytes(), m_PacketSize, SocketFlags.None, m_RemoteEndpoint); // Packet size = message field + 4 header bytes
                    long requestTimestamp = Stopwatch.GetTimestamp();
                    m_PingResults.IncrementSentPackets();

                    // Just for artifically testing higher ping response times
                    if (m_Debug) {
                        Random rnd = new Random();
                        Thread.Sleep(rnd.Next(10, 400));
                        if (rnd.Next(3) == 1) { throw new SocketException(); }
                    }

                    // Try and recieve a packet
                    ICMP response = null;
                    EndPoint responseEP = m_RemoteEndpoint;
                    TimeSpan replyTime = TimeSpan.Zero;
                    ReceivePacket(ref response, ref responseEP, ref replyTime, ref bytesRead, requestTimestamp);

                    if (Display.ShowReplies) {

                        // Determine what form the response address is going to be displayed in
                        string responseAddress = responseEP.ToString();
                        if (Display.UseResolvedAddress) {
                            // Returned address normally have port at the end (eg 8.8.8.8:0) so we need to remove that before trying to query the DNS 
                            string responseIP = responseEP.ToString().Split(':')[0];

                            // Resolve the ip and store as the response address
                            responseAddress = Helper.RunWithCancellationToken(() => Lookup.QueryHost(responseIP), m_CancellationToken);
                        }
                        else if (Display.UseInputtedAddress) {
                            responseAddress = m_PingAttributes.InputtedAddress;
                        }

                        Display.ReplyPacket(response, responseAddress, index, replyTime, bytesRead);

                    }

                    // Store response info
                    m_PingResults.IncrementReceivedPackets();
                    m_PingResults.CountPacketType(response.Type);
                    m_PingResults.SaveResponseTime(replyTime.TotalMilliseconds);

                    if (m_PingAttributes.BeepMode == 2) {
                        try { Console.Beep(); }
                        catch (Exception) { } // Silently continue if Console.Beep errors
                    }
                }
                catch (IOException) {

                    if (Display.ShowOutput) {
                        Display.Error("General transmit error");
                    }
                    m_PingResults.SaveResponseTime(-1);
                    m_PingResults.IncrementLostPackets();

                }
                catch (SocketException) {

                    Display.Timeout(index);
                    if (m_PingAttributes.BeepMode == 1) {
                        try { Console.Beep(); }
                        catch (Exception) { }
                    }
                    m_PingResults.SaveResponseTime(-1);
                    m_PingResults.IncrementLostPackets();

                }
                catch (OperationCanceledException) {

                    m_PingResults.ScanWasCanceled = true;
                    break;

                }
                catch (Exception) {

                    if (Display.ShowOutput) {
                        Display.Error("General error occured");
                    }
                    m_PingResults.SaveResponseTime(-1);
                    m_PingResults.IncrementLostPackets();

                }

                // Run callback (if provided) to notify of updated results
                OnPingResultsUpdateCallback?.Invoke(m_PingResults);
            }

        }
        private void ReceivePacket(ref ICMP response, ref EndPoint responseEndPoint, ref TimeSpan replyTime, ref int bytesRead, long requestTimestamp)
        {
            byte[] receiveBuffer = new byte[m_PingAttributes.ReceiveBufferSize];

            // Wait for request
            do {
                // Cancel if requested
                m_CancellationToken.ThrowIfCancellationRequested();

                // Set receive timeout, limited to 250ms so we don't block very long without checking for
                // cancellation. If the requested ping timeout is longer, we will wait some more in subsequent
                // loop iterations until the requested ping timeout is reached.
                int remainingTimeout = (int)Math.Ceiling(m_PingAttributes.Timeout - replyTime.TotalMilliseconds);
                if (remainingTimeout <= 0) {
                    throw new SocketException();
                }
                SetSocketReceiveTimeout(Math.Min(remainingTimeout, 250));

                // Wait for response
                try {
                    bytesRead = m_Socket.ReceiveFrom(receiveBuffer, ref responseEndPoint);
                }
                catch (SocketException) {
                    bytesRead = 0;
                }
                replyTime = new TimeSpan(Helper.StopwatchToTimeSpanTicks(Stopwatch.GetTimestamp() - requestTimestamp));

                if (bytesRead == 0) {
                    response = null;
                }
                else {
                    // Store reply packet
                    IPv4 header = new IPv4(receiveBuffer, bytesRead);
                    Console.WriteLine(header.PrettyPrint());
                    response = new ICMP(receiveBuffer, bytesRead);

                    // If we sent an echo and receive a response with a different identifier or sequence
                    // number, ignore it (it could correspond to an older request that timed out)
                    if (m_Packet.Type == 8 && response.Type == 0) {
                        ushort responseSessionId = BitConverter.ToUInt16(response.Message, 0);
                        ushort responseSequenceNum = BitConverter.ToUInt16(response.Message, 2);
                        if (responseSessionId != m_SessionId || responseSequenceNum != m_CurrentSequenceNumber) {
                            response = null;
                        }
                    }
                }
            } while (response == null);
        }
    }
}
