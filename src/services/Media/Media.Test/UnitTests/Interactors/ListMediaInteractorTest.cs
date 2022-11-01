namespace Media.Test.UnitTests.Interactors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using API.Core;
    using API.Core.Exceptions;
    using API.Core.Interactors;
    using AutoFixture;
    using Moq;
    using Shouldly;
    using Xunit;

    public class ListMediaInteractorTest
    {
        private readonly Mock<IMediaRepository> repo;
        private readonly ListMediaInteractor interactor;

        public ListMediaInteractorTest()
        {
            this.repo = new Mock<IMediaRepository>();
            this.interactor = new ListMediaInteractor(this.repo.Object);
        }

        [Theory]
        [InlineData(6, 5, 5)]
        [InlineData(4, 10, 4)]
        [InlineData(11, 0, 10)]
        [InlineData(99, 101, 99)]
        [InlineData(0, 10, 0)]
        public async Task When_ReturnsMediaList(int numOfItemsReturnedByRepo, int requestedSize, int actualCount)
        {
            // Arrange
            var fixture = new Fixture();
            var medias = new List<Media>();
            for (var i = 0; i < numOfItemsReturnedByRepo; i++)
            {
                medias.Add(fixture.Create<Media>());
            }

            this.repo.Setup(x => x.ListAsync(It.IsAny<PaginationParams>(), CancellationToken.None)).ReturnsAsync(medias);

            // Act
            var res = await this.interactor.HandleAsync(new ListMediaRequest(requestedSize, null), CancellationToken.None);

            // Assert
            res.Items.Count().ShouldBe(actualCount);
            if (numOfItemsReturnedByRepo > 0 && numOfItemsReturnedByRepo > requestedSize)
            {
                res.NextToken.ShouldBe(medias.LastOrDefault()!.ExternalId);
            }
            else
            {
                res.NextToken.ShouldBeNull();
            }
        }
    }
}
