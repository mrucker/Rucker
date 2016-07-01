using System;

namespace Rucker.Flow
{
    public interface IPipe: IDisposable
    {
        PipeStatus Status { get; }
    }
}