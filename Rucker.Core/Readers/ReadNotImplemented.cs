namespace Rucker.Core
{
    public class ReadNotImplemented<T>: IRead<T>
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public int Size()
        {
            throw new System.NotImplementedException();
        }

        public T Read(int skip, int take)
        {
            throw new System.NotImplementedException();
        }
    }
}