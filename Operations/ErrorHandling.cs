namespace RegCleanerScheduler ;

    public class ErrorHandling
    {
        public static void HandleException(Exception ex)
        {
            throw new Exception($"Coravel encountered error {ex.Message} {ex.InnerException} {ex.StackTrace}");
        }
    }