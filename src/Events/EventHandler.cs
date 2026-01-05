
namespace PowerPing
{
    public class PingEventHandlerBase
    {
        public void Register(Ping ping)
        {
            ping.OnError += OnError;
            ping.OnFinish += OnFinish;
            ping.OnReply += OnReply;
            ping.OnRequest += OnRequest;
            ping.OnStart += OnStart;
            ping.OnTimeout += OnTimeout;
        }
        
        public virtual void OnError(PingError error) { }
        
        public virtual void OnFinish(PingResults results) { }
        
        public virtual void OnReply(PingReply response) { }
        
        public virtual void OnRequest(PingRequest request) { }
        
        public virtual void OnStart(PingAttributes attributes) { }
        
        public virtual void OnTimeout(PingTimeout timeout) { }
    }

    public interface IScanEventHandler
    {
        public void Register()
        {
            Scan.OnScanProgress += OnScanProgress;
            Scan.OnScanFinished += OnScanFinished;
        }

        public void OnScanProgress(Scan.ProgressEvent progress) { }

        public void OnScanFinished(Scan.ResultsEvent results) { }
    }
}