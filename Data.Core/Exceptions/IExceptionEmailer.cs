using System;

namespace Data.Core
{
    public interface IExceptionEmailer
    {
        void Email(Exception ex, string extra = null);
    }
}