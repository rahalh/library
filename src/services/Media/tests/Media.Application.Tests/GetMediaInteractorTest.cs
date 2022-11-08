namespace Media.Application.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Services;
    using Exceptions;
    using Interactors;
    using Moq;
    using Shouldly;
    using Xunit;

    public class GetMediaInteractorTest
    {
        private readonly Mock<IMediaRepository> repo;
        private readonly GetMediaInteractor interactor;

        public GetMediaInteractorTest()
        {
            this.repo = new Mock<IMediaRepository>();
            this.interactor = new GetMediaInteractor(this.repo.Object);
        }

        [Fact]
        public async Task WhenIdIsNotFound_ThrowsNotFoundException()
        {
            // Arrange
            this.repo.Setup(x => x.FetchByIdAsync(It.IsAny<string>(), CancellationToken.None)).ReturnsAsync((Media) null);

            // Assert
            await Should.ThrowAsync<NotFoundException>(async () => await this.interactor.HandleAsync(new GetMediaRequest("id"), CancellationToken.None));
        }

        [Fact]
        public async Task WhenIdIsFound_ReturnsMediaAfterIncrementingViewCount()
        {
            var totalViews = 5;
            var media = new Media("title", "description", "en", DateTime.UtcNow, "book") {TotalViews = totalViews};

            // Arrange
            this.repo.Setup(x => x.FetchByIdAsync(media.ExternalId, CancellationToken.None)).ReturnsAsync(media);

            // Act
            var res = await this.interactor.HandleAsync(new GetMediaRequest(media.ExternalId), CancellationToken.None);

            // Assert
            this.repo.Verify(x => x.SetViewCountAsync(media.ExternalId, totalViews + 1, CancellationToken.None), Times.Once);

            res.ExternalId.ShouldBe(media.ExternalId);
            res.Title.ShouldBe(media.Title);
            res.Description.ShouldBe(media.Description);
            res.PublishDate.ShouldBe(media.PublishDate);
            res.LanguageCode.ShouldBe(media.LanguageCode);
            res.MediaType.ShouldBe(media.MediaType, StringCompareShould.IgnoreCase);
            res.TotalViews.ShouldBe(totalViews + 1);
            res.CreateTime.ShouldBe(media.CreateTime);
            res.UpdateTime.ShouldBe(media.UpdateTime);
            res.ContentURL.ShouldBe(media.ContentURL?.ToString());
        }
    }
}
