namespace Media.API.Core
{
    public class PaginationParams
    {
        public string Token;
        public int Size;

        public PaginationParams(string token, int? size)
        {
            this.Token = token;
            if (size is not null && size > 0)
            {
                if (size > 100)
                {
                    this.Size = 100;
                }
                else
                {
                    this.Size = size.Value;
                }
            }
            else
            {
                this.Size = 10;
            }
        }
    }
}
