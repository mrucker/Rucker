using System;

namespace Rucker.Exceptions
{
    /// <summary>
    /// Messages from these exceptions will be shown directly to users
    /// </summary>
    public class UserMessageException: Exception
    {
        public UserMessageException(string message):base(message)
        { }

         public UserMessageException(string message, Exception innerException):base(message, innerException)
         { }
    }
}