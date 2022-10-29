namespace Blob.API.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using Configuration;
    using Core;
    using Microsoft.Extensions.Configuration;

    public class DDBRepository : IBlobRepository
    {
        private readonly IAmazonDynamoDB ddbClient;
        private readonly string tableName;

        public DDBRepository(DDBSettings settings)
        {
            this.tableName = settings.TableName;
            this.ddbClient = new AmazonDynamoDBClient(new AmazonDynamoDBConfig()
            {
                ServiceURL = settings.ServiceURL
            });
        }

        public async Task<Blob> SaveAsync(Blob blob, CancellationToken token)
        {
            var req = new PutItemRequest()
            {
                TableName = this.tableName,
                Item = new Dictionary<string, AttributeValue>()
                {
                    { "Id", new AttributeValue() { S = blob.Id } },
                    { "Name", new AttributeValue() { S = blob.Name } },
                    { "Size", new AttributeValue() { N = blob.Size.ToString() } },
                    { "BlobType", new AttributeValue() { S = blob.BlobType } },
                    { "Extension", new AttributeValue() { S = blob.Extension } },
                    { "URL", new AttributeValue() { S = blob.URL } },
                    { "CreateTime", new AttributeValue() { S = blob.CreateTime.ToString("o") } },
                    { "UpdateTime", new AttributeValue() { S = blob.UpdateTime.ToString("o") } },
                }
            };

            await this.ddbClient.PutItemAsync(req, token);
            return blob;
        }

        public async Task RemoveAsync(string id, CancellationToken token)
        {
            var req = new DeleteItemRequest()
            {
                TableName = this.tableName,
                Key = new Dictionary<string, AttributeValue>() {{"Id", new AttributeValue() { S = id }}}
            };
            await this.ddbClient.DeleteItemAsync(req, token);
        }

        public async Task<Blob> GetByIdAsync(string id, CancellationToken token)
        {
            var req = new GetItemRequest()
            {
                TableName = this.tableName,
                Key = new Dictionary<string, AttributeValue>() {{"Id", new AttributeValue { S = id }}}
            };
            var resp = await this.ddbClient.GetItemAsync(req, token);
            if (resp.Item.Count == 0)
            {
                return null;
            }

            return new Blob()
            {
                Id = resp.Item["Id"].S,
                Name = resp.Item["Name"].S,
                Size = long.Parse(resp.Item["Size"].N),
                BlobType = resp.Item["BlobType"].S,
                Extension = resp.Item["Extension"].S,
                URL = resp.Item["URL"].S,
                CreateTime = DateTime.Parse(resp.Item["CreateTime"].S),
                UpdateTime = DateTime.Parse(resp.Item["UpdateTime"].S)
            };
        }
    }
}
