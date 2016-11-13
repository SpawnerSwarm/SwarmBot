using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmBot
{
    enum GenericErrorCode { };
    enum XMLErrorCode
    {
        NotFound = 404,
        MultipleFound = 405,
        Maximum = 301,
        Greater = 302,
        Old = 501,
        Unknown = 000
    }
    enum NexusErrorCode
    {
        NotFound = 404,
        MultipleFound = 405
    }

    class XMLException : Exception
    {
        public XMLErrorCode errorCode { get; }

        public XMLException(XMLErrorCode errorCode)
            : base("XML Exception was thrown. Catch expected.")
        {
            this.errorCode = errorCode;
        }
    }
}
