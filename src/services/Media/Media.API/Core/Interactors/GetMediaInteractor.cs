namespace Media.API.Core.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;
    using Adapters.Exceptions;

    public class GetMediaInteractor
    {
        private readonly IMediaRepository repo;

        public GetMediaInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task<MediaDTO> Handle(GetMediaRequest request, CancellationToken token)
        {
            var media = await this.repo.FetchById(request.Id, token);
            if (media is null)
            {
                throw new NotFoundException();
            }
            media.TotalViews += 1;
            await this.repo.SetViewCount(request.Id, media.TotalViews, token);
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

    public record GetMediaRequest(string Id);
}
