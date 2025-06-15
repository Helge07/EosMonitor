using System;

namespace EosMonitor
{
    public class EosExceptionEventArgs : EventArgs
    {
        public Exception? Exception { get; internal set; }
    }

    public class EosException : Exception
    {
        public ErrorCode EosErrorCode { get; set; }
        public string EosErrorMessage { get; set; }

        internal EosException(uint eosErrorCode, string message) : base(message)
        {
            EosErrorCode = (ErrorCode)eosErrorCode;
            EosErrorMessage = message;
        }

        internal EosException(uint eosErrorCode, string message, Exception innerException) : base(message, innerException)
        {
            EosErrorCode = (ErrorCode)eosErrorCode;
            EosErrorMessage = message;
        }
    }
}
