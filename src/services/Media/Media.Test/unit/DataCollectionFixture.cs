namespace Media.Test
{
    using Xunit;

    [CollectionDefinition(nameof(DataCollectionFixture))]
    public class DataCollectionFixture : ICollectionFixture<TestContainer>
    {
    }
}
