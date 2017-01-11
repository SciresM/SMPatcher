using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CodeIsle.LibIpsNet.Exceptions
{
    [Serializable]
    public class IpsNotThisException : Exception
    {
        public IpsNotThisException()
            : base() { }

        public IpsNotThisException(string message)
            : base(message) { }

        public IpsNotThisException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public IpsNotThisException(string message, Exception innerException)
            : base(message, innerException) { }

        public IpsNotThisException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected IpsNotThisException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
