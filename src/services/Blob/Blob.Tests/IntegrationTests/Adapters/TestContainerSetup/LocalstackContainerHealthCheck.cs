namespace Blob.Tests.IntegrationTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Microsoft.Extensions.Logging;

    public class LocalstackContainerHealthCheck : IWaitUntil
    {
        private readonly string endpoint;

        public LocalstackContainerHealthCheck(string endpoint) => this.endpoint = endpoint;

        public async Task<bool> Until(ITestcontainersContainer testcontainers, ILogger logger)
        {
            // using var httpClient = new HttpClient {BaseAddress = new Uri(this.endpoint)};
            // JsonNode? res;
            // try
            // {
            //     res = await httpClient.GetFromJsonAsync<JsonNode>("/_localstack/init/ready");
            //
            // }
            // catch (Exception e)
            // {
            //     return false;
            // }
            //
            // if (res is null)
            //     return false;
            //
            // var scripts = res["scripts"];
            // if (scripts is null)
            //     return false;
            //
            // foreach (var script in scripts.Deserialize<IEnumerable<Script>>() ?? Enumerable.Empty<Script>())
            // {
            //     if (!"READY".Equals(script.Stage, StringComparison.OrdinalIgnoreCase))
            //     {
            //         continue;
            //     }
            //
            //     if (!"init.sh".Equals(script.Name, StringComparison.OrdinalIgnoreCase))
            //     {
            //         continue;
            //     }
            //
            //     return "SUCCESSFUL".Equals(script.State, StringComparison.OrdinalIgnoreCase);
            // }

            await Task.Delay(30000);
            return true;
        }

        public record Script(
            [property: JsonPropertyName("stage")] string Stage,
            [property: JsonPropertyName("state")] string State,
            [property: JsonPropertyName("Name")] string Name
        );

    }
}
