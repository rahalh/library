namespace Blob.API.Config
{
    using System;

    public class DDBSettings
    {
        public string TableName { get; }
        public string ServiceURL { get; }

        public DDBSettings(string tableName, string serviceURL)
        {
            this.TableName = string.IsNullOrEmpty(tableName)
                ? throw new ArgumentNullException($"{nameof(TableName)} is missing")
                : tableName;
            this.ServiceURL = serviceURL;
        }
    }
}
