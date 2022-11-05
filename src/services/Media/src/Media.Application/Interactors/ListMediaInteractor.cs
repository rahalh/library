namespace Media.Application.Interactors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Domain;
    using Domain.Services;

    public class ListMediaInteractor
    {
        private readonly IMediaRepository repo;

        public ListMediaInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task<ListMediaResponse> HandleAsync(ListMediaRequest request, CancellationToken token)
        {
            var paginationParams = new PaginationParams(request.Token, request.PageSize);
            paginationParams.Size++;

            var medias = await this.repo.ListAsync(paginationParams, token);

            string? nextToken = null;
            if (medias.Count > paginationParams.Size - 1)
            {
                nextToken = medias[medias.Count - 1].ExternalId;
                medias = medias.SkipLast(1).ToList();
            }

            return new ListMediaResponse(medias.Select(x => new MediaDTO(
                x.Title,
                x.Description,
                x.LanguageCode,
                x.MediaType,
                x.CreateTime,
                x.UpdateTime,
                x.ExternalId,
                x.ContentURL,
                x.TotalViews,
                x.PublishDate
            )), nextToken);
        }
    }

    public record ListMediaRequest(int PageSize, string? Token);

    public record ListMediaResponse(IEnumerable<MediaDTO> Items, string NextToken);
}
