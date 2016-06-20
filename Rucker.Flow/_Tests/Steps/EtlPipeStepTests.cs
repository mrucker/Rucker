using System;
using System.Linq;
using NUnit.Framework;
using Rucker.Data;
using Rucker.Testing;


namespace Rucker.Flow.Tests
{
    [TestFixture]
    public class EtlPipeStepTests: EtlCodeStepTests
    {
        public EtlPipeStepTests() : base(() => new EtlPipeStep<IRows, IRows>())
        {
            
        }
    }
}