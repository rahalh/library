namespace Media.Application.Interactors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Models;
    using Domain.Services;

    public class ListMediaInteractor
    {
        private readonly IMediaRepository repo;

        public ListMediaInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task<ListMediaResponse> HandleAsync(ListMediaRequest request, CancellationToken token)
        {
            var medias = await this.repo.ListAsync(request.PageSize + 1, request.PageToken, token);

            string? nextToken = null;
            if (medias.Count > request.PageSize)
            {
                nextToken = medias[^1].ExternalId;
                medias = medias.SkipLast(1).ToList();
            }

            return new ListMediaResponse(
                medias.Select(x => new MediaDTO(
                        x.Title,
                        x.Description,
                        x.LanguageCode,
                        x.MediaType,
                        x.CreateTime,
                        x.UpdateTime,
                        x.ExternalId,
                        x.ContentURL?.ToString(),
                        x.TotalViews,
                        x.PublishDate
                    )
                ), nextToken);
        }
    }

    public record ListMediaRequest
    {
        public string? PageToken { get; }
        public int PageSize { get; }

        public ListMediaRequest(int? size, string? token)
        {
            this.PageToken = token;
            if (size is not null && size > 0)
            {
                this.PageSize = this.PageSize > 100 ? 100 : size.Value;
            }
            else
            {
                this.PageSize = 10;
            }
        }
    };

    public record ListMediaResponse(IEnumerable<MediaDTO> Items, string? NextToken);
}
