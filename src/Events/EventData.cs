
namespace PowerPing
{
    public abstract class EventData
    {
        
    }
    
    public class PingError : EventData
    {
        public DateTime Timestamp;
        public string Message;
        public Exception Exception;
        public bool Fatal;
    }
    
    public struct PingReply : EventData
    {
        public ICMP Packet;
        public string EndpointAddress;
        public DateTime Timestamp;
        public int SequenceNumber;
        public TimeSpan RoundTripTime;
        public int TimeToLive;
        public int BytesRead;
    }
    
    public struct PingRequest : EventData
    {
        public IPEndPoint Destination;
        public ICMP Packet;
        public DateTime Timestamp;
        public int SequenceNumber;
        public int PacketSize;
    }
    
    public struct PingTimeout
    {
        public IPEndPoint? Endpoint;
        public DateTime Timestamp;
        public int SequenceNumber;
    }
}
