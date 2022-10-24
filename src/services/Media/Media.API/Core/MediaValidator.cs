using FluentValidation;

namespace Media.API.Core
{
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
                .Must(x =>
                    x == MediaType.MediaBook ||
                    x == MediaType.MediaPodcast ||
                    x == MediaType.MediaVideo
                )
                .NotNull();

            this.RuleFor(x => x.Status)
                .Must(x =>
                    x == Status.StatusDone ||
                    x == Status.StatusPending
                )
                .NotNull();

            // TODO LanguageCode validation rules
            this.RuleFor(x => x.TotalViews)
                .GreaterThanOrEqualTo(0)
                .NotNull();

            this.RuleFor(x => x.ExternalId).NotNull();
        }
    }
}
