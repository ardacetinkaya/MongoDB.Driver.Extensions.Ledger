using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.Extensions.Ledger;

internal class LogRecord<T>
{
    [BsonId]
    public string Id { get; set; }

    public T Data { get; set; }

    public Metadata Metadata { get; set; }
}

internal class Metadata
{
    public string OriginalId { get; set; }

    public string Hash { get; set; }

#nullable enable
    public string? PreviousHash { get; set; }
#nullable disable

    public string Operation { get; set; }

    public int Version { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
