# MongoDB Ledger Demo

This is a simple demo showcasing how to use the **MongoDB.Driver.Extensions.Ledger**  library to insert and update documents with ledger functionality in MongoDB.

### Steps

1. Insert a new expense:
    - Creates an Expense object.
    - Inserts the expense into the Expenses collection and add log data in ledger.
2. Update(s) the expense:
    - Updates the Status of the expense.
    - Replaces the document in the Expenses collection and add log data also in ledger.

<table>
<tr>
<td> Original Document </td> <td> Document Changes </td>
</tr>
<tr>
<td>

```json
[
  {
    "_id": "102",
    "Amount": "50.00",
    "Category": "Food",
    "Status": "Reimbursed",
    .....
    ]
  }
]
```

</td>
<td>

```json
[
  {
    "_id": "fgLuXBUjVY3U3rwDWUzIBy5c4+k13hcGtXmxh1bf4Ro=",
    "Data": {
      "_id": "102",
      "Amount": "50.00",
      "Category": "Food",
      "Status": "Requested",
      .....
    },
    "Metadata": {
      "OriginalId": "102",
      "Hash": "dbe25eaa8dbca9ad10f734ececf543518ba8c645b561c9b5bee879cf4d995f61",
      "PreviousHash": "",
      "Operation": "INSERT",
      "Version": 0,
      "Timestamp": .....
    }
  },
  {
    "_id": "yequHNiCfl/6YeGT30yteRKKfDWT7Vn5CnrEeV1E3X0=",
    "Data": {
      "_id": "102",
      "Amount": "50.00",
      "Category": "Food",
      "Status": "Approved",
      .....
      ]
    },
    "Metadata": {
      "OriginalId": "102",
      "Hash": "6b0dbc2336cb42d025b190503257937960c52f5f98f406db3266bc0cb8ce7546",
      "PreviousHash": "dbe25eaa8dbca9ad10f734ececf543518ba8c645b561c9b5bee879cf4d995f61",
      "Operation": "UPDATE",
      "Version": 1,
      "Timestamp": .....
      ]
    }
  }
]
```

</td>
</tr>
</table>




### Running the Demo
1. Clone the repository.
2. Ensure you have MongoDB running.
3. Update the MongoDB connection string in the MongoClient initialization.
4. Run the demo:

```bash
dotnet run
```