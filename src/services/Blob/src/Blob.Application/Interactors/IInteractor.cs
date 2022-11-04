namespace Blob.Application.Interactors
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IInteractor<T, R>
    {
        public Task<R> HandleAsync(T request, CancellationToken token);
    }
}
