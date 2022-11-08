namespace Media.Application.Interactors
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models;
    using Domain;
    using Domain.Services;
    using FluentValidation;
    using Helpers;
    using Newtonsoft.Json;
    using ValidationException = global::Media.Application.Exceptions.ValidationException;

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

            var media = new Media(request.Title, request.Description, request.LanguageCode, request.PublishDate!.Value,
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
                media.ContentURL?.ToString(),
                media.TotalViews,
                media.PublishDate
            );
        }
    }

    // todo validation error messages (incorrect format)
    // todo media type error message unclear
    public record CreateMediaRequest(
        string Title,
        string Description,
        string LanguageCode,
        string MediaType,
        DateTime? PublishDate
    )
    {
        public class Validator : AbstractValidator<CreateMediaRequest>
        {
            public Validator()
            {
                this.RuleFor(x => x.Title)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(255)
                    .MinimumLength(1);

                this.RuleFor(x => x.Description)
                    .NotNull()
                    .NotEmpty()
                    .MaximumLength(1024)
                    .MinimumLength(1);

                this.RuleFor(x => x.PublishDate)
                    .NotNull();

                this.RuleFor(x => x.MediaType)
                    .NotNull()
                    .NotEmpty()
                    .Must(x => EnumUtils.TryParseWithMemberName<MediaType>(x, out _))
                    .WithErrorCode("The provided media type is unsupported");

                // todo language code can be null, but if present must be valid
                this.RuleFor(x => x.LanguageCode)
                    .NotNull()
                    .NotEmpty();
            }
        }
    };
}
