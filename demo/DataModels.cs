public class Expense
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; }
    public string Status { get; set; }
    public DateTimeOffset Date { get; set; }
}