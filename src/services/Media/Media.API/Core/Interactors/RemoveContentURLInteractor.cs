namespace Media.API.Core.Interactors
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Exceptions;
    using FluentValidation;
    using ValidationException = FluentValidation.ValidationException;

    public class RemoveContentURLInteractor
    {
        private readonly IMediaRepository repo;

        public RemoveContentURLInteractor(IMediaRepository repo) => this.repo = repo;

        // Domain event (side-effect) does not contain a rollback operation
        public async Task HandleAsync(string message, CancellationToken token)
        {
            var request = JsonSerializer.Deserialize<Request>(message);

            var validationResults = new Request.Validator().Validate(request);
            if (!validationResults.IsValid)
            {
                throw new ValidationException(JsonSerializer.Serialize(validationResults.ToDictionary()));
            }

            var media = await this.repo.FetchById(request.Id, token);
            if (media is null)
            {
                throw new NotFoundException($"Can't find media with Id: {request.Id}");
            }

            await this.repo.SetContentURL(request.Id, null, token);
        }
    }

    public record Request(string Id)
    {
        public class Validator : AbstractValidator<Request>
        {
            public Validator() =>
                this.RuleFor(x => x.Id)
                    .NotNull()
                    .NotEmpty();
        }
    };
}
