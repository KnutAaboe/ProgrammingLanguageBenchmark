using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using prog;

namespace Csharp
{
    public static class HttpTriggers

    {
        [FunctionName("HttpTriggerS")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return new OkObjectResult("Hello World");
        }

        [FunctionName("HttpTriggerBT")]
        public static void RunTwo(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            BinaryTrees.Main();
        }

        [FunctionName("HttpTriggerFR")]
        public static void RunThree(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            FannkuchRedux.Main();
        }

        [FunctionName("HttpTriggerF")]
        public static void RunFour(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Fasta.Main();
        }
    }
}
