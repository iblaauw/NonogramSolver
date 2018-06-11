using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{

    public class TestFailureException : Exception
    {
        public TestFailureException() { }
        public TestFailureException(string message) : base(message) { }
        public TestFailureException(string message, Exception inner) : base(message, inner) { }
    }

    public static class Assert
    {
        public static void True(bool value, string message)
        {
            if (!value)
                throw new TestFailureException(message);
        }

        public static void True(bool value)
        {
            if (!value)
                throw new TestFailureException("Value was false.");
        }

        public static void False(bool value, string message)
        {
            True(!value, message);
        }

        public static void False(bool value)
        {
            True(!value);
        }
    }
}
