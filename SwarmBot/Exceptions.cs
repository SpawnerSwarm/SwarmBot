using System;

namespace SwarmBot
{
    enum XMLErrorCode
    {
        NotFound = 404,
        MultipleFound = 405,
        Maximum = 301,
        Greater = 302,
        Old = 501,
        Unknown = 000
    }

    class XMLException : Exception
    {
        public XMLErrorCode errorCode;
        public string message;

        public XMLException(XMLErrorCode errorCode, string message = null)
            :base("XML Exception was thrown. Catch expected. " + message + " " + (int)errorCode + " ")
        {
            this.errorCode = errorCode;
            this.message = message;
        }
    }
}