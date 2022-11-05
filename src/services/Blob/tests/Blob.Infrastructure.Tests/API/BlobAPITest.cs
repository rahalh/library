namespace Blob.Infrastructure.Tests.API
{
    using TestContainers;
    using Xunit;

    public class BlobAPITest : IClassFixture<LocalstackTestContainer>
    {
        // private readonly HttpClient httpClient;
        // private readonly IntegrationTestFactory fixture;
        //
        // public BlobAPITest(IntegrationTestFactory fixture)
        // {
        //     this.fixture = fixture;
        //     this.httpClient = fixture.CreateClient();
        // }

        // todo should run kafka too
        // todo reset (ddb, s3, kafka) between test runs
    }
}
