using System;
using Microsoft.Extensions.Logging;

namespace Tests ;

    public class NullLogger : ILoggingClass
    {
    private readonly ILogger<NullLogger> logger;

        public NullLogger(ILogger<NullLogger> logger) =>
            this.logger = logger;

        public void LogCritical(Exception exception) =>
            this.logger.LogCritical(exception, exception.Message);
}