# CushionDataGenerator Tool

## Overview
The CushionDataGenerator tool is a command-line application designed to generate documents based on a JSON template. It supports various options to customize the document generation process, including specifying the number of documents, batch size, and user content file.

## Prerequisites
- .NET Core SDK
- CouchDB instance (if you plan to use CouchDB)

## Installation
1. Clone the repository:
    ```sh
    git clone https://github.com/your-repo/CushionDataGenerator.git
    ```
2. Navigate to the project directory:
    ```sh
    cd CushionDataGenerator
    ```
3. Build the project:
    ```sh
    dotnet build
    ```

## Usage
Run the tool using the following command:
```sh
dotnet run -- [options]
```

# Options
- `-d, --database` (required): CouchDB database name.
- `-n, --username` (required): CouchDB username.
- `-p, --password` (required): CouchDB password.
- `-t, --template` (required): Path to the JSON template file.
- `-c, --count` (optional, default: 1): Number of documents to generate.
- `-b, --batchsize` (optional, default: 1): Batch size for document generation.
- `-u, --usercontent` (optional): Path to a file containing user content.

# Example
## Explanation
- `-d mydatabase`: Specifies the CouchDB database name.
- `-n myusername`: Specifies the CouchDB username.
- `-p mypassword`: Specifies the CouchDB password.
- `-t template.json`: Specifies the path to the JSON template file.
- `-c 10`: Specifies the number of documents to generate (default is 1).
- `-b 2`: Specifies the batch size for document generation (default is 1).
- `-u usercontent.json`: Specifies the path to a file containing user content (optional).

## Template file

The below is an example for a more complex template file:
```json
{
    "_id": "partion:list(names)",
    "name": "string:list(names)",
    "age": "int:range(18, 65)",
    "balance": "decimal:range(18.5, 122.5)",
    "created_at": "datetime:range(2020-01-01, 2021-01-01)",
    "address": {
        "street": "string",
        "zipcode": "int:range(10000, 99999)",
        "coordinates": {
            "latitude": "decimal",
            "longitude": "decimal"
        }
    }
}
```




# Contributing
Feel free to submit issues or pull requests if you find any bugs or have suggestions for improvements.

# License
This project is licensed under the MIT License. See the LICENSE file for details.