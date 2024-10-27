using Xunit;
using CushionData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace CushionData.Tests
{
    public class DataGeneratorTestsTest
    {
        [Fact]
        public async Task BasicTemplateGenerationTest()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = true;
            generator.DocumentCallback = doc => documents.Add(doc);
            int numberOfDocuments = 10;
            var documentTemplateJson = @"
            {
                ""_id"": ""uuid"",
                ""flag"": ""bool"",
                ""name"": ""string"",
                ""type"":""user"",
                ""guid"":""uuid"",
                ""age"": ""int"",
                ""balance"": ""decimal"",
                ""created_at"": ""datetime"",
                ""address"": {
                    ""street"": ""string"",
                    ""zipcode"": ""int"",
                    ""coordinates"": {
                        ""latitude"": ""decimal"",
                        ""longitude"": ""decimal""
                    }
                }
            }
            ";
            // Act
            await generator.GenerateDocumentsAsync(documentTemplateJson, numberOfDocuments);

            // Assert
            Assert.Equal(numberOfDocuments, documents.Count);
            foreach (var doc in documents)
            {
                Assert.NotNull(doc);
                Assert.True(doc.AdditionalProperties.ContainsKey("name"));
                Assert.IsType<string>(doc.AdditionalProperties["name"]);

                // Check if the flag field is a boolean
                Assert.True(doc.AdditionalProperties.ContainsKey("flag"));
                Assert.IsType<bool>(doc.AdditionalProperties["flag"]);

                // ensure the ID field is a valid UUID
                Assert.True(Guid.TryParse((string)doc.ID, out _));

                // Ensure the type field is set to "user"
                Assert.True(doc.AdditionalProperties.ContainsKey("type"));
                Assert.IsType<string>(doc.AdditionalProperties["type"]);
                Assert.Equal("user", doc.AdditionalProperties["type"]);

                Assert.True(doc.AdditionalProperties.ContainsKey("age"));
                Assert.IsType<int>(doc.AdditionalProperties["age"]);

                Assert.True(doc.AdditionalProperties.ContainsKey("balance"));
                Assert.IsType<decimal>(doc.AdditionalProperties["balance"]);

                Assert.True(doc.AdditionalProperties.ContainsKey("created_at"));
                Assert.IsType<DateTime>(doc.AdditionalProperties["created_at"]);

                Assert.True(doc.AdditionalProperties.ContainsKey("address"));
                var address = doc.AdditionalProperties["address"] as Dictionary<string, object>;
                Assert.NotNull(address);

                Assert.True(address.ContainsKey("street"));
                Assert.IsType<string>(address["street"]);

                Assert.True(address.ContainsKey("zipcode"));
                Assert.IsType<int>(address["zipcode"]);

                Assert.True(address.ContainsKey("coordinates"));
                var coordinates = address["coordinates"] as Dictionary<string, object>;
                Assert.NotNull(coordinates);

                Assert.True(coordinates.ContainsKey("latitude"));
                Assert.IsType<decimal>(coordinates["latitude"]);

                Assert.True(coordinates.ContainsKey("longitude"));
                Assert.IsType<decimal>(coordinates["longitude"]);

                // ensure the guid field is a valid UUID
                Assert.True(doc.AdditionalProperties.ContainsKey("guid"));
                Assert.IsType<string>(doc.AdditionalProperties["guid"]);
                Assert.True(Guid.TryParse((string)doc.AdditionalProperties["guid"], out _));
            }
        }

        [Fact]
        public async Task ComplexTemplateGenerationTest()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = true;
            generator.DocumentCallback = doc => documents.Add(doc);
            generator.UserContent = "{\"names\": [\"Alice\", \"Bob\", \"Charlie\"]}";
            int numberOfDocuments = 10;
            var documentTemplateJson = @"
            {
                ""_id"": ""partion:list(names)"",
                ""name"": ""string:list(names)"",
                ""age"": ""int:range(18, 65)"",
                ""balance"": ""decimal:range(18.5, 122.5)"",
                ""created_at"": ""datetime:range(2020-01-01, 2021-01-01)"",
                ""address"": {
                    ""street"": ""string"",
                    ""zipcode"": ""int:range(10000, 99999)"",
                    ""coordinates"": {
                        ""latitude"": ""decimal"",
                        ""longitude"": ""decimal""
                    }
                }
            }
            ";
            // Act
            await generator.GenerateDocumentsAsync(documentTemplateJson, numberOfDocuments);

            // Assert
            Assert.Equal(numberOfDocuments, documents.Count);
            foreach (var doc in documents)
            {
                Assert.NotNull(doc);
                Assert.True(doc.AdditionalProperties.ContainsKey("name"));
                Assert.IsType<string>(doc.AdditionalProperties["name"]);
                // ensure the name is from the given list
                Assert.Contains(doc.AdditionalProperties["name"], new object[] { "Alice", "Bob", "Charlie" });

                // Ensue the ID field is a valid UUID after splitting the partition
                Assert.True(doc.ID.Contains(":"));
                var parts = doc.ID.Split(":");
                Assert.True(Guid.TryParse(parts[1], out _));
                // Ensure the partition is from the given list
                Assert.Contains(parts[0], new object[] { "Alice", "Bob", "Charlie" });

                Assert.True(doc.AdditionalProperties.ContainsKey("age"));
                Assert.IsType<int>(doc.AdditionalProperties["age"]);
                Assert.InRange((int)doc.AdditionalProperties["age"], 18, 65);

                Assert.True(doc.AdditionalProperties.ContainsKey("balance"));
                Assert.IsType<decimal>(doc.AdditionalProperties["balance"]);
                Assert.InRange((decimal)doc.AdditionalProperties["balance"], 18.5m, 122.5m);

                Assert.True(doc.AdditionalProperties.ContainsKey("created_at"));
                Assert.IsType<DateTime>(doc.AdditionalProperties["created_at"]);
                Assert.InRange((DateTime)doc.AdditionalProperties["created_at"], new DateTime(2020, 1, 1), new DateTime(2021, 1, 1));

                Assert.True(doc.AdditionalProperties.ContainsKey("address"));
                var address = doc.AdditionalProperties["address"] as Dictionary<string, object>;
                Assert.NotNull(address);

                Assert.True(address.ContainsKey("street"));
                Assert.IsType<string>(address["street"]);

                Assert.True(address.ContainsKey("zipcode"));
                Assert.IsType<int>(address["zipcode"]);
                Assert.InRange((int)address["zipcode"], 10000, 99999);

                Assert.True(address.ContainsKey("coordinates"));
                var coordinates = address["coordinates"] as Dictionary<string, object>;
                Assert.NotNull(coordinates);

                Assert.True(coordinates.ContainsKey("latitude"));
                Assert.IsType<decimal>(coordinates["latitude"]);

                Assert.True(coordinates.ContainsKey("longitude"));
                Assert.IsType<decimal>(coordinates["longitude"]);
            }
        }

        [Fact]
        public async Task SimpleArrayFieldTypeGenerationTest()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = true;
            generator.DocumentCallback = doc => documents.Add(doc);

            // Act
            var template = @"{""tags"": ""array:value(tag1,1,5.5,2024-12-24):range(4,4)""}";
            await generator.GenerateDocumentsAsync(template, 10);

            // Assert
            foreach (var doc in documents)
            {
                Assert.True(doc.AdditionalProperties.ContainsKey("tags"));
                var tags = doc.AdditionalProperties["tags"] as List<Object>;
                Assert.NotNull(tags);
                Assert.InRange(tags.Count, 4, 4);
                // Esnure all tags are from the given list
                foreach (var tag in tags)
                {
                    Assert.Contains(tag, new object[] { "tag1", 1, 5.5m, new DateTime(2024, 12, 24) });
                }
            }
        }

        [Fact]
        public async Task SimpleArrayFieldTypeGenerationTest_NoValuesOrRange()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = true;
            generator.DocumentCallback = doc => documents.Add(doc);

            // Act
            var template = "{\"tags\": \"array\"}";
            await generator.GenerateDocumentsAsync(template, 10);

            // Assert
            foreach (var doc in documents)
            {
                Assert.True(doc.AdditionalProperties.ContainsKey("tags"));
                var tags = doc.AdditionalProperties["tags"] as List<Object>;
                Assert.NotNull(tags);
                Assert.True(tags.Count >= 0); // Ensure that the array is generated, even if empty
            }
        }

        [Fact]
        public async Task ComplexArrayFieldTypeGenerationTest()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = true;
            generator.DocumentCallback = doc => documents.Add(doc);

            // Act
            var template = "{\"tags\": [\"{\\\"a\\\":\\\"string\\\"}:range(5)\"]}";
            await generator.GenerateDocumentsAsync(template, 10);

            // Assert
            foreach (var doc in documents)
            {
                Assert.True(doc.AdditionalProperties.ContainsKey("tags"));
                var tags = doc.AdditionalProperties["tags"] as List<Object>;
                Assert.NotNull(tags);
                Assert.True(tags.Count >= 0); // Ensure that the array is generated, even if empty
                Assert.InRange(tags.Count, 5, 5);
                var first = tags.First();
                Assert.IsType<Dictionary<string, object>>(first);
                var firstDict = first as Dictionary<string, object>;
                Assert.True(firstDict.ContainsKey("a"));
            }
        }


        [Fact]
        public async Task InsertIntoCouchDb()
        {
            // Arrange
            var generator = new CushionDataGenerator("test", "http://localhost:5984", "admin", "admin");
            var documents = new List<DynamicCouchDocument>();
            generator.DryMode = false;
            generator.DocumentCallback = doc => documents.Add(doc);
            generator.UserContent = "{\"names\": [\"Alice\", \"Bob\", \"Charlie\"]}";
            int numberOfDocuments = 10;
            var documentTemplateJson = @"
            {
                ""_id"": ""uuid"",
                ""name"": ""string:list(names)"",
                ""age"": ""int:range(18, 65)"",
                ""balance"": ""decimal:range(18.5, 122.5)"",
                ""created_at"": ""datetime:range(2020-01-01, 2021-01-01)"",
                ""address"": {
                    ""street"": ""string"",
                    ""zipcode"": ""int:range(10000, 99999)"",
                    ""coordinates"": {
                        ""latitude"": ""decimal"",
                        ""longitude"": ""decimal""
                    }
                }
            }
            ";
            // Act
            await generator.GenerateDocumentsAsync(documentTemplateJson, numberOfDocuments);

            /*
            // Assert
            Assert.Equal(numberOfDocuments, documents.Count);
            foreach (var doc in documents)
            {
                Assert.NotNull(doc);
                Assert.True(doc.AdditionalProperties.ContainsKey("name"));
                Assert.IsType<string>(doc.AdditionalProperties["name"]);
                // ensure the name is from the given list
                Assert.Contains(doc.AdditionalProperties["name"], new object[] { "Alice", "Bob", "Charlie" });

                // Ensue the ID field is a valid UUID after splitting the partition
                Assert.True(doc.ID.Contains(":"));
                var parts = doc.ID.Split(":");
                Assert.True(Guid.TryParse(parts[1], out _));
                // Ensure the partition is from the given list
                Assert.Contains(parts[0], new object[] { "Alice", "Bob", "Charlie" });

                Assert.True(doc.AdditionalProperties.ContainsKey("age"));
                Assert.IsType<int>(doc.AdditionalProperties["age"]);
                Assert.InRange((int)doc.AdditionalProperties["age"], 18, 65);

                Assert.True(doc.AdditionalProperties.ContainsKey("balance"));
                Assert.IsType<decimal>(doc.AdditionalProperties["balance"]);
                Assert.InRange((decimal)doc.AdditionalProperties["balance"], 18.5m, 122.5m);

                Assert.True(doc.AdditionalProperties.ContainsKey("created_at"));
                Assert.IsType<DateTime>(doc.AdditionalProperties["created_at"]);
                Assert.InRange((DateTime)doc.AdditionalProperties["created_at"], new DateTime(2020, 1, 1), new DateTime(2021, 1, 1));

                Assert.True(doc.AdditionalProperties.ContainsKey("address"));
                var address = doc.AdditionalProperties["address"] as Dictionary<string, object>;
                Assert.NotNull(address);

                Assert.True(address.ContainsKey("street"));
                Assert.IsType<string>(address["street"]);

                Assert.True(address.ContainsKey("zipcode"));
                Assert.IsType<int>(address["zipcode"]);
                Assert.InRange((int)address["zipcode"], 10000, 99999);

                Assert.True(address.ContainsKey("coordinates"));
                var coordinates = address["coordinates"] as Dictionary<string, object>;
                Assert.NotNull(coordinates);

                Assert.True(coordinates.ContainsKey("latitude"));
                Assert.IsType<decimal>(coordinates["latitude"]);

                Assert.True(coordinates.ContainsKey("longitude"));
                Assert.IsType<decimal>(coordinates["longitude"]);
            }
            */
        }




    }
}