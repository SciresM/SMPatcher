using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CodeIsle.LibIpsNet.Exceptions
{
    [Serializable]
    public class IpsInvalidException : Exception
    {
        public IpsInvalidException()
            : base() { }

        public IpsInvalidException(string message)
            : base(message) { }

        public IpsInvalidException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public IpsInvalidException(string message, Exception innerException)
            : base(message, innerException) { }

        public IpsInvalidException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected IpsInvalidException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
