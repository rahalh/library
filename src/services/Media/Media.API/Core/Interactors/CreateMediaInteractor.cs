namespace Media.API.Core.Interactors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using Helpers;
    using Newtonsoft.Json;
    using ValidationException = global::Media.API.Core.Exceptions.ValidationException;

    public class CreateMediaInteractor
    {
        private readonly IMediaRepository repo;

        public CreateMediaInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task<MediaDTO> HandleAsync(CreateMediaRequest request, CancellationToken token)
        {
            var validationResult = new CreateMediaRequest.Validator().Validate(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(JsonConvert.SerializeObject(validationResult.ToDictionary()));
            }

            var media = new Media(request.Title, request.Description, request.LanguageCode, request.PublishDate,
                request.MediaType);
            await this.repo.SaveAsync(media, token);
            return new MediaDTO(
                media.Title,
                media.Description,
                media.LanguageCode,
                media.MediaType,
                media.CreateTime,
                media.UpdateTime,
                media.ExternalId,
                media.ContentURL,
                media.TotalViews,
                media.PublishDate
            );
        }
    }

    public record CreateMediaRequest(
        string Title,
        string Description,
        string LanguageCode,
        string MediaType,
        DateTime PublishDate
    )
    {
        public class Validator : AbstractValidator<CreateMediaRequest>
        {
            public Validator()
            {
                this.RuleFor(x => x.Title)
                    .NotNull()
                    .MaximumLength(255)
                    .MinimumLength(1);

                this.RuleFor(x => x.Description)
                    .NotNull()
                    .MaximumLength(1024)
                    .MinimumLength(1);

                this.RuleFor(x => x.PublishDate).NotNull();

                this.RuleFor(x => x.MediaType)
                    .NotNull()
                    .NotEmpty()
                    .Must(x => EnumUtils.TryParseWithMemberName<MediaType>(x, out _))
                    .WithErrorCode("unsupported media type");

                // todo language code can be null, but if present must be valid
            }
        }
    };
}
