using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;

namespace MongoDB.Driver.Extensions.Ledger;

public static class LedgerExtensions
{
    private static readonly string LOG_COLLECTION_PREFIX = "_audit_log";

    /// <summary>
    /// Inserts a document into the collection and insert a log in the log collection
    /// </summary>
    /// <typeparam name="T">Type for document</typeparam>
    /// <param name="collection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static async Task InsertOneAsLedger<T>(this IMongoCollection<T> collection, T document)
    {
        // Get the database and log collection
        var db = collection.Database.Client.GetDatabase(collection.Database.DatabaseNamespace.DatabaseName);
        var logCollection = db.GetCollection<LogRecord<T>>($"{collection.CollectionNamespace.CollectionName}{LOG_COLLECTION_PREFIX}");

        // Generate a random document ID for the log record
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var documentId = Convert.ToBase64String(randomBytes);

        // Create a SHA256 hash of the entity's JSON representation
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(document.ToJson()));

        // Get the original ID of the entity
        var originalId = document.ToBsonDocument()["_id"].ToString();
        var logRecord = new LogRecord<T>
        {
            Id = documentId,
            Data = document,
            Metadata = new Metadata
            {
                Hash = BitConverter.ToString(hash).Replace("-", "").ToLower(),
                PreviousHash = string.Empty,
                Version = 0,
                Operation = "INSERT",
                OriginalId = originalId,
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        // Start a client session for the transaction
        using var session = await collection.Database.Client.StartSessionAsync();
        try
        {
            session.StartTransaction();

            // Insert the document into the collection
            await collection.InsertOneAsync(session, document);

            // Insert the log record
            await logCollection.InsertOneAsync(session, logRecord);

            session.CommitTransaction();
        }
        catch (Exception)
        {
            // Abort the transaction if an error occurs
            session.AbortTransaction();
            throw;
        }
    }

    /// <summary>
    /// Replaces a document in the collection and insert a log in the log collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="document"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static async Task ReplaceOneAsLedger<T>(this IMongoCollection<T> collection, T document, FilterDefinition<T> filter)
    {
        // Get the database and log collection
        var db = collection.Database.Client.GetDatabase(collection.Database.DatabaseNamespace.DatabaseName);
        var logCollection = db.GetCollection<LogRecord<T>>($"{collection.CollectionNamespace.CollectionName}{LOG_COLLECTION_PREFIX}");

        // Generate a random document ID for the log record
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var documentId = Convert.ToBase64String(randomBytes);

        // Create a SHA256 hash of the entity's JSON representation
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(document.ToJson()));

        // Get the original ID of the entity
        var originalId = document.ToBsonDocument()["_id"].ToString();
        var logRecord = new LogRecord<T>
        {
            Id = documentId,
            Data = document,
            Metadata = new Metadata
            {
                Hash = BitConverter.ToString(hash).Replace("-", "").ToLower(),
                Operation = "UPDATE",
                OriginalId = originalId,
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        // Start a client session for the transaction
        using var session = await collection.Database.Client.StartSessionAsync();
        try
        {
            session.StartTransaction();

            // Replace the document in the collection
            await collection.ReplaceOneAsync(session, filter, document);

            // Find the most recent log record for the document
            var logRecordFilter = Builders<LogRecord<T>>.Filter.Eq(p => p.Metadata.OriginalId, originalId);
            var sort = Builders<LogRecord<T>>.Sort.Descending(doc => doc.Metadata.Timestamp);
            var existinglog = await logCollection
                .Find(logRecordFilter)
                .Sort(sort)
                .FirstOrDefaultAsync();

            ArgumentNullException.ThrowIfNull(existinglog, nameof(existinglog));

            // Update the log record with the previous hash and increment the version
            logRecord.Metadata.PreviousHash = existinglog.Metadata.Hash;
            logRecord.Metadata.Version = existinglog.Metadata.Version + 1;

            // Insert the log record
            await logCollection.InsertOneAsync(session, logRecord);

            session.CommitTransaction();
        }
        catch (Exception)
        {
            // Abort the transaction if an error occurs
            session.AbortTransaction();
            throw;
        }
    }

    /// <summary>
    /// Deletes a document from the collection and insert a log in the log collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static async Task DeleteOneAsLedger<T>(this IMongoCollection<T> collection, FilterDefinition<T> filter)
    {
        // Get the database and log collection
        var db = collection.Database.Client.GetDatabase(collection.Database.DatabaseNamespace.DatabaseName);
        var logCollection = db.GetCollection<LogRecord<T>>($"{collection.CollectionNamespace.CollectionName}{LOG_COLLECTION_PREFIX}");

        // Generate a random document ID for the log record
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var documentId = Convert.ToBase64String(randomBytes);

        var logRecord = new LogRecord<T>
        {
            Id = documentId,
            Data = default,
            Metadata = new Metadata
            {
                Operation = "DELETE",
                Timestamp = DateTimeOffset.UtcNow
            }
        };

        // Start a client session for the transaction
        using var session = await collection.Database.Client.StartSessionAsync();
        try
        {
            session.StartTransaction();

            // Find and delete the document from the collection
            var existingDocument = collection.Find(session, filter).FirstOrDefault();
            ArgumentNullException.ThrowIfNull(existingDocument, nameof(existingDocument));

            // Delete the document from the collection
            await collection.DeleteOneAsync(session, filter);

            // Get the original ID of the document
            var originalId = existingDocument.ToBsonDocument()["_id"].ToString();

            // Create a SHA256 hash instance
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(existingDocument.ToJson()));

            // Find the most recent log record for the document
            var logRecordFilter = Builders<LogRecord<T>>.Filter.Eq(p => p.Metadata.OriginalId, originalId);
            var sort = Builders<LogRecord<T>>.Sort.Descending(doc => doc.Metadata.Timestamp);
            var existinglog = await logCollection
                .Find(logRecordFilter)
                .Sort(sort)
                .FirstOrDefaultAsync();

            ArgumentNullException.ThrowIfNull(existinglog, nameof(existinglog));

            // Update the log record with the previous hash, increment the version,add the original ID and data
            logRecord.Metadata.PreviousHash = existinglog.Metadata.Hash;
            logRecord.Metadata.Version = existinglog.Metadata.Version + 1;
            logRecord.Metadata.OriginalId = originalId;
            logRecord.Metadata.Hash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            logRecord.Data = existingDocument;

            // Insert the log record
            await logCollection.InsertOneAsync(session, logRecord);

            session.CommitTransaction();
        }
        catch (Exception)
        {
            // Abort the transaction if an error occurs
            session.AbortTransaction();
            throw;
        }
    }

    /// <summary>
    /// Verifies if an item's history is unchanged by comparing the current hash with the stored hash
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="document"></param>
    /// <returns></returns>
    public static async Task<bool> VerifyOneFromLedger<T>(this IMongoCollection<T> collection, T document)
    {
        // Get the database and log collection
        var db = collection.Database.Client.GetDatabase(collection.Database.DatabaseNamespace.DatabaseName);
        var logCollection = db.GetCollection<LogRecord<T>>($"{collection.CollectionNamespace.CollectionName}{LOG_COLLECTION_PREFIX}");

        // Get the original ID of the entity
        var originalId = document.ToBsonDocument()["_id"].ToString();

        // Retrieve the log records for the document
        var logRecordFilter = Builders<LogRecord<T>>.Filter.Eq(p => p.Metadata.OriginalId, originalId);
        var sort = Builders<LogRecord<T>>.Sort.Ascending(doc => doc.Metadata.Version);
        var logRecords = await logCollection
            .Find(logRecordFilter)
            .Sort(sort)
            .ToListAsync();

        // Check if there is any missing version in the log history
        for (int i = 0; i < logRecords.Count; i++)
        {
            var isHashValid = i > 0 && logRecords[i].Metadata.PreviousHash != logRecords[i - 1].Metadata.Hash;
            if (logRecords[i].Metadata.Version != i || isHashValid)
            {
                return false;
            }
        }

        return true;
    }
}

