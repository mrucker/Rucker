using System;

namespace Data.Flow
{
    public interface IErrorReporter
    {
        void Report(Exception exception);
    }
}