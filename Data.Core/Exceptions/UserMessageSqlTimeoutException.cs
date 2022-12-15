using System;

namespace Data.Core
{
    public class UserMessageSqlTimeoutException: UserMessageException
    {
        public UserMessageSqlTimeoutException(Exception innerException): base("A sql timeout occurred -- please try your action again", innerException)
        { }
    }
}