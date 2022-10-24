namespace Media.API.Ports.Events
{
    using System.Threading.Tasks;
    using Core;

    public interface IKafkaHandler<TK, TV>
    {
        Task HandleAsync(IMediaService srv, TK key, TV value);
    }
}
