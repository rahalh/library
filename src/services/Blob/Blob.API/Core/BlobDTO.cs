namespace Blob.API.Core
{
    using System;

    public record BlobDTO(
        string Id,
        string Name,
        long Size,
        string BlobType,
        string Extension,
        string URL,
        DateTime CreateTime,
        DateTime UpdateTime
    );
}
