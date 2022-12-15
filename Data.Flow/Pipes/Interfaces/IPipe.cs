using System;

namespace Data.Flow
{
    public interface IPipe: IDisposable
    {
        PipeStatus Status { get; }
    }
}