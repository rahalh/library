namespace Media.Application
{
    using System;

    public record MediaDTO(
        string Title,
        string Description,
        string LanguageCode,
        string MediaType,
        DateTime CreateTime,
        DateTime UpdateTime,
        string ExternalId,
        string ContentURL,
        int TotalViews,
        DateTime PublishDate
    );
}
