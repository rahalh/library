namespace Media.Application.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Services;
    using Interactors;
    using Moq;
    using Shouldly;
    using Xunit;
    using ValidationException = Exceptions.ValidationException;

    public class CreateMediaInteractorTest
    {
        private readonly Mock<IMediaRepository> repo;
        private readonly CreateMediaInteractor interactor;

        public CreateMediaInteractorTest()
        {
            this.repo = new Mock<IMediaRepository>();
            this.interactor = new CreateMediaInteractor(this.repo.Object);
        }

        [Theory]
        [InlineData("invalid_type", true)]
        [InlineData("video", false)]
        [InlineData("Audio", false)]
        [InlineData("book", false)]
        [InlineData("boOk", false)]
        public async Task WhenReqIsInvalid_ThrowsValidationException(string mediaType, bool throws)
        {
            // Arrange
            var request = new CreateMediaRequest("title", "description", "en", mediaType, DateTime.UtcNow);

            // Act
            if (throws)
            {
                await Should.ThrowAsync<ValidationException>(async () => await this.interactor.HandleAsync(request, CancellationToken.None));
            }
        }

        [Fact]
        public async Task WhenReqValid_StoresAndReturnsMedia()
        {
            // Arrange
            var request = new CreateMediaRequest("title", "description", "en", "book", DateTime.UtcNow);

            // Act
            var res = await this.interactor.HandleAsync(request, CancellationToken.None);

            // Assert
            this.repo.Verify(x => x.SaveAsync(It.IsAny<Media>(), CancellationToken.None), Times.Once);

            res.Title.ShouldBe(request.Title);
            res.Description.ShouldBe(request.Description);
            res.LanguageCode.ShouldBe(request.LanguageCode, StringCompareShould.IgnoreCase);
            res.MediaType.ShouldBe(request.MediaType, StringCompareShould.IgnoreCase);
            res.ExternalId.ShouldNotBeNullOrEmpty();
            res.TotalViews.ShouldBe(0);
            res.PublishDate.ShouldBe(request.PublishDate);
        }
    }
}
