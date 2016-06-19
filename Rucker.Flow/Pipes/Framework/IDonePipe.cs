using System.Security.Cryptography.X509Certificates;

namespace Rucker.Flow
{
    public interface IDonePipe: IPipe
    {
        void Start();
        void Stop();
    }
}