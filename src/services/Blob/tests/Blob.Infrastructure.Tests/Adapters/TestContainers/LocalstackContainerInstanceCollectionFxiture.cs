namespace Blob.Infrastructure.Tests.Adapters.TestContainers
{
    using Xunit;

    [CollectionDefinition(nameof(LocalstackTestContainer))]
    public class LocalstackContainerInstanceCollectionFixture : ICollectionFixture<LocalstackTestContainer>
    {
    }
}
