# AzureCosmosDbApp

## Project Purpose
This is a basic CRUD application using Azure Cosmos DB NoSQL. It supports standard Create, Read, Update, and Delete operations through a .NET 6 REST API. The project also incorporates bulk insert procedures, pre-trigger (for validating duplicate names), post-trigger (for inserting log information), and user-defined functions (UDF) to calculate days left for tasks. This implementation serves as a practice for working with Cosmos DB in a RESTful architecture.

## Contributors
Darshana Wijesinghe

## App Features
- Supports multiple collections (containers).
- Supports Create, Read, Update, and Delete operations for managing data.
- Supports bulk insert procedure.
- Supports pre-trigger (validates duplicate names) and post-trigger (inserts log info) execution.
- Supports user-defined function (UDF) to calculate days left for tasks.

## Packages
- Microsoft.Azure.Cosmos
- Newtonsoft.Json

## Procedure example
```javascript
/**
 * Performs a bulk insert operation for the given items.
 * 
 * @param {Array} items - An array of objects representing the data to be inserted.
 */
function bulkInsert(items) {

    // Gets the current collection (container)
    var collection = getContext().getCollection();

    // Gets the response object
    var response   = getContext().getResponse();

    // If no items are provided, return 0 and exit
    if (!items || !items.length) throw new Error("No data found to insert.");

    var count = 0;              // Track the number of successfully inserted items
    var total = items.length;   // Total number of items to insert

    // Loops through all items and attempt to insert them into the collection
    for (var i = 0; i < total; i++) {
        var isAccepted = collection.createDocument(
            collection.getSelfLink(),     // Reference to the current collection
            items[i],                     // Item to be inserted
            insertItemCallback            // Callback function for result handling
        );

        // Ensures the create document execution was accepted by Cosmos DB
        if (!isAccepted) throw new Error("Create document execution was not accepted.");
    }

    // Callback function to handle the result of each document insertion
    function insertItemCallback(err, item) {
        if (err) throw err; // If an error occurs, throw an exception

        count++;            // Increment count on successful insertion

        // If all items have been inserted, return the count in response
        if (count >= total) {
            response.setBody(count);
        }
    }
}
```
## Pre-Trigger example
```javascript
/**
 * Checks if there is an existing item with the same name.
 */
function checkSameName(){

    var context    = getContext();               // Gets the execution context
    var request    = context.getRequest();       // Gets the request object
    var collection = context.getCollection();    // Gets the document collection
    var document   = request.getBody();          // Gets the document being inserted

    // Defines the query to check for an existing document with the same email
    var query = {
        query     : "SELECT * FROM c WHERE c.Assignee = @assignee",
        parameters: [{ name: "@assignee", value: document.Assignee }]
    };

    // Executes the query to check for existing documents
    var isAccepted = collection.queryDocuments(
        collection.getSelfLink(), // Gets the collection link
        query,                    // Query definition
        function (err, documents) {
            if (err) throw new Error("Error checking for existing document: " + err.message);
            
            // If a document with the same email exists, prevent insertion
            if (documents.length > 0) throw new Error("A document with the same assignee already exists.");
        }
    );

    // Ensures the query execution was accepted by Cosmos DB
    if (!isAccepted) throw new Error("Query execution was not accepted.");
}
```
## Post-Trigger example
```javascript
/**
 * Inserts log info after adding the data.
 */
function insertLog(){

    var context     = getContext();             // Gets execution context
    var response    = context.getResponse();    // Gets response object
    var collection  = context.getCollection();  // Gets collection object
    var createdItem = response.getBody();       // Gets inserted document

    // Creates a log entry
    var logEntry = {
        action      : "INSERT",
        documentId  : createdItem.id,
        PartitionKey: createdItem.PartitionKey,
        timestamp   : new Date().toISOString()
    };

    // Inserts log entry into 'logs' collection
    var isAccepted = collection.createDocument(
        collection.getSelfLink(),                   // Gets the collection link
        logEntry,                                   // Log entry
        { PartitionKey: createdItem.PartitionKey }, // Partition key
        function (err, doc) {
            if (err) throw new Error("Log insertion failed: " + err.message);
        }
    );

    // Ensures log entry creation was accepted
    if (!isAccepted) throw new Error("Log insert request was not accepted.");
}
```
## User Defined Function (UDF) example
```javascript
/**
 * Calculates the number of days left until the target date.
 *
 * @param {Date} targetDate - The target date to calculate the days left.
 * @returns {number} The number of days left. Returns a negative number if the target date has passed.
 */
function getDaysLeft(targetDate) {
    
    // Gets the current date
    const currentDate    = new Date();
    
    // Parses the target date (assuming targetDate is in YYYY-MM-DD format)
    const target         = new Date(targetDate);
    
    // Calculates the difference in time
    const timeDifference = target - currentDate;
    
    // Converts time difference to days
    const daysLeft       = Math.ceil(timeDifference / (1000 * 3600 * 24)); // 1000 ms * 3600 seconds * 24 hours
    
    // Returns days
    return daysLeft;
}
```

## Usage
```json5
// Creates a new task

POST /api/cosmos/create-record?containerName={containerName}

// Request Body

{
  "TaskName": "The first task for the cosmos db.",
  "Assignee": "Nadeesha",
  "Deadline": "2025-02-10" 
}
```
```json5
// Bulk insert

POST /api/cosmos/bulk-insert?containerName={containerName}

// Request Body

[
  {
    "TaskName": "Task 1.",
    "Assignee": "Nadeesha",
    "Deadline": "2025-02-10"
  },
  {
    "TaskName": "Task 2.",
    "Assignee": "Nadeesha",
    "Deadline": "2025-02-11"
  },
  ...
]
```
```json5
// Gets a specific task

GET /api/cosmos/get-task?id={id}&containerName={containerName}
```
```json5
// Gets all tasks

GET /api/cosmos/get-tasks?containerName={containerName}
```
```json5
// Gets remaining days of the tasks

GET /api/cosmos/get-remaining-days?containerName={containerName}
```
```json5
// Updates an existing task

PUT /api/cosmos/update-task?id={id}&containerName={containerName}

// Request Body

{
  "TaskName": "The first task for the cosmos db.",
  "Assignee": "Harshani",
  "Deadline": "2025-02-10" 
}
```
```json5
// Deletes an existing task

DELETE /api/cosmos/delete-task?id={id}&containerName={containerName}
```
## Support
Darshana Wijesinghe  
Email address - [dar.mail.work@gmail.com](mailto:dar.mail.work@gmail.com)  
Linkedin - [darwijesinghe](https://www.linkedin.com/in/darwijesinghe/)  
GitHub - [darwijesinghe](https://github.com/darwijesinghe)

## License
This project is licensed under the terms of the **MIT** license.