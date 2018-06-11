using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{
    class TestStatistics
    {
        public TestStatistics()
        {
            numRun = 0;
            numPass = 0;
            failureNames = new List<string>();
        }

        public int NumberTestsRun { get { return numRun; } }
        public int NumberTestsPassed { get { return numPass; } }
        public float PercentPassed { get { return (100 * (float)numPass / numRun); } }
        public int NumTestsFailed { get { return numRun - numPass; } }
        public IReadOnlyList<string> FailedTests { get { return failureNames; } }

        public void AddTestResult(bool succeeded, string name)
        {
            numRun++;
            if (succeeded)
            {
                numPass++;
            }
            else
            {
                failureNames.Add(name);
            }
        }

        public void PrintSummary()
        {
            Debug.WriteLine("Summary:");
            Debug.WriteLine($"{numPass}/{numRun} ({PercentPassed}%) succeeded");
            if (failureNames.Count > 0)
            {
                Debug.WriteLine("Failed Tests:");
                foreach (string failure in failureNames)
                {
                    Debug.WriteLine($"!!  {failure}");
                }
            }
        }

        private int numRun;
        private int numPass;
        private List<string> failureNames;
    }
}