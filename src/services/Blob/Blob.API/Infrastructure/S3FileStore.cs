namespace Blob.API.Infrastructure
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Transfer;
    using Configuration;
    using Core;
    using Microsoft.VisualBasic;

    public class S3FileStore : IFileStore
    {
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;
        private readonly string prefix;

        public S3FileStore(S3Settings settings)
        {
            this.bucketName = settings.BucketName;
            this.prefix = settings.Prefix;
            this.s3Client = new AmazonS3Client(new AmazonS3Config()
            {
                ServiceURL = settings.ServiceUrl,
                ForcePathStyle = settings.ForcePathStyle
            });
        }

        public async Task StoreAsync(string key, Stream stream, CancellationToken token)
        {
            var utility = new TransferUtility(this.s3Client);
            await utility.UploadAsync(stream, this.bucketName, Strings.Join(new [] {this.prefix, key}, "/"), token);
        }

        public async Task RemoveAsync(string key, CancellationToken token) =>
            await this.s3Client.DeleteAsync(this.bucketName, Strings.Join(new [] {this.prefix, key}, "/"), null, token);
    }
}
