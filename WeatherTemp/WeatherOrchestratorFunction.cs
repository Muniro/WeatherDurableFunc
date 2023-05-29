using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System.Threading;

namespace WeatherTemp
{
    public static class WeatherOrchestratorFunction
    {

        private const int MaxRetries = 3;
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);

        [FunctionName("OrchestratorFunction")]   
        
        public static async Task<IActionResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            // Get input from the orchestrator function
            var input = context.GetInput<YourInputType>();

            int retryCount = 0;
            bool success = false;

            while (!success && retryCount < MaxRetries)
            {
                try
                {
                    // Call the API
                    using (var httpClient = new HttpClient())
                    {
                        var response = await httpClient.GetAsync("YOUR_API_URL");
                        response.EnsureSuccessStatusCode();

                        var apiResult = await response.Content.ReadAsStringAsync();

                        // Process the API result or perform further actions

                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    log.LogWarning($"API call failed with error: {ex.Message}");

                    // Wait for the specified interval before retrying
                    await context.CreateTimer(context.CurrentUtcDateTime.Add(RetryInterval), CancellationToken.None);

                    retryCount++;
                }
            }

            if (success)
            {
                return new OkObjectResult("Orchestration completed successfully.");
            }
            else
            {
                return new StatusCodeResult(500); // Or any appropriate error response
            }
        }

    }
}
