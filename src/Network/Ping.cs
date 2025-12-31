/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2026 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PowerPing
{
    /// <summary>
    /// Ping Class, used for constructing and sending ICMP data over a network.
    /// </summary>
    public class Ping
    {
        public Action<PingRequest>? OnRequest = null;
        public Action<PingReply>? OnReply = null;
        public Action<PingResults>? OnResultsUpdate = null;
        public Action<PingTimeout>? OnTimeout = null;
        public Action<PingAttributes>? OnStart = null;
        public Action<PingResults>? OnFinish = null;
        public Action<PingError>? OnError = null;

        private PingAttributes _attributes = new();
        private PingResults _results = new();

        private Socket? _socket = null;
        private ICMP? _packet = null;
        private int _packetSize = 0;
        private IPEndPoint? _remoteEndpoint = null;
        private readonly CancellationToken _cancellationToken;

        private bool _debugIpHeader = false;
        private bool _debugTimings = false;

        private PingRequest _requestMessage = new();
        private PingReply _responseMessage = new();
        private PingTimeout _timeoutMessage = new();
        private PingError _errorMesssage = new();

        private ushort _currentSequenceNumber = 0;
        private int _currentReceiveTimeout = 0;

        private static readonly ushort _sessionId = Helper.GenerateSessionId();

        public Ping(
            PingAttributes attributes,
            CancellationToken cancellationToken)
        {
            _attributes = attributes;
            _cancellationToken = cancellationToken;

            Setup();
        }

        ~Ping()
        {
            Cleanup();
        }

        public PingResults Send(PingAttributes attributes)
        {
            if (attributes != _attributes)
            {
                // Replace attributes if they are different
                _attributes = attributes;

                // Setup everything again
                Setup();
            }

            return Send();
        }

        public PingResults Send(string address)
        {
            _attributes.InputtedAddress = address;
            _attributes.ResolvedAddress = "";

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

            return _results;
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
            _socket?.Close();
            _packet = null;
            _attributes.ResetToDefaultValues();
            _results.Reset();
            OnResultsUpdate = null;
        }

        private void Reset()
        {
            _currentSequenceNumber = 0;
            _currentReceiveTimeout = 0;

            // Wipe any previous results
            _results = new PingResults();
        }

        private void CreateRawSocket()
        {
            // Determine what address family we are using
            AddressFamily family;
            ProtocolType protocol;
            if (_attributes.UseICMPv4)
            {
                family = AddressFamily.InterNetwork;
                protocol = ProtocolType.Icmp;
            }
            else
            {
                family = AddressFamily.InterNetworkV6;
                protocol = ProtocolType.IcmpV6;
            }

            // Create the raw socket
            try
            {
                _socket = new Socket(family, SocketType.Raw, protocol);
            }
            catch (SocketException e)
            {
                if (OnError != null)
                {
                    _errorMesssage.Message =
                        "PowerPing uses raw sockets which require Administrative rights to create." + Environment.NewLine +
                        "(You can find more info at https://github.com/Killeroo/PowerPing/issues/110)" + Environment.NewLine +
                        "Make sure you are running as an Administrator and try again.";
                    _errorMesssage.Exception = e;
                    _errorMesssage.Timestamp = DateTime.Now;
                    _errorMesssage.Fatal = true;

                    OnError.Invoke(_errorMesssage);

                    // Exit on fatal error
                    Helper.ExitWithError();
                }

            }
        }

        private void SetupSocketOptions()
        {
            if (_socket == null)
            {
                return;
            }

            _socket.Ttl = (short)_attributes.Ttl;
            _socket.DontFragment = _attributes.DontFragment;
            _socket.ReceiveBufferSize = _attributes.ReceiveBufferSize;
        }

        private void ResolveAddress()
        {
            // If we have not been given a resolved address, perform dns query for inputted address
            if (_attributes.ResolvedAddress == "")
            {
                AddressFamily family = _attributes.UseICMPv4 ? AddressFamily.InterNetwork : AddressFamily.InterNetworkV6;
                _attributes.ResolvedAddress = Lookup.QueryDNS(_attributes.InputtedAddress, family);
            }
        }

        private void CreateRemoteEndpoint()
        {
            // Convert our resolved address
            IPAddress addr = IPAddress.Parse(_attributes.ResolvedAddress);

            // Create the endpoint we are going to recieve from
            _remoteEndpoint = new IPEndPoint(addr, 0);
        }

        private void SetSocketReceiveTimeout(int timeout)
        {
            if (_socket == null)
            {
                return;
            }

            if (timeout != _currentReceiveTimeout)
            {
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
                _currentReceiveTimeout = timeout;
            }
        }

        private void ConstructPacket()
        {
            _packet = new ICMP();
            _packet.Type = _attributes.Type;
            _packet.Code = _attributes.Code;

            // Work out what our intial payload will be and add to packet
            byte[] payload;
            if (_attributes.ArtificalMessageSize != -1)
            {
                payload = Helper.GenerateByteArray(_attributes.ArtificalMessageSize);
            }
            else
            {
                payload = Encoding.ASCII.GetBytes(_attributes.Message);
            }
            UpdatePacketMessage(payload);

            // Add identifier to ICMP message
            Buffer.BlockCopy(BitConverter.GetBytes(_sessionId), 0, _packet.Message, 0, 2); 
        }

        private void UpdatePacketSequenceNumber(int sequence)
        {
            if (_packet == null) 
            {
                return;
            }

            _currentSequenceNumber = (ushort)sequence;
            Buffer.BlockCopy(BitConverter.GetBytes(_currentSequenceNumber), 0, _packet.Message, 2, 2);
        }

        private void UpdatePacketMessage(byte[] message)
        {
            const int kIcmpIdAndSequenceSize = 4;

            if (_packet == null)
            {
                return;
            }

            // Increase size of packet message if we need to
            if (message.Length + kIcmpIdAndSequenceSize > _packet.Message.Length)
            {
                byte[] newPacketData = new byte[message.Length + kIcmpIdAndSequenceSize];

                // Copy over id and sequence number
                Buffer.BlockCopy(_packet.Message, 0, newPacketData, 0, kIcmpIdAndSequenceSize);
                _packet.Message = newPacketData;
            }

            // Copy into packet
            Buffer.BlockCopy(message, 0, _packet.Message, kIcmpIdAndSequenceSize, message.Length);

            // Update message size
            if (message.Length + kIcmpIdAndSequenceSize != _packetSize)
            {
                _packet.MessageSize = message.Length + kIcmpIdAndSequenceSize;
                _packetSize = _packet.MessageSize + kIcmpIdAndSequenceSize;
            }
        }

        private void UpdatePacketChecksum()
        {
            if (_packet == null)
            {
                return;
            }

            _packet.Checksum = 0;
            UInt16 chksm = _packet.GetChecksum();
            _packet.Checksum = chksm;
        }

        private void SendPacket()
        {
            if (_remoteEndpoint == null || _socket == null || _packet == null)
            {
                return;
            }

            byte[] receiveBuffer = new byte[20 /* Ipv4 header length */ + 4 /* ICMP header length */ + _packet.MessageSize + _attributes.ReceiveBufferSize];
            int bytesRead = 0;

            OnStart?.Invoke(_attributes);
            _results.Start();

            // Sending loop
            for (int index = 1; _attributes.Continous || index <= _attributes.Count; index++)
            {
                if (index != 1)
                {
                    // Wait for set interval before sending again or cancel if requested
                    if (_cancellationToken.WaitHandle.WaitOne(_attributes.Interval))
                    {
                        break;
                    }

                    // Generate random interval when RandomTiming flag is set
                    if (_attributes.RandomTiming)
                    {
                        _attributes.Interval = Helper.RandomInt(5000, 100000);
                    }
                }

                // Update packet before sending
                UpdatePacketSequenceNumber(index);
                if (_attributes.RandomMessage)
                {
                    UpdatePacketMessage(Encoding.ASCII.GetBytes(Helper.RandomString()));
                }
                UpdatePacketChecksum();

                try
                {
                    // If there were extra responses from a prior request, ignore them
                    while (_socket.Available != 0)
                    {
                        bytesRead = _socket.Receive(receiveBuffer);
                    }

                    // Send ping request
                    _socket.SendTo(_packet.GetBytes(), _packetSize, SocketFlags.None, _remoteEndpoint); // Packet size = message field + 4 header bytes
                    long requestTimestamp = Stopwatch.GetTimestamp();
                    _results.IncrementSentPackets();

                    // Raise message on request sent
                    if (OnRequest != null)
                    {
                        _requestMessage.Timestamp = DateTime.Now;
                        _requestMessage.SequenceNumber = index;
                        _requestMessage.Packet = _packet;
                        _requestMessage.Destination = _remoteEndpoint;
                        _requestMessage.PacketSize = _packetSize;

                        OnRequest.Invoke(_requestMessage);
                    }

                    // Just for artifically testing higher ping response times
                    if (_debugTimings)
                    {
                        Random rnd = new Random();
                        Thread.Sleep(rnd.Next(10, 400));
                        if (rnd.Next(3) == 1) { throw new SocketException(); }
                    }

                    // Try and recieve a packet
                    ICMP response = ICMP.EmptyPacket;
                    EndPoint responseEP = _remoteEndpoint;
                    TimeSpan replyTime = TimeSpan.Zero;
                    ReceivePacket(ref response, ref responseEP, ref replyTime, ref bytesRead, requestTimestamp);

                    // Store response info
                    _results.IncrementReceivedPackets();
                    _results.CountPacketType(response.Type);
                    _results.SaveResponseTime(replyTime.TotalMilliseconds);
                }
                catch (IOException e)
                {
                    if (OnError != null)
                    {
                        _errorMesssage.Message = "General transmit error";
                        _errorMesssage.Exception = e;
                        _errorMesssage.Timestamp = DateTime.Now;
                        _errorMesssage.Fatal = false;

                        OnError.Invoke(_errorMesssage);
                    }

                    _results.SaveResponseTime(-1);
                    _results.IncrementLostPackets();
                }
                catch (SocketException)
                {
                    if (OnTimeout != null)
                    {
                        _timeoutMessage.Timestamp = DateTime.Now;
                        _timeoutMessage.SequenceNumber = index;
                        _timeoutMessage.Endpoint = _remoteEndpoint;

                        OnTimeout.Invoke(_timeoutMessage);
                    }

                    _results.SaveResponseTime(-1);
                    _results.IncrementLostPackets();
                }
                catch (OperationCanceledException)
                {
                    _results.ScanWasCancelled = true;
                    break;
                }
                catch (Exception e)
                {
                    if (OnError != null)
                    {
                        _errorMesssage.Message = "General error occured";
                        _errorMesssage.Exception = e;
                        _errorMesssage.Timestamp = DateTime.Now;
                        _errorMesssage.Fatal = false;

                        OnError.Invoke(_errorMesssage);
                    }

                    _results.SaveResponseTime(-1);
                    _results.IncrementLostPackets();
                }

                // Run callback (if provided) to notify of updated results
                OnResultsUpdate?.Invoke(_results);
            }

            _results.Stop();
            OnFinish?.Invoke(_results);
        }

        private void ReceivePacket(ref ICMP response, ref EndPoint responseEndPoint, ref TimeSpan replyTime, ref int bytesRead, long requestTimestamp)
        {
            if (_socket == null || _packet == null)
            {
                return;
            }

            byte[] receiveBuffer = new byte[_attributes.ReceiveBufferSize];
            int ttl = 0;

            // Wait for request
            do
            {
                // Cancel if requested
                _cancellationToken.ThrowIfCancellationRequested();

                // Set receive timeout, limited to 250ms so we don't block very long without checking for
                // cancellation. If the requested ping timeout is longer, we will wait some more in subsequent
                // loop iterations until the requested ping timeout is reached.
                int remainingTimeout = (int)Math.Ceiling(_attributes.Timeout - replyTime.TotalMilliseconds);
                if (remainingTimeout <= 0)
                {
                    throw new SocketException();
                }
                SetSocketReceiveTimeout(Math.Min(remainingTimeout, 250));

                // Wait for response
                try
                {
                    bytesRead = _socket.ReceiveFrom(receiveBuffer, ref responseEndPoint);
                }
                catch (SocketException)
                {
                    bytesRead = 0;
                }
                replyTime = new TimeSpan(Helper.StopwatchToTimeSpanTicks(Stopwatch.GetTimestamp() - requestTimestamp));

                if (bytesRead != 0)
                {
                    // Parse outter IPv4 header
                    IPv4 header = new IPv4(receiveBuffer, bytesRead);
                    ttl = header.TimeToLive;

                    // Store reply packet
                    response = new ICMP(receiveBuffer, bytesRead, header.HeaderLength);

                    if (_debugIpHeader)
                    {
                        // Print out parsed IPv4 header data
                        IPv4 ipheader = new(receiveBuffer, bytesRead);
                        if (ipheader.Data != null)
                        {
                            ICMP ping = new(ipheader.Data, ipheader.TotalLength - ipheader.HeaderLength, 0);

                            ipheader.GetBytes();
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine(header.PrettyPrint());
                            Console.ResetColor();

                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine(ping.PrettyPrint());
                            Console.ResetColor();
                        }
                    }

                    // If we sent an echo and receive a response with a different identifier or sequence
                    // number, ignore it (it could correspond to an older request that timed out)
                    if (_packet.Type == 8 && response.Type == 0)
                    {
                        ushort responseSessionId = BitConverter.ToUInt16(response.Message, 0);
                        ushort responseSequenceNum = BitConverter.ToUInt16(response.Message, 2);
                        if (responseSessionId != _sessionId || responseSequenceNum != _currentSequenceNumber)
                        {
                            response = ICMP.EmptyPacket;
                        }
                    }
                }
            } while (response == ICMP.EmptyPacket);

            // Raise message on response
            if (OnReply != null)
            {
                _responseMessage.Packet = response;
                _responseMessage.EndpointAddress = responseEndPoint.ToString() ?? string.Empty;
                _responseMessage.Timestamp = DateTime.Now;
                _responseMessage.SequenceNumber = _currentSequenceNumber;
                _responseMessage.BytesRead = bytesRead;
                _responseMessage.RoundTripTime = replyTime;
                _responseMessage.TimeToLive = ttl;

                OnReply.Invoke(_responseMessage);
            }
        }
    }
}