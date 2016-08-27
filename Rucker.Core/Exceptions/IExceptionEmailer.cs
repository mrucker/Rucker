using System;

namespace Rucker.Core
{
    public interface IExceptionEmailer
    {
        void Email(Exception ex, string extra = null);
    }
}