namespace Media.Test
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;

    public class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                // var c = config
                //     .AddJsonFile("integrationsettings.json")
                //     .Build();
                // config.AddConfiguration(c);
            });

            builder.ConfigureTestServices(services => { });
        }
    }
}
