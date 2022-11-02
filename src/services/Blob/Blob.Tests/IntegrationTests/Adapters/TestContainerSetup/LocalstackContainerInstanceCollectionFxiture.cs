namespace Blob.Tests.IntegrationTests.Adapters.TestContainerSetup
{
    using Xunit;

    [CollectionDefinition(nameof(LocalstackTestContainer))]
    public class LocalstackContainerInstanceCollectionFixture : ICollectionFixture<LocalstackTestContainer>
    {
    }
}
