namespace Blob.Infrastructure.Tests.Adapters.TestContainerSetup
{
    using Xunit;

    [CollectionDefinition(nameof(LocalstackTestContainer))]
    public class LocalstackContainerInstanceCollectionFixture : ICollectionFixture<LocalstackTestContainer>
    {
    }
}
