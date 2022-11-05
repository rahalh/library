namespace Blob.Infrastructure.Tests.Adapters.TestContainers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Configuration;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Containers;
    using Xunit;

    public class LocalstackTestContainer : IAsyncLifetime
    {
        private readonly TestcontainersContainer container;
        private readonly int localstackport;
        public string LocalstackUri => $"http://localhost:{this.localstackport}";

        private readonly string bucketName = "blob";
        private readonly string tableName = "blob";

        public readonly S3Settings S3Settings;
        public readonly DDBSettings DDBSettings;

        public LocalstackTestContainer()
        {
            this.localstackport = Random.Shared.Next(4000, 5000);

            this.S3Settings = new S3Settings(this.bucketName, this.LocalstackUri) {ServiceUrl = this.LocalstackUri, ForcePathStyle = true};
            this.DDBSettings = new DDBSettings(this.tableName, this.LocalstackUri);

            this.container = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack")
                .WithCleanUp(true)
                .WithAutoRemove(true)
                .WithPortBinding(this.localstackport, 4566)
                .WithBindMount(ToAbsolute("./Adapters/TestContainers/scripts/init-aws.sh"), "/etc/localstack/init/ready.d/init-aws.sh")
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(4566)
                        .AddCustomWaitStrategy(new LocalstackContainerHealthCheck(this.LocalstackUri))
                )
                .Build();
        }

        public async Task InitializeAsync() => await this.container.StartAsync();

        public async Task DisposeAsync() => await this.container.DisposeAsync();

        private static string ToAbsolute(string path) => Path.GetFullPath(path);
    }
}
