using System;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main()
        {
            var detector = new UnobservedTaskExceptionDetector(Console.WriteLine);

            Console.WriteLine("\n************************* Manually thrown exceptions");
            try
            {
                detector.InspectManuallyTriggeredExceptions();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("\n************************* Exception after cancellation passing token to ContinueWith");
            try
            {
                detector.InspectPostCancellationExceptionWhenCanceledTokenIsPassedToContinueWith();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("\n************************* Exception after cancellation passing CancellationToken.None to ContinueWith");
            try
            {
                detector.InspectPostCancellationExceptionWhenNoneTokenIsPassedToContinueWith();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}