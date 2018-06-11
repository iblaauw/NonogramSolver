using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{
    [TestClass]
    class DummyTest : IDisposable
    {
        public void Dispose()
        {
            Debug.WriteLine("And dispose works too :D");
        }

        [TestMethod]
        public void MyFirstTest()
        {
            Debug.WriteLine("Yay it actually ran!");
        }
    }
}
