namespace Blob.Infrastructure.Tests.Adapters.TestContainerSetup
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Configurations;
    using DotNet.Testcontainers.Containers;
    using Xunit;

    public class LocalstackTestContainer : IAsyncLifetime
    {
        public readonly TestcontainersContainer container;
        private readonly int localstackport;
        public string LocalstackUri => $"http://localhost:{this.localstackport}";

        public LocalstackTestContainer()
        {
            this.localstackport = Random.Shared.Next(4000, 5000);
            this.container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack")
                .WithCleanUp(true)
                .WithPortBinding(this.localstackport, 4566)
                .WithBindMount(ToAbsolute("./Adapters/scripts"), "/etc/localstack/init/ready.d", AccessMode.ReadOnly)
                .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(4566)
                .AddCustomWaitStrategy(new LocalstackContainerHealthCheck(this.LocalstackUri)))
                .Build();
        }

        public async Task InitializeAsync() => await this.container.StartAsync();

        public async Task DisposeAsync() => await this.container.DisposeAsync();

        private static string ToAbsolute(string path) => Path.GetFullPath(path);
    }
}
