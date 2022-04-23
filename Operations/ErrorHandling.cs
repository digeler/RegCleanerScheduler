namespace RegCleanerScheduler ;

    public class ErrorHandling
    {
        private static ILogger<ErrorHandling> _logger;

        public ErrorHandling(ILogger<ErrorHandling> logger)
        {
            _logger = logger;
        }
        public static void HandleException(Exception ex)
        {
            _logger.LogCritical($"Coravel encountered exception details " +
                                $"{ex.Message} \n {ex.StackTrace} \n {ex.InnerException}");

        }
    }