using System;
using System.Threading.Tasks;
using CommandLine;

class Program
{
    public class Options
    {
        [Option('u', "url", Required = true, HelpText = "CouchDB URL.")]
        public string CouchDbUrl { get; set; }

        [Option('d', "database", Required = true, HelpText = "CouchDB database name.")]
        public string DatabaseName { get; set; }

        [Option('n', "username", Required = true, HelpText = "CouchDB username.")]
        public string Username { get; set; }

        [Option('p', "password", Required = true, HelpText = "CouchDB password.")]
        public string Password { get; set; }

        [Option('t', "template", Required = true, HelpText = "Path to the JSON template file.")]
        public string TemplateFilePath { get; set; }

        [Option('c', "count", Required = false, Default = 1, HelpText = "Number of documents to generate. Default 1")]
        public int DocumentCount { get; set; }

        [Option('b', "batchsize", Required = false, Default = 1, HelpText = "Batch size for document generation. Default 1")]
        public int BatchSize { get; set; }

        [Option('u', "usercontent", Required = false, HelpText = "Path to a file containing user content.")]
        public string UserContentFilePath { get; set; }
    }

    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(async options =>
            {
                // Ensure batch size is not greater than the document count
                var finalBatchSize = Math.Min(options.DocumentCount, options.BatchSize);
                // Ensure document count is at least 1
                var finalDocumentSize = Math.Min(1, options.DocumentCount);

                // Ensure the given template file exists
                if (!System.IO.File.Exists(options.TemplateFilePath))
                {
                    Console.WriteLine($"Template file not found: {options.TemplateFilePath}");
                    Environment.Exit(-1);
                }

                var generator = new CushionDataGenerator(
                    databaseName: options.DatabaseName,
                    couchDbUrl: options.CouchDbUrl,
                    username: options.Username,
                    password: options.Password
                );

                // Read all text from the template file
                var templateFileContent = System.IO.File.ReadAllText(options.TemplateFilePath);
                // Check if user coneent file path is provided and read the content if the file exists
                if (!string.IsNullOrEmpty(options.UserContentFilePath) && System.IO.File.Exists(options.UserContentFilePath))
                {
                    generator.UserContent = System.IO.File.ReadAllText(options.UserContentFilePath);
                }

                await generator.GenerateDocumentsAsync(templateFileContent, finalDocumentSize, finalBatchSize);

                Console.WriteLine("Documents generated successfully!");
                Environment.Exit(0);
            });
    }
}