namespace Blob.Infrastructure.Tests.API.TestContainers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Amazon.Util;
    using Configuration;
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Containers;
    using Xunit;

    public class LocalstackTestContainer : IAsyncLifetime
    {
        private readonly TestcontainersContainer localstackContainer;
        private readonly TestcontainersContainer kafkaContainer;
        private readonly TestcontainersContainer zookeeperContainer;

        public readonly string LocalstackUri = "http://localhost:4566";

        private readonly string bucketName = "blob";
        private readonly string tableName = "blob";

        public readonly S3Settings S3Settings;
        public readonly DDBSettings DDBSettings;

        public LocalstackTestContainer()
        {
            this.localstackContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("localstack/localstack")
                .WithCleanUp(true)
                .WithAutoRemove(true)
                .WithPortBinding(4566)
                .WithBindMount(ToAbsolute("./API/TestContainers/scripts/init-aws.sh"), "/etc/localstack/init/ready.d/init-aws.sh")
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(4566)
                        .AddCustomWaitStrategy(new LocalstackContainerHealthCheck(this.LocalstackUri))
                )
                .Build();

            this.S3Settings = new S3Settings(this.bucketName, this.LocalstackUri) {ServiceUrl = this.LocalstackUri, ForcePathStyle = true};
            this.DDBSettings = new DDBSettings(this.tableName, this.LocalstackUri);

            this.zookeeperContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("bitnami/zookeeper")
                .WithCleanUp(true)
                .WithNetworkAliases("zookeeper")
                .WithPortBinding(2181)
                .WithEnvironment("ALLOW_ANONYMOUS_LOGIN", "true")
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(2181)
                )
                .Build();

            // todo create a network and attach these containers to it
            this.kafkaContainer = new TestcontainersBuilder<KafkaTestcontainer>()
                .WithImage("bitnami/kafka")
                .WithPortBinding(9092)
                .WithNetworkAliases("kafka")
                .WithEnvironment("KAFKA_BROKER_ID", "1")
                .WithEnvironment("KAFKA_CFG_ZOOKEEPER_CONNECT", "zookeeper:2181")
                .WithEnvironment("ALLOW_PLAINTEXT_LISTENER", "yes")
                .WithEnvironment("KAFKA_CFG_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT")
                .WithEnvironment("KAFKA_CFG_LISTENERS", "PLAINTEXT://:9092")
                .WithEnvironment("KAFKA_CFG_ADVERTISED_LISTENERS", "PLAINTEXT://kafka:9092")
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilPortIsAvailable(9092)
                )
                .Build();
        }

        public async Task InitializeAsync()
        {
            await this.localstackContainer.StartAsync();
            await this.zookeeperContainer.StartAsync();
            await Task.Delay(10000);
            await this.kafkaContainer.StartAsync();
            await Task.Delay(20000);
        }

        public async Task DisposeAsync()
        {
            await this.localstackContainer.DisposeAsync();
            await this.zookeeperContainer.DisposeAsync();
            await this.kafkaContainer.DisposeAsync();
        }

        private static string ToAbsolute(string path) => Path.GetFullPath(path);
    }
}
