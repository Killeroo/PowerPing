/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

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
    /// Ping Class, used for constructing and sending ICMP data over a network.
    /// </summary>
    class Ping 
    {
        public Action<PingRequest> ?OnRequest = null;
        public Action<PingReply> ?OnReply = null;
        public Action<PingResults> ?OnResultsUpdate = null;
        public Action<PingTimeout> ?OnTimeout = null;
        public Action<PingAttributes> ?OnStart = null;
        public Action<PingResults>? OnFinish = null;
        public Action<(string message, Exception e, bool fatal)> ?OnError = null;

        private PingAttributes m_Attributes = null;
        private PingResults m_Results = null;

        private Socket m_Socket = null;
        private ICMP m_Packet = null;
        private int m_PacketSize = 0;
        private IPEndPoint m_RemoteEndpoint = null;
        private readonly CancellationToken m_CancellationToken;
        private bool m_Debug = false;

        private PingRequest m_RequestMessage = new();
        private PingReply m_ResponseMessage = new();
        private PingTimeout m_TimeoutMessage = new();

        private ushort m_CurrentSequenceNumber = 0;
        private int m_CurrentReceiveTimeout = 0;

        private static readonly ushort m_SessionId = Helper.GenerateSessionId();

        public Ping(
            PingAttributes attributes, 
            CancellationToken cancellationToken) 
        {
            m_Attributes = attributes;
            m_Results = new PingResults();
            m_CancellationToken = cancellationToken;

            Setup();
        }
        ~Ping()
        {
            Cleanup();
        }

        public PingResults Send(PingAttributes attributes)
        {
            if (attributes != m_Attributes) {
                // Replace attributes if they are different
                m_Attributes = attributes;

                // Setup everything again
                Setup();
            }

            return Send();
        }
        public PingResults Send(string address)
        {
            m_Attributes.InputtedAddress = address;
            m_Attributes.ResolvedAddress = ""; 

            // If we get a new address not only do we have to force a lookup
            // Do this by leaving ResolvedAddress blank, we also need to recreate the
            // remove endpoint that is based on that returned address
            ResolveAddress();
            CreateRemoteEndpoint();

            return Send();
        }
        public PingResults Send()
        {
            // Reset some properties so they are ready for pinging
            Reset();

            // Perform ping operation
            SendPacket();

            return m_Results;
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
            m_Attributes = null;
            m_Results = null;
            OnResultsUpdate = null;
        }
        private void Reset()
        {
            m_CurrentSequenceNumber = 0;
            m_CurrentReceiveTimeout = 0;

            // Wipe any previous results 
            m_Results = new PingResults();
        }

        private void CreateRawSocket()
        {
            // Determine what address family we are using
            AddressFamily family;
            ProtocolType protocol;
            if (m_Attributes.UseICMPv4) {
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
            catch (SocketException e) {
                OnError?.Invoke((
                    "PowerPing uses raw sockets which require Administrative rights to create." + Environment.NewLine +
                    "(You can find more info at https://github.com/Killeroo/PowerPing/issues/110)" + Environment.NewLine +
                    "Make sure you are running as an Administrator and try again.",
                    e,
                    true));
            }
        }
        private void SetupSocketOptions()
        {
            m_Socket.Ttl = (short)m_Attributes.Ttl;
            m_Socket.DontFragment = m_Attributes.DontFragment;
            m_Socket.ReceiveBufferSize = m_Attributes.ReceiveBufferSize;
        }
        private void ResolveAddress()
        {
            // If we have not been given a resolved address, perform dns query for inputted address
            if (m_Attributes.ResolvedAddress == "") {
                AddressFamily family = m_Attributes.UseICMPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
                m_Attributes.ResolvedAddress = Lookup.QueryDNS(m_Attributes.InputtedAddress, family);
            }
        }
        private void CreateRemoteEndpoint()
        {
            // Convert our resolved address
            IPAddress addr = IPAddress.Parse(m_Attributes.ResolvedAddress);

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
            m_Packet.Type = m_Attributes.Type;
            m_Packet.Code = m_Attributes.Code;

            // Work out what our intial payload will be and add to packet
            byte[] payload;
            if (m_Attributes.ArtificalMessageSize != -1) {
                payload = Helper.GenerateByteArray(m_Attributes.ArtificalMessageSize);
            } else {
                payload = Encoding.ASCII.GetBytes(m_Attributes.Message);
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
            byte[] receiveBuffer = new byte[m_Attributes.ReceiveBufferSize]; // Ipv4Header.length + IcmpHeader.length + attrs.recievebuffersize
            int bytesRead = 0;

            OnStart?.Invoke(m_Attributes);

            // Sending loop
            for (int index = 1; m_Attributes.Continous || index <= m_Attributes.Count; index++) {

                if (index != 1) {
                    // Wait for set interval before sending again or cancel if requested
                    if (m_CancellationToken.WaitHandle.WaitOne(m_Attributes.Interval)) {
                        break;
                    }

                    // Generate random interval when RandomTiming flag is set
                    if (m_Attributes.RandomTiming) {
                        m_Attributes.Interval = Helper.RandomInt(5000, 100000);
                    }
                }

                // Update packet before sending
                UpdatePacketSequenceNumber(index);
                if (m_Attributes.RandomMessage) {
                    UpdatePacketMessage(Encoding.ASCII.GetBytes(Helper.RandomString()));
                }
                UpdatePacketChecksum();

                try {
                    // If there were extra responses from a prior request, ignore them
                    while (m_Socket.Available != 0) {
                        bytesRead = m_Socket.Receive(receiveBuffer);
                    }

                    // Send ping request
                    m_Socket.SendTo(m_Packet.GetBytes(), m_PacketSize, SocketFlags.None, m_RemoteEndpoint); // Packet size = message field + 4 header bytes
                    long requestTimestamp = Stopwatch.GetTimestamp();
                    m_Results.IncrementSentPackets();

                    // Raise message on request sent
                    m_RequestMessage.Timestamp = DateTime.Now;
                    m_RequestMessage.SequenceNumber = index;
                    m_RequestMessage.Packet = m_Packet;
                    m_RequestMessage.Destination = m_RemoteEndpoint;
                    OnRequest?.Invoke(m_RequestMessage);

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

                    // Store response info
                    m_Results.IncrementReceivedPackets();
                    m_Results.CountPacketType(response.Type);
                    m_Results.SaveResponseTime(replyTime.TotalMilliseconds);
                }
                catch (IOException e) {    
                    OnError?.Invoke(("General transmit error", e, false));

                    m_Results.SaveResponseTime(-1);
                    m_Results.IncrementLostPackets();
                }
                catch (SocketException) {
                    m_TimeoutMessage.Timestamp = DateTime.Now;
                    m_TimeoutMessage.SequenceNumber = index;
                    m_TimeoutMessage.Endpoint = m_RemoteEndpoint;
                    OnTimeout?.Invoke(m_TimeoutMessage);

                    m_Results.SaveResponseTime(-1);
                    m_Results.IncrementLostPackets();
                }
                catch (OperationCanceledException) {

                    m_Results.ScanWasCanceled = true;
                    break;
                }
                catch (Exception e) {
                    OnError?.Invoke(("General error occured", e, false));

                    m_Results.SaveResponseTime(-1);
                    m_Results.IncrementLostPackets();
                }

                // Run callback (if provided) to notify of updated results
                OnResultsUpdate?.Invoke(m_Results);
            }

            OnFinish?.Invoke(m_Results);
        }
        private void ReceivePacket(ref ICMP response, ref EndPoint responseEndPoint, ref TimeSpan replyTime, ref int bytesRead, long requestTimestamp)
        {
            byte[] receiveBuffer = new byte[m_Attributes.ReceiveBufferSize];

            // Wait for request
            do {
                // Cancel if requested
                m_CancellationToken.ThrowIfCancellationRequested();

                // Set receive timeout, limited to 250ms so we don't block very long without checking for
                // cancellation. If the requested ping timeout is longer, we will wait some more in subsequent
                // loop iterations until the requested ping timeout is reached.
                int remainingTimeout = (int)Math.Ceiling(m_Attributes.Timeout - replyTime.TotalMilliseconds);
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

            // Raise message on response
            m_ResponseMessage.Packet = response;
            m_ResponseMessage.Endpoint = responseEndPoint as IPEndPoint;
            m_ResponseMessage.Timestamp = DateTime.Now;
            m_ResponseMessage.SequenceNumber = m_CurrentSequenceNumber;
            m_ResponseMessage.BytesRead = bytesRead;
            m_ResponseMessage.RoundTripTime = replyTime;
            OnReply?.Invoke(m_ResponseMessage);
        }
    }
}
