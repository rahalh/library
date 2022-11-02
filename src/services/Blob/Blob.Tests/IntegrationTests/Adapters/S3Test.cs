namespace Blob.Tests.IntegrationTests.Adapters
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using API.Configuration;
    using API.Infrastructure;
    using Shouldly;
    using TestContainerSetup;
    using Xunit;

    [Collection(nameof(LocalstackTestContainer))]
    public class S3Test
    {
        private readonly string localstackUrl;

        public S3Test(LocalstackTestContainer container) => this.localstackUrl = container.LocalstackUri;

        [Fact]
        public async Task StoreAsync()
        {
            // Arrange
            var bucketName = "blob";
            var fileStore = new S3FileStore(new S3Settings(bucketName, this.localstackUrl) {ServiceUrl = this.localstackUrl, ForcePathStyle = true});

            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
            await writer.FlushAsync();
            stream.Position = 0;

            // Act
            await fileStore.StoreAsync(fileName, stream, CancellationToken.None);

            var httpClient = new HttpClient();
            var res = await httpClient.GetAsync($"{this.localstackUrl}/{bucketName}/{fileName}");

            // Assert
            res.StatusCode.ShouldBe(HttpStatusCode.OK);
            var resstream = await res.Content.ReadAsStreamAsync();
            var reader = new StreamReader(resstream);
            reader.ReadToEnd().ShouldBe(content);
        }
    }
}
