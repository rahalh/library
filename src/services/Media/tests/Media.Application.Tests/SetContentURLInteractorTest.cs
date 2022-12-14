namespace Media.Application.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain.Services;
    using Exceptions;
    using Interactors;
    using Moq;
    using Serilog.Core;
    using Services;
    using Shouldly;
    using Xunit;

    public class SetContentURLInteractorTest
    {
        private readonly Mock<IMediaRepository> repo;
        private readonly SetContentURLInteractor interactor;
        private readonly Mock<IEventProducer> eventProducer;

        public SetContentURLInteractorTest()
        {
            this.repo = new Mock<IMediaRepository>();
            this.eventProducer = new Mock<IEventProducer>();
            this.interactor = new SetContentURLInteractor(Logger.None, this.repo.Object, this.eventProducer.Object);
        }

        [Fact]
        public async Task WhenIdIsNotFound_ThrowsNotFoundExceptionAndProduceURLUpdateFailedEvent()
        {
            var id = "id";
            var URL = "URL";
            // Arrange
            this.repo.Setup(x => x.CheckExistsAsync(id, CancellationToken.None)).ReturnsAsync(false);

            // Assert
            await Should.ThrowAsync<NotFoundException>(async () =>
                await this.interactor.HandleAsync(new SetContentURLRequest(id, URL), CancellationToken.None));
            this.eventProducer.Verify(
                x => x.ProduceAsync(
                    It.Is<Event<MediaUpdateFailedEvent>>(y =>
                        y.EventType == ProducedEvents.MediaUpdateFailed &&
                        y.Content == new MediaUpdateFailedEvent(id, URL)), CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.CheckExistsAsync(id, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SetContentURLAsync(id, URL, CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task WhenIdIsFoundAndSaveOperationFails_ThrowsExceptionAndProducesUpdateFailedEvent()
        {
            var id = "id";
            var URL = "url";
            // Arrange
            this.repo.Setup(x => x.CheckExistsAsync(id, CancellationToken.None)).ReturnsAsync(true);
            this.repo.Setup(x => x.SetContentURLAsync(id, URL, CancellationToken.None)).Throws<Exception>();

            // Act
            await Should.ThrowAsync<Exception>(async () =>
                await this.interactor.HandleAsync(new SetContentURLRequest(id, URL), CancellationToken.None));

            // Assert
            // todo test event's content
            this.eventProducer.Verify(
                x => x.ProduceAsync(
                    It.Is<Event<MediaUpdateFailedEvent>>(y =>
                        y.EventType == ProducedEvents.MediaUpdateFailed &&
                        y.Content == new MediaUpdateFailedEvent(id, URL)),
                    CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.CheckExistsAsync(id, CancellationToken.None), Times.Once);
            this.repo.Verify(x => x.SetContentURLAsync(id, URL, CancellationToken.None), Times.Once);
        }
    }
}
