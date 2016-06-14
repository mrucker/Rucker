using System;

namespace Rucker.Exceptions
{
    public interface IExceptionEmailer
    {
        void Email(Exception ex, string extra = null);
    }
}