using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
namespace CodeIsle.LibIpsNet.Exceptions
{
    [Serializable]
    public class Ips16MegabytesException : Exception
    {
        public Ips16MegabytesException()
            : base() { }

        public Ips16MegabytesException(string message)
            : base(message) { }

        public Ips16MegabytesException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public Ips16MegabytesException(string message, Exception innerException)
            : base(message, innerException) { }

        public Ips16MegabytesException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected Ips16MegabytesException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
