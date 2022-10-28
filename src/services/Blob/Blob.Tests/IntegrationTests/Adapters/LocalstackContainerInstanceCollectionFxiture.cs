namespace Blob.Tests.IntegrationTests.Adapters
{
    using Xunit;

    [CollectionDefinition(nameof(LocalstackTestContainer))]
    public class LocalstackContainerInstanceCollectionFixture : ICollectionFixture<LocalstackTestContainer>
    {
    }
}
