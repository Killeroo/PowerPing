using System.Net;

namespace PowerPing
{
    public abstract class PingEventData
    {
        
    }
    
    public class PingError : PingEventData
    {
        public DateTime Timestamp;
        public string Message;
        public Exception Exception;
        public bool Fatal;
    }
    
    public class PingReply : PingEventData
    {
        public ICMP Packet;
        public string EndpointAddress;
        public DateTime Timestamp;
        public int SequenceNumber;
        public TimeSpan RoundTripTime;
        public int TimeToLive;
        public int BytesRead;
    }
    
    public class PingRequest : PingEventData
    {
        public IPEndPoint Destination;
        public ICMP Packet;
        public DateTime Timestamp;
        public int SequenceNumber;
        public int PacketSize;
    }
    
    public class PingTimeout : PingEventData
    {
        public IPEndPoint? Endpoint;
        public DateTime Timestamp;
        public int SequenceNumber;
    }
}
