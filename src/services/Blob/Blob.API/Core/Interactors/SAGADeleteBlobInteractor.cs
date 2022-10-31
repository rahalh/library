namespace Blob.API.Core.Interactors
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using Serilog;

    public class SAGADeleteBlobInteractor
    {
        private readonly IBlobRepository repo;
        private readonly IFileStore fileStore;
        private readonly ILogger logger;

        public SAGADeleteBlobInteractor(ILogger logger, IBlobRepository repo, IFileStore fileStore)
        {
            this.repo = repo;
            this.fileStore = fileStore;
            this.logger = logger.ForContext<SAGADeleteBlobInteractor>().ForContext("Method", $"{typeof(DeleteBlobInteractor).FullName}.{nameof(this.HandleAsync)}");
        }

        public async Task HandleAsync(string message, CancellationToken token)
        {
            try
            {
                var request = JsonSerializer.Deserialize<Request>(message);

                var blob = await this.repo.GetByIdAsync(request.Id, token);
                if (blob is null)
                {
                    throw new NotFoundException($"Can't find Blob with Id: {request.Id}");
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

        public record Request(string Id);
    }
}
