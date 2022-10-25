namespace Blob.API.Adapters
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Transfer;
    using Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualBasic;

    public class S3FileStore : IFileStore
    {
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;
        private readonly string prefix;

        public S3FileStore(IConfiguration config)
        {
            this.bucketName = config["AWS:S3:BucketName"];
            this.prefix = config["AWS:S3:StoragePath"];
            this.s3Client = new AmazonS3Client();
        }

        public async Task StoreAsync(string key, Stream stream, CancellationToken token)
        {
            var utility = new TransferUtility(this.s3Client);
            await utility.UploadAsync(stream, this.bucketName, Strings.Join(new [] {this.prefix, key}, "/"));
        }

        public async Task RemoveAsync(string key, CancellationToken token) =>
            await this.s3Client.DeleteAsync(this.bucketName, Strings.Join(new [] {this.prefix, key}, "/"), null);
    }
}
