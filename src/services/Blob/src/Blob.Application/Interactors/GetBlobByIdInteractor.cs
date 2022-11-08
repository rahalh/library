namespace Blob.Application.Interactors
{
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Common.Models;
    using Domain;
    using Domain.Services;
    using Exceptions;
    using FluentValidation;
    using ValidationException = global::Blob.Application.Exceptions.ValidationException;

    public class GetBlobByIdInteractor
    {
        private readonly IBlobRepository repo;

        public GetBlobByIdInteractor(IBlobRepository repo) => this.repo = repo;

        public async Task<BlobDTO> HandleAsync(GetBlobByIdRequest req, CancellationToken token)
        {
            var validationResult = new GetBlobByIdRequest.Validator().Validate(req);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(JsonSerializer.Serialize(validationResult.ToDictionary()));
            }

            var blob = await this.repo.GetByIdAsync(req.Id, token);
            if (blob is null)
            {
                throw new NotFoundException($"Can't find Blob with Id: {req.Id}");
            }
            return new BlobDTO(blob.Id, blob.Name, blob.Size, blob.BlobType, blob.Extension, blob.URL, blob.CreateTime, blob.UpdateTime);
        }
    }

    public record GetBlobByIdRequest(string Id)
    {
        public class Validator : AbstractValidator<GetBlobByIdRequest>
        {
            public Validator() =>
                this.RuleFor(x => x.Id)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(30);
        }
    };
}
