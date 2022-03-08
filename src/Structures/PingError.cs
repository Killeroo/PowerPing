using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPing
{
    public struct PingError
    {
        public DateTime Timestamp;
        public string Message;
        public Exception Exception;
        public bool Fatal;
    }
}
