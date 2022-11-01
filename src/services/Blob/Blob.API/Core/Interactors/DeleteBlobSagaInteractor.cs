namespace Blob.API.Core.Interactors
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Serilog;

    public class DeleteBlobSagaInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;
        private readonly ILogger logger;

        public DeleteBlobSagaInteractor(ILogger logger, IBlobRepository repo, IFileStore fileStore)
        {
            this.repo = repo;
            this.fileStore = fileStore;
            this.logger = logger.ForContext<DeleteBlobSagaInteractor>().ForContext("Method", $"{typeof(DeleteBlobInteractor).FullName}.{nameof(this.HandleAsync)}");
        }

        public async Task HandleAsync(DeleteBlobSagaRequest req, CancellationToken token)
        {
            try
            {
                var blob = await this.repo.GetByIdAsync(req.Id, token);
                if (blob is null)
                {
                    throw new NotFoundException($"Can't find Blob with Id: {req.Id}");
                }

                await this.repo.RemoveAsync(blob.Id, token);
                await this.fileStore.RemoveAsync(blob.Name, token);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, ex.Message);
                throw;
            }
        }
    }

    public record DeleteBlobSagaRequest(string Id);
}
