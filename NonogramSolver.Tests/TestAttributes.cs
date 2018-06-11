using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TestClassAttribute : Attribute
    {
        public TestClassAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class TestMethodAttribute : Attribute
    {
        public TestMethodAttribute()
        {
        }
    }

}
