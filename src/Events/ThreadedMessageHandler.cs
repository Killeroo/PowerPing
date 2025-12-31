
namespace PowerPing
{
    public class ThreadedMessageHandler
    {
        public DisplayConfiguration DisplayConfig = new();
        public PingAttributes Attributes = new();
        public CancellationToken Token = new();
        
        private object _lock = new();
        private List<object> _eventData = new();

        public ThreadedMessageHandler(DisplayConfiguration config, CancellationToken token)
        {
            Attributes = new();
            DisplayConfig = config ?? new DisplayConfiguration();
            Token = token;
        }
        
        public void FlushMessages()
        {
            
            foreach (Action action in actions)
            {
                action();
            }
        }

        public void OnError(PingError error)
        {
            lock (_lock)
            {
                _eventData.Add(error);
            }
        }
        

        public void OnFinish(PingResults results)
        {
            lock (_lock)
            {
                _eventData.Add(results);
            }
        }

        public void OnReply(PingReply response)
        {
            lock (_lock)
            {
                _eventData.Add(response);
            }
        }

        public void OnRequest(PingRequest request)
        {
            lock (_lock)
            {
                _eventData.Add(request);
            }
        }

        public void OnStart(PingAttributes attributes)
        {
            lock (_lock)
            {
                _eventData.Add(attributes);
            }
        }

        public void OnTimeout(PingTimeout timeout)
        {
            lock (_lock)
            {
                _eventData.Add(timeout);
            }
        }
    }
}