namespace Media.Test.UnitTests.Interactors
{
    using System;
    using System.Threading;
    using API.Core;
    using API.Core.Interactors;
    using Moq;
    using Serilog.Core;
    using Shouldly;
    using Xunit;
    using NotFoundException = API.Core.Exceptions.NotFoundException;

    public class DeleteMediaInteractorTest
    {
        private readonly Mock<IMediaRepository> repo;
        private readonly DeleteMediaInteractor interactor;
        private readonly Mock<IEventProducer> eventProducer;

        public DeleteMediaInteractorTest()
        {
            this.repo = new Mock<IMediaRepository>();
            this.eventProducer = new Mock<IEventProducer>();
            this.interactor = new DeleteMediaInteractor(Logger.None, this.repo.Object, this.eventProducer.Object);
        }

        // transaction scope
        [Fact]
        public async void WhenIdIsNotFound_ThrowNotFoundException()
        {
            // Arrange
            var req = new DeleteMediaRequest("id");
            this.repo.Setup(x => x.CheckExistsAsync(req.Id, CancellationToken.None)).ReturnsAsync(false);

            // Act + Assert
            await Should.ThrowAsync<NotFoundException>(() => this.interactor.HandleAsync(req, CancellationToken.None));
        }

        [Fact]
        public async void WhenIdIsFoundAndDeletionSucceeds_AnEventIsPublished()
        {
            // Arrange
            var req = new DeleteMediaRequest("id");
            this.repo.Setup(x => x.CheckExistsAsync(req.Id, CancellationToken.None)).ReturnsAsync(true);

            // Act
            await this.interactor.HandleAsync(req, CancellationToken.None);

            // Assert
            this.repo.Verify(x => x.RemoveAsync(req.Id, CancellationToken.None), Times.Once);
            this.eventProducer.Verify(
                x => x.ProduceAsync(
                    It.Is<Event<DeleteMediaEvent>>(y =>
                        y.EventType == ProducedEvents.MediaRemoved && y.Content == new DeleteMediaEvent(req.Id)),
                    CancellationToken.None), Times.Once);
        }

        // todo test transaction scope

        [Fact]
        public async void WhenIdIsFoundAndDeletionFails_ThrowsException()
        {
            // Arrange
            var req = new DeleteMediaRequest("id");
            this.repo.Setup(x => x.CheckExistsAsync(req.Id, CancellationToken.None)).ReturnsAsync(true);
            this.repo.Setup(x => x.RemoveAsync(req.Id, CancellationToken.None)).Throws<Exception>();

            // Act
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            // Assert
            this.repo.Verify(x => x.RemoveAsync(req.Id, CancellationToken.None), Times.Once);
            this.eventProducer.Verify(
                x => x.ProduceAsync(
                    It.Is<Event<DeleteMediaEvent>>(y =>
                        y.EventType == ProducedEvents.MediaRemoved && y.Content == new DeleteMediaEvent(req.Id)),
                    CancellationToken.None), Times.Never);
        }

        [Fact]
        public async void WhenIdIsFoundAndEventPublishFails_ThrowsExceptionAndRollsBackRemoval()
        {
            // Arrange
            var req = new DeleteMediaRequest("id");
            this.repo.Setup(x => x.CheckExistsAsync(req.Id, CancellationToken.None)).ReturnsAsync(true);
            this.eventProducer.Setup(x => x.ProduceAsync(It.IsAny<Event<It.IsAnyType>>(), CancellationToken.None)).Throws<Exception>();

            // Act
            await Should.ThrowAsync<Exception>(async () => await this.interactor.HandleAsync(req, CancellationToken.None));

            // Assert
            this.repo.Verify(x => x.RemoveAsync(req.Id, CancellationToken.None), Times.Once);
            this.eventProducer.Verify(
                x => x.ProduceAsync(
                    It.Is<Event<DeleteMediaEvent>>(y =>
                        y.EventType == ProducedEvents.MediaRemoved && y.Content == new DeleteMediaEvent(req.Id)),
                    CancellationToken.None), Times.Once);
            // todo verify rollback (I don't think it is possible to do with my particular implementation)
        }
    }
}
