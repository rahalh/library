namespace Media.Application.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models;
    using Domain.Services;
    using Exceptions;

    public class GetMediaInteractor
    {
        private readonly IMediaRepository repo;

        public GetMediaInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task<MediaDTO> HandleAsync(GetMediaRequest request, CancellationToken token)
        {
            var media = await this.repo.FetchByIdAsync(request.Id, token);
            if (media is null)
            {
                throw new NotFoundException($"Can't find Media with Id: {request.Id}");
            }
            media.TotalViews += 1;
            await this.repo.SetViewCountAsync(request.Id, media.TotalViews, token);
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

    public record GetMediaRequest(string Id);
}
