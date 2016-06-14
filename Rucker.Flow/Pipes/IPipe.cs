using System.Threading.Tasks;

namespace Rucker.Flow
{
    public interface IPipe
    {
        PipeStatus PipeStatus { get; }
        Task Start();
        void Stop();
    }
}