/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2026 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System;

namespace PowerPing
{
    public class LogMessageHandler : PingEventHandlerBase, IScanEventHandler, IDisposable
    {
        private const string DATETIME_STRING_FORMAT = "yyyy-MM-dd HH:mm:ss.fffzzzzz";

        private LogFile _logFile;
        private DisplayConfiguration _displayConfiguration;
        private string _destinationAddress = "";

        public LogMessageHandler(string logPath, DisplayConfiguration displayConfiguration)
        {
            _logFile = new LogFile(logPath);
            _displayConfiguration = displayConfiguration;
        }
        ~LogMessageHandler()
        {
            Dispose();
        }

        public override void OnStart(PingAttributes attributes)
        {
            _destinationAddress = attributes.InputtedAddress;
            string message = attributes.Message.Length > 50 ? $"{attributes.Message.Substring(0, 50)}...({attributes.Message.Length - 50} lines truncated)" : attributes.Message;

            _logFile.Append($"[{_destinationAddress}] " +
                $"[{DateTime.Now.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[START] " +
                $"input_address={attributes.InputtedAddress} " +
                $"resolved_address={attributes.ResolvedAddress} " +
                $"source_address={attributes.SourceAddress} " +
                $"message={message} " +
                $"interval={attributes.Interval}ms " +
                $"timeout={attributes.Timeout}ms " +
                $"count={attributes.Count} " +
                $"TTL={attributes.Ttl} " +
                $"receive_buffer_size={attributes.ReceiveBufferSize} bytes " +
                $"artifical_message_size={attributes.ArtificalMessageSize} bytes " +
                $"type={attributes.Type} " +
                $"code={attributes.Code} " +
                $"beep_mode={attributes.BeepMode} " +
                $"continous_mode={attributes.Continous} " +
                $"ipv4={attributes.UseICMPv4} " +
                $"ipv6={attributes.UseICMPv6} " +
                $"random_message={attributes.RandomMessage} " +
                $"dont_fragment={attributes.DontFragment} " +
                $"random_timing={attributes.RandomTiming} " +
                $"enable_logging={attributes.EnableFileLogging} " +
                $"log_filename={attributes.LogFilePath}");
        }

        public override void OnFinish(PingResults results)
        {
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{results.EndTime.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[FINISH] ellapsed={results.TotalRunTime} start_time={results.StartTime} ");
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{results.EndTime.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[STATISTICS] [PINGS] " +
                $"sent={results.Sent} " +
                $"received={results.Received} " +
                $"lost={results.Lost} ");
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{results.EndTime.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[STATISTICS] [PACKETS] " +
                $"good={results.GoodPackets} " +
                $"error={results.ErrorPackets} " +
                $"other={results.OtherPackets} ");
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{results.EndTime.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[STATISTICS] [TIME] " +
                $"max={results.MaxTime} " +
                $"min={results.MinTime} " +
                $"avg={results.AvgTime} ");
        }

        public override void OnTimeout(PingTimeout timeout)
        {
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{timeout.Timestamp.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[TIMEOUT] " +
                $"seq={timeout.SequenceNumber}");
        }

        public override void OnRequest(PingRequest request)
        {
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{request.Timestamp.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[REQUEST] " +
                $"seq={request.SequenceNumber} " +
                $"bytes={request.PacketSize} " +
                $"type={request.Packet.Type} " +
                $"code={request.Packet.Code}");
        }

        public override void OnReply(PingReply reply)
        {
            _logFile.Append($"[{_destinationAddress}] " +
                $"[{reply.Timestamp.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[REPLY] " +
                $"endpoint={reply.EndpointAddress} " +
                $"seq={reply.SequenceNumber} " +
                $"bytes={reply.BytesRead} " +
                $"type={reply.Packet.Type} " +
                $"code={reply.Packet.Code} " +
                $"ttl={reply.TimeToLive}" +
                $"time={reply.RoundTripTime.TotalMilliseconds}ms");
        }

        public override void OnError(PingError error)
        {
            _logFile.Append($"[{_destinationAddress}]" +
                $"[{error.Timestamp.ToString(DATETIME_STRING_FORMAT)}] " +
                $"[ERROR] " +
                $"message={error.Message} " +
                $"error={error.Exception.GetType().Name} ");
        }

        public void OnScanFinished(Scan.ResultsEvent results)
        {
            _logFile.Append($"[SCAN] " +
                $"[{DateTime.Now.ToString(DATETIME_STRING_FORMAT)}] " +
                $"Scan {(results.RanToEnd ? "complete" : "aborted")}. {results.Scanned} addresses pinged, {results.Hosts.Count} hosts found.");

            if (results.Hosts.Count > 0)
            {
                _logFile.Append("[SCAN] Found Hosts:");

                string hostsList = "";
                for (int i = 0; i < results.Hosts.Count; i++)
                {
                    hostsList += string.Format("[{0}] time={1}ms name={2}" + Environment.NewLine, results.Hosts[i].Address, results.Hosts[i].ResponseTime, results.Hosts[i].HostName != "" ? results.Hosts[i].HostName : "UNAVAILABLE");
                }
                _logFile.Append(hostsList);
            }
        }

        public void Dispose()
        {
            // Close log file on shutdown
            _logFile.Dispose();
        }
    }
}
