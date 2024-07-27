# MongoDB.Driver.Extensions.Ledger

**MongoDB.Driver.Extensions.Ledger** is a .NET library that extends the **MongoDB** driver to include **ledger** functionality, allowing you to maintain a "audit or history" log of document operations (insert, update, delete) within a MongoDB collection. All document operations are stored in append-only mode so that any change can be preserved.

Once the document has been stored in a ledger, it is aimed to not be changed later. Within this extension SHA256 hashed document is preserved within log collection so that if an unauthorized change is made to the document, the change can be determined.

The main motivation for this repository is to be able to use ledger approach within MongoDB for .NET workloads. It was inspired by [this proof-of-concept _python_ implemention](https://github.com/mongodb-labs/ledger) by MongoDB-Labs. 


### Installation


### Usage and How It Works

First, ensure you have the necessary using directives:

```csharp
using MongoDB.Driver;
using MongoDB.Driver.Extensions.Ledger;
```

#### Insert a document

```csharp
var collection = database.GetCollection<MyDocument>("myCollection");
var document = new MyDocument { /* Initialize your document */ };
await collection.InsertOneAsLedger(document);
```

- `InsertOneAsLedger`
    - Inserts the document into the specified collection.
    - Logs the operation in a audit or history collection with a unique ID, SHA256 hash of the document, and metadata.


#### Replace a document

```csharp
var collection = database.GetCollection<MyDocument>("myCollection");
var document = new MyDocument { /* Initialize your document */ };
var filter = Builders<MyDocument>.Filter.Eq(doc => doc.Id, document.Id);
await collection.ReplaceOneAsLedger(document, filter);

```

- `ReplaceOneAsLedger`
    - Replaces the document in the specified collection based on the provided filter.
    - Logs the operation in a audit or history collection with a unique ID, SHA256 hash of the document, and metadata, including the previous hash and versioning.


#### Delete a document

```csharp
var collection = database.GetCollection<MyDocument>("myCollection");
var filter = Builders<MyDocument>.Filter.Eq(doc => doc.Id, documentId);
await collection.DeleteOneAsLedger(filter);
```

- `DeleteOneAsLedger`
    - Deletes the document from the specified collection based on the provided filter.
    - Logs the operation in a audit or history collection with a unique ID, SHA256 hash of the document, and metadata, including the previous hash and versioning.

### Log Document Model

Log document has the following model to have audit or history log in the collection.

```csharp
internal class LogRecord<T>
{
    [BsonId]
    public string Id { get; set; }

    public T Data { get; set; }

    public Metadata Metadata { get; set; }
}
```

`Metadata` contains the meta information for the data. Hashed data and some reference properties are defined in metadata. Also every metadata has a version number and timestamp for changes.

```csharp
internal class Metadata
{
    public string OriginalId { get; set; }

    public string Hash { get; set; }

    public string? PreviousHash { get; set; }

    public string Operation { get; set; }

    public int Version { get; set; }

    public DateTimeOffset Timestamp { get; set; }
}
```

### Contributing

Contributions are more than welcome! Please submit a pull request or create an issue to discuss your ideas or feedbacks.

### License
This project is licensed under the MIT License.

