namespace Media.Test
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;

    public class ApiWebApplicationFactory : WebApplicationFactory<Program>
    {
        public IConfiguration Config { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                this.Config = new ConfigurationBuilder()
                    .AddJsonFile("integrationsettings.json")
                    .Build();
                config.AddConfiguration(this.Config);
            });

            builder.ConfigureTestServices(services => { });
        }
    }
}
