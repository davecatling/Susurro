using Azure;
using Azure.Data.Tables;
using System;

namespace SusurroFunctions.Model
{
    class MessageEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string To { get; set; }
    }
}
