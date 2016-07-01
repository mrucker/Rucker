using System;

namespace Rucker.Flow
{
    public interface IErrorReporter
    {
        void Report(Exception exception);
    }
}