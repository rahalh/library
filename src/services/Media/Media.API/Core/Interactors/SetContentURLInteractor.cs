namespace Media.API.Core.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;

    public class SetContentURLInteractor
    {
        private readonly IMediaRepository repo;

        public SetContentURLInteractor(IMediaRepository repo) => this.repo = repo;

        public async Task Handle(SetContentURLRequest request, CancellationToken token) => await this.repo.SetContentURL(request.Id, request.URL, token);
    }

    // todo add validation
    public record SetContentURLRequest(string Id, string URL);
}
