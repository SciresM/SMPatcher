using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
namespace CodeIsle.LibIpsNet.Exceptions
{
    [Serializable]
    public class IpsScrambledException : Exception
    {
        public IpsScrambledException()
            : base() { }

        public IpsScrambledException(string message)
            : base(message) { }

        public IpsScrambledException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public IpsScrambledException(string message, Exception innerException)
            : base(message, innerException) { }

        public IpsScrambledException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected IpsScrambledException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
