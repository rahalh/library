namespace Media.API.Core
{
    public class PaginationParams
    {
        public string Token { get; }
        public int Size { get; set; }

        public PaginationParams(string token, int? size)
        {
            this.Token = token;
            if (size is not null && size > 0)
            {
                this.Size = this.Size > 100 ? 100 : size.Value;
            }
            else
            {
                this.Size = 10;
            }
        }
    }
}
