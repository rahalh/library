using FluentValidation;

namespace Media.API.Core
{
    using Helpers;

    public class MediaValidator : AbstractValidator<Media>
    {
        public MediaValidator()
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
                .WithErrorCode("unsupported media type")
                .When(x => !string.IsNullOrEmpty(x.MediaType));

            this.RuleFor(x => x.Status)
                .Must(x =>
                    x == StatusEnum.StatusDone ||
                    x == StatusEnum.StatusPending)
                .NotNull();

            // TODO LanguageCode validation rules
            this.RuleFor(x => x.TotalViews)
                .GreaterThanOrEqualTo(0)
                .NotNull();

            this.RuleFor(x => x.ExternalId).NotNull();
        }
    }
}
