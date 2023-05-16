using System;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Resilience.Bulkhead;
using Mochineko.Relent.Resilience.Retry;
using Mochineko.Relent.Resilience.Timeout;
using Mochineko.Relent.Resilience.Wrap;

namespace Mochineko.KoeiromapAPI.Samples
{
    internal static class PolicyFactory
    {
        private const float TotalTimeoutSeconds = 60f;
        private const float EachTimeoutSeconds = 30f;
        private const int MaxRetryCount = 5;
        private const float RetryIntervalSeconds = 1f;
        private const int MaxParallelization = 1;
        
        public static IPolicy<SpeechSynthesisResult> BuildPolicy()
        {
            var totalTimeoutPolicy = TimeoutFactory.Timeout<SpeechSynthesisResult>(
                timeout: TimeSpan.FromSeconds(TotalTimeoutSeconds));
            
            var retryPolicy = RetryFactory.RetryWithInterval<SpeechSynthesisResult>(
                MaxRetryCount,
                interval: TimeSpan.FromSeconds(RetryIntervalSeconds));

            var eachTimeoutPolicy = TimeoutFactory.Timeout<SpeechSynthesisResult>(
                timeout: TimeSpan.FromSeconds(EachTimeoutSeconds));

            var bulkheadPolicy = BulkheadFactory.Bulkhead<SpeechSynthesisResult>(
                MaxParallelization);

            return totalTimeoutPolicy
                .Wrap(retryPolicy)
                .Wrap(eachTimeoutPolicy)
                .Wrap(bulkheadPolicy);
        }
    }
}