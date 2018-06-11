using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NonogramSolver.Tests
{
    class TestManager
    {
        public TestManager()
        {
        }

        public void FindAllTests()
        {
            Debug.WriteLine("Searching for tests...");
            BuildTestGrid();
            int numTests = GetTestCount();
            Debug.WriteLine($"Found {numTests} tests.");
        }

        public void RunAllTests(TestStatistics stats)
        {
            Debug.WriteLine("Beginning test run");

            foreach (var testClass in m_testGrid)
            {
                RunTestClass(testClass, stats);
            }

            Debug.WriteLine("End of test run");
        }

        private void BuildTestGrid()
        {
            var testClasses = FindAllTestsClasses();
            m_testGrid = testClasses.Select(tc => CreateTestData(tc)).ToList();
        }

        private int GetTestCount()
        {
            return m_testGrid.Sum(data => data.Methods.Count);
        }

        private static IEnumerable<TypeInfo> FindAllTestsClasses()
        {
            var currentAssembly = typeof(TestManager).GetTypeInfo().Assembly;
            return currentAssembly.DefinedTypes.Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.IsDefined(typeof(TestClassAttribute), true));
        }

        private static IEnumerable<MethodInfo> FindAllTestMethods(TypeInfo typeInfo)
        {
            return typeInfo.DeclaredMethods.Where(t => t.IsDefined(typeof(TestMethodAttribute), true));
        }

        private static TestClassData CreateTestData(TypeInfo typeInfo)
        {
            var testMethods = FindAllTestMethods(typeInfo).ToList();

            MethodInfo disposeMethod = null;
            if (typeof(IDisposable).IsAssignableFrom(typeInfo.AsType()))
            {
                disposeMethod = typeInfo.GetDeclaredMethod("Dispose");
            }

            return new TestClassData(typeInfo.AsType(), testMethods, disposeMethod);
        }

        private static void RunTestClass(TestClassData testData, TestStatistics stats)
        {
            object instance = Activator.CreateInstance(testData.Type);

            foreach (var testMethod in testData.Methods)
            {
                InvokeTest(instance, testMethod, stats);
            }

            testData.DisposeMethod?.Invoke(instance, null);
        }

        private static void InvokeTest(object instance, MethodInfo method, TestStatistics stats)
        {
            string testName = $"{method.DeclaringType.Name}.{method.Name}";
            Debug.WriteLine("");
            Debug.WriteLine($"-- Running test {testName}:");

            Exception failure = null;
            try
            {
                method.Invoke(instance, null);
            }
            catch(Exception ex)
            {
                failure = ex;
            }

            if (failure == null)
            {
                Debug.WriteLine("-- Test Passed --");
                stats.AddTestResult(true, testName);
            }
            else
            {
                Debug.WriteLine("!! Test Failed !!");
                Debug.WriteLine($"An exception of type {failure.GetType().FullName} was thrown: {failure.Message}");
                Debug.WriteLine(failure.StackTrace);
                stats.AddTestResult(false, testName);
            }
        }

        private class TestClassData
        {
            public TestClassData(Type testClass, List<MethodInfo> testMethods, MethodInfo disposeMethod)
            {
                Type = testClass;
                Methods = testMethods;
                DisposeMethod = disposeMethod;
            }

            public Type Type { get; private set; }
            public List<MethodInfo> Methods { get; private set; }
            public MethodInfo DisposeMethod { get; private set; }
        }

        private List<TestClassData> m_testGrid;
    }
}
