
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
        
        public void OnError(PingError error) { }

        public void OnFinish(PingResults results) { }
        
        public void OnReply(PingReply response) { }

        public void OnRequest(PingRequest request) { }

        public void OnStart(PingAttributes attributes) { }

        public void OnTimeout(PingTimeout timeout) { }
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