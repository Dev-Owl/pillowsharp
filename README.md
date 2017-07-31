# Pillowsharp

[![Build Status](https://travis-ci.org/Dev-Owl/pillowsharp.svg?branch=master)](https://travis-ci.org/Dev-Owl/pillowsharp)

## Content
* Description
* Setup
* Install
* Basic usage
* How to...
* By example:
  * Create database
  * Create document
  * Get documents
  * Delete documents
  * Attachments
  * Views
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

In case you encounter issues in step 5, please check the CouchSettings.cs file if all configurations are correct for your instance of CouchDB. If you still have issues or questions, please open an issue or contact me.

## Install
Wait until the library is published on the NuGet List or a release is tagged here.

# Work in progress, stay tuned!
