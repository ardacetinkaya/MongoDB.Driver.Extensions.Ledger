# MongoDB Ledger Demo

This is a simple demo showcasing how to use the **MongoDB.Driver.Extensions.Ledger**  library to insert and update documents with ledger functionality in MongoDB.

### Steps

1. Insert a new expense:
    - Creates an Expense object.
    - Inserts the expense into the Expenses collection and add log data in ledger.
2. Update the expense:
    - Updates the Status of the expense.
    - Replaces the document in the Expenses collection and add log data also in ledger.

### Running the Demo
1. Clone the repository.
2. Ensure you have MongoDB running.
3. Update the MongoDB connection string in the MongoClient initialization.
4. Run the demo:

```bash
dotnet run
```