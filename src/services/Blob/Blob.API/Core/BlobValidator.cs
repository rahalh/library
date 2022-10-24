namespace Blob.API.Core
{
    using System;
    using System.Linq;
    using FluentValidation;
    using Helpers;

    public class BlobValidator : AbstractValidator<Blob>
    {
        public BlobValidator()
        {
            this.RuleFor(x => x.Name)
                .MinimumLength(1)
                .MaximumLength(512)
                .NotNull();

            this.RuleFor(x => x.BlobType)
                .NotNull()
                .NotEmpty()
                .Must(x => EnumUtils.TryParseWithMemberName<BlobTypes>(x, out _))
                .When(x => !string.IsNullOrEmpty(x.BlobType));

            this.RuleFor(x => x.Extension)
                .NotNull()
                .MinimumLength(1)
                .MaximumLength(8)
                .Equal("pdf")
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Application)
                .Must(x => new[] {"mpeg", "vorbis", "aac", "mp3", "ogg"}.Contains(x))
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Audio)
                .Must(x => new[] {"h264", "mp4", "mpeg", "ogg"}.Contains(x))
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Video);

            this.RuleFor(x => x.URL)
                .MaximumLength(2048);

            this.RuleFor(x => x.Size)
                .NotNull()
                // max 25 MB
                .LessThan(25 * (long)Math.Pow(1024, 2))
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Application)
                // max 256 MB
                .LessThan(256 * (long)Math.Pow(1024, 2))
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Audio)
                // max 8 GB
                .LessThan(8192 * (long)Math.Pow(1024, 2))
                .When(x => EnumUtils.GetEnumValueOrDefault<BlobTypes>(x.BlobType) == BlobTypes.Video);
        }
    }
}
