using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NonogramSolver.Tests
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Used across multiple threads
        private TestManager testManager;

        public MainPage()
        {
            this.InitializeComponent();
            testManager = new TestManager();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            TestStatistics stats = new TestStatistics();

            statusTextBlock.Text = "Finding tests...";
            await Task.Run(() =>
            {
                testManager.FindAllTests();
            });

            statusTextBlock.Text = "Running all tests...";

            await Task.Run(() =>
            {
                testManager.RunAllTests(stats);
            });

            stats.PrintSummary();

            statusTextBlock.Text = "Finished";
            summaryTextBlock.Text = $"{stats.NumberTestsPassed}/{stats.NumberTestsRun} tests passed ({stats.PercentPassed}%)";
            failuresList.ItemsSource = stats.FailedTests;
        }
    }
}
