using System;

namespace Rucker.Data
{
    /// <summary>
    /// Able to Write a given Type
    /// </summary>
    /// <typeparam name="T">The Type that is to be written</typeparam>
    public interface IWrite<in T>: IDisposable
    {
        /// <summary>
        /// Write the given data
        /// </summary>
        /// <param name="data">The data to write</param>
        void Write(T data);
    }
}
