using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class UnobservedTaskExceptionDetector
    {
        private const int DefaultSleepTimeMs = 100;
        private const int DefaultCompletionWaitTimeMs = 5000;

        private readonly Action<string> writeToOutput;

        public int SleepTimeMs { get; init; }

        public int CompletionWaitTimeMs { get; init; }


        public UnobservedTaskExceptionDetector(Action<string> writeToOutput, int sleepTimeMs = DefaultSleepTimeMs, int completionTimeMs = DefaultCompletionWaitTimeMs)
        {
            this.writeToOutput = writeToOutput;
            SleepTimeMs = sleepTimeMs;
            CompletionWaitTimeMs = completionTimeMs;
        }

        public void InspectManuallyTriggeredExceptions()
        {
            ThrowOnUnobservedTaskException(() =>
                Task
                    .Factory
                    .StartNew(() => throw new DivideByZeroException())
                    .ContinueWith(_ => string.Empty)
                    .ContinueWith(_ => throw new InvalidOperationException())
            );
        }

        public void InspectPostCancellationExceptionWhenCanceledTokenIsPassedToContinueWith()
        {
            InspectPostCancellationException(true);
        }

        public void InspectPostCancellationExceptionWhenNoneTokenIsPassedToContinueWith()
        {
            InspectPostCancellationException(false);
        }

        private void InspectPostCancellationException(bool passCancellationTokenToContinueWith)
        {
            ThrowOnUnobservedTaskException(() =>
            {
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                Task
                    .Factory
                    .StartNew(() =>
                    {
                        cts.Cancel();
                        throw new DivideByZeroException();
                    }, token)
                    .ContinueWith
                    (
                        t => writeToOutput($"Handled exception {t.Exception?.GetBaseException().GetType()}"),
                        passCancellationTokenToContinueWith ? token : CancellationToken.None
                    );
            });
        }

        private void ThrowOnUnobservedTaskException(Action action)
        {
            var mre = new ManualResetEvent(initialState: false);
            void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs eventArgs)
            {
                mre.Set();
                eventArgs.SetObserved();
                eventArgs.Exception.Handle(ex =>
                {
                    writeToOutput($"Exception type: {ex.GetType()}");
                    return true;
                });
            }
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            action();

            Thread.Sleep(SleepTimeMs);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            try
            {
                if (mre.WaitOne(CompletionWaitTimeMs))
                    throw new ApplicationException("UNOBSERVED TASK EXCEPTION DETECTED!!!");
            }
            finally
            {
                TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
            }
        }
    }
}