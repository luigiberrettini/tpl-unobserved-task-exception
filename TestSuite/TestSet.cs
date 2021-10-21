using System;
using ConsoleApplication;
using Xunit;
using Xunit.Abstractions;

namespace TestSuite
{
    public class TestSuite
    {
        private readonly UnobservedTaskExceptionDetector detector;

        public TestSuite(ITestOutputHelper testOutputHelper)
        {
            detector = new UnobservedTaskExceptionDetector(testOutputHelper.WriteLine);
        }

        [Fact]
        public void UnobservedExceptionIfNotHandled()
        {
            Assert.Throws<ApplicationException>(() => detector.InspectManuallyTriggeredExceptions());
        }

        [Fact]
        public void UnobservedExceptionIfCanceledTokenPassedToContinueWith()
        {
            Assert.Throws<ApplicationException>(() => detector.InspectPostCancellationExceptionWhenCanceledTokenIsPassedToContinueWith());
        }

        [Fact]
        public void NoUnobservedExceptionIfCancellationTokenNonePassedToContinueWith()
        {
            var exception = Record.Exception(() => detector.InspectPostCancellationExceptionWhenNoneTokenIsPassedToContinueWith());
            Assert.Null(exception);
        }
    }
}