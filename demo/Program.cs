using MongoDB.Driver;
using MongoDB.Driver.Extensions.Ledger;

Console.WriteLine("Hello, MongoDB Ledger!");

var client = new MongoClient("__CONNECTION_STRING__");
var database = client.GetDatabase("Budget");
var collection = database.GetCollection<Expense>("Expenses");


var expense = new Expense
{
    Id = "102",
    Amount = 50.00m,
    Category = "Food",
    Status = "Requested",
    Date = DateTimeOffset.UtcNow
};
await collection.InsertOneAsLedger(expense);
Console.WriteLine("Expense is inserted.");

expense.Status = "Approved";

var filter = Builders<Expense>.Filter.Eq(doc => doc.Id, expense.Id);
await collection.ReplaceOneAsLedger(expense, filter);
Console.WriteLine("Expense is updated.");

expense.Status = "Spent";

var filter = Builders<Expense>.Filter.Eq(doc => doc.Id, expense.Id);
await collection.ReplaceOneAsLedger(expense, filter);
Console.WriteLine("Expense is updated.");

expense.Status = "Reimbursed";

var filter = Builders<Expense>.Filter.Eq(doc => doc.Id, expense.Id);
await collection.ReplaceOneAsLedger(expense, filter);
Console.WriteLine("Expense is updated.");