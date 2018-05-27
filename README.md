# Pillowsharp
[![NuGet version (PillowSharp)](https://img.shields.io/nuget/v/PillowSharp.svg?style=flat-square)](https://https://www.nuget.org/packages/PillowSharp/)
[![Build Status](https://travis-ci.org/Dev-Owl/pillowsharp.svg?branch=master)](https://travis-ci.org/Dev-Owl/pillowsharp)

## Content
* Description
* Setup
* Install
* Planned features
* Basic usage
* How to...
* By example:
  * Create database
  * Create document
  * Get documents
  * Delete documents
  * Attachments
  * Design Documents
  * UUID

## Description
Pillowsharp is a library designed to be used as a client for [Apache CouchDB](https://github.com/apache/couchdb) written in .NET Core. The goal is to offer something that can be used out of the box without the need of configuration or a lot of code. Pillowsharp makes use of the `async` and `await` feature from .NET that allows none blocking requests to the DB.

## Setup
To build the library and to run the unit tests, please follow the instructions below:
1. Install .Net Core as described here [.NET Core](https://www.microsoft.com/net/download/core)
2. Install [Apache CouchDB](https://github.com/apache/couchdb)
3. Add an admin for couch using `curl -X PUT localhost:5984/_config/admins/admin -d '"admin"'`
4. Clone or download this repository 
5. Switch into the cloned folder and execute build.bat (Windows) or build.sh
6. After the script is completed, you are ready to use the `library/nuget`

**Please note:** If you are using CouchDB 2.X there are some changes to the configure a development enviorment you can use the dev_couch2_setup.sh/bat file. It will configure a single node instance with the default admin used also in the unit tests.

In case you encounter issues in step 5, please check the CouchSettings.cs file if all configurations are correct for your instance of CouchDB. If you still have issues or questions, please open an issue or contact me.

## Install
Either you can clone the repository and run the related build script (please see the Setup section) or download a build from nuget.org

## Planned features
The following things will be added, or at least I would like to add them:

* Replication end point support
* Replication status check
* Replication conflict support
* Backup and restore of a db (will be in a different tool)

# Basic usage
All communication with CouchDB uses the PillowClient class, you can generate a new instance like this:
```cs
var client = = new PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984")); 
```
The above example will use anonymous communication with CouchDB (no user required,default CouchDB setup). To use PillowSharp with a login the following client setup would be correct:
```cs
var client = new PillowClient(new BasicCouchDBServer("http://127.0.0.1:5984",new CouchLoginData("admin","admin"),ELoginTypes.TokenLogin));
```
**Please note:** You need to set the Login type as described [here](http://docs.couchdb.org/en/2.1.1/api/server/authn.html?highlight=authentication)

Example usage for some Pillow functions are located in the [example project in the repository](https://github.com/Dev-Owl/pillowsharp/blob/master/src/example/Program.cs).

## Model data
Pillow allows you to use any C# class as a model to send and receive from CouchDB. If you would like to make your life a little bit easier inerheit from `CouchDocument`. This allows all the communication functions of Pillow to set the ID, Revision and Delete flag automatically.
```cs
public class TestDocument : CouchDocument
{
    public int Fu { get; set; }
    public string Bar { get; set; }
}
```
If the above module is used for example in an update call, Pillow will update the revsion number inside the object.

All calls accept a parameter to define the database you want to talk to. If you would like to avoid setting this on every call you can also set this as a parameter for your client:
```cs
client.ForcedDatabaseName = "pillow";
```
Another way is to set the database for each model:
```cs
[DBName("pillow")]
public class TestDocument : CouchDocument
{
    public int Fu { get; set; }
    public string Bar { get; set; }
}
```
It is fine to use all three ways to set the database, Pillow will use them in the following order:
1. Caller Parameter
2. Class DB, see ForcedDatabaseName
3. DBNameAttribute of the current document

# Examples
Please also see the [example project](https://github.com/Dev-Owl/pillowsharp/blob/master/src/example/Program.cs).
## Create a database
```cs
if(!await client.DbExists("pillow"))
  {
    if( await client.CreateNewDatabase("pillow"))
      Console.WriteLine("Database pillow created");
  }
```
## Create a document

The example below assumes that the ```Person``` class inerheits ```CouchDocument``` and uses the ```[DBName("pillow")]``` attribute. It would also be possible to check the new revision and id in the result object from the call.

```cs
person = new Person(){Name="Dev",LastName="Owl",Role="Developer" };
var result = await client.CreateANewDocument(person);
if(result.Ok)
{
  Console.WriteLine($"Document created with id:{person.ID} and Rev:{person.Rev}");
}
```
## Get documents

Get a single document by id and optional by revision:
```cs
var result = await client.GetDocument<Person>(ID="developer");
```

Use the [all documents endpoint](http://docs.couchdb.org/en/2.1.1/api/database/bulk-api.html#db-all-docs) in CouchDB:
```cs
await client.GetAllDocuments(Person.GetType());
```
There are some more ways you can use PillowClient to get a list of documents:
* GetDocuments<T>(string RequestURL)

## Delete documents
The example below assumes that the ```Person``` class inerheits ```CouchDocument``` and uses the ```[DBName("pillow")]``` attribute. Important for this call is to send an existing ID to CouchDB.
```cs
var result = await client.DeleteDocument(person);
```
## Attachments
The examples below assumes that the ```Person``` class inerheits ```CouchDocument``` and uses the ```[DBName("pillow")]``` attribute. 

### Upload
Just provide the path to the file you would like to attach to the document.
```cs
var result =  await client.AddAttachment(person,"fav.image","sleepy_owl.JPG");
```

### Download
The result will be a byte[]
```cs
var result =  await client.GetAttachement(person,"fav.image");
```
### Delete

```cs
var result =  await client.DeleteAttachment(person,"fav.image");
```

## Design Documents

### Create a document
PillowSharp supports:
* Filters
* Lists
* Views 
* Shows 
* Updates

```cs
var designDoc = new CouchDesignDocument();
designDoc.ID="testDesignDoc";
designDoc.AddView("test","function (doc) {emit(doc._id, 1);}");
designDoc.AddView("testReduce","function (doc) {emit(doc._id, 1);}","_count");
var result = await client.UpsertDesignDocument(designDoc);
```

### Get a design document
```cs
var dbDocument = await client.GetDesignDocument(ID);
```

### Run a view
```cs
var result = await client.GetView<dynamic>(ID,"viewname",new[] {new KeyValuePair<string, object>("reduce","false")});
```

## UUID
You can ask CouchDB to generate IDs for you, the PillowSharp Client can auto generate IDs for new documents. All can be configured inside ```PillowClientSettings``` default settings are:
```cs
public class PillowClientSettings
{
        public bool AutoGenerateID { get; set; } = true;

        public bool UseCouchUUID { get; set; } = false;

        public bool IgnoreJSONNull { get; set; } = true;
}
```
To generate CouchDB UUID's you can use the follwing call:
```cs
var uuidResponse = await client.GetManyUUIDs(AmountOfUUIDs:10);
foreach(var id in uuidResponse.UUIDS){
  Console.WriteLine(id);
}
```
