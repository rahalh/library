namespace Blob.API.Configuration
{
    using System;

    public class DDBSettings
    {
        public string TableName { get; }
        public string ServiceURL { get; }

        public DDBSettings(string tableName, string serviceURL)
        {
            this.TableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException($"{nameof(this.TableName)} is missing")
                : tableName;
            this.ServiceURL = serviceURL;
        }
    }
}
