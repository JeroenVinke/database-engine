using DatabaseEngine.Commands;
using DatabaseEngine.Operations;
using DatabaseEngine.Relations;
using System;
using System.Collections.Generic;
using System.IO;

namespace DatabaseEngine
{
    public class Program
    {
        public static string StorageFilePath = $"{Directory.GetCurrentDirectory()}\\data.storage";
        public static RelationManager RelationManager { get; set; }

        unsafe static void Main(string[] args)
        {
            //File.Delete(StorageFilePath);
            StorageFile storageFile = new StorageFile(StorageFilePath);

            RelationManager = new RelationManager(storageFile);
            RelationManager.Initialize();
            CreateProductsTableIfNotExists(RelationManager);
            CreateProducersTableIfNotExists(RelationManager);


            string query = "SELECT products.BuildYear, * FROM products JOIN producers on products.producer = producers.name WHERE producers.Name = \"AMD\" ";
            Console.WriteLine("Query: " + query);
            while (!string.IsNullOrEmpty(query))
            {
                Console.WriteLine("Executing query...");

                List<CustomTuple> result = ExecuteQuery(query);

                if (result.Count > 0)
                {
                    List<string> columnNames = new List<string>();
                    foreach (CustomObject entry in result[0].Entries)
                    {
                        columnNames.Add(entry.AttributeDefinition.Name.ToString());
                    }
                    Console.WriteLine("[" + string.Join("|", columnNames) + "]");
                }

                foreach (CustomTuple tuple in result)
                {
                    List<string> s = new List<string>();

                    foreach (CustomObject entry in tuple.Entries)
                    {
                        s.Add(entry.Value.ToString());
                    }

                    Console.WriteLine(string.Join("|", s));
                }

                Console.WriteLine("Enter new query:");
                query = Console.ReadLine();
            }
        }

        public static List<CustomTuple> ExecuteQuery(string query)
        {
            CommandParser commandParser = new CommandParser(RelationManager);

            Command command = commandParser.Parse(query);

            QueryPlan plan = new QueryPlan(command);

            List<CustomTuple> result = plan.Execute();

            return result;
        }

        private static void CreateProducersTableIfNotExists(RelationManager relationManager)
        {
            if (!relationManager.TableExists("Producers"))
            {
                TableDefinition table = new TableDefinition()
                {
                    Name = "Producers",
                    Id = 2
                };

                table.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "Name", Type = ValueType.String });

                relationManager.CreateTable(table);
                WriteProducers();
            }
        }

        private static void CreateProductsTableIfNotExists(RelationManager relationManager)
        {
            if (!relationManager.TableExists("Products"))
            {
                TableDefinition table = new TableDefinition()
                {
                    Name = "Products",
                    Id = 1
                };

                table.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "BuildYear", Type = ValueType.Integer });
                table.Add(new AttributeDefinition() { Name = "Producer", Type = ValueType.String });
                table.AddIndex(new Index { IsClustered = true, Column = "Id" });
                table.AddIndex(new Index { IsClustered = false, Column = "Producer" });

                relationManager.CreateTable(table);
                WriteProducts();
            }
        }

        private static void WriteProducers()
        {
            ExecuteQuery("INSERT INTO producers VALUES (1, \"Intel\")");
            ExecuteQuery("INSERT INTO producers VALUES (2, \"AMD\")");
        }

        private static void WriteProducts()
        {
            ExecuteQuery("INSERT INTO products VALUES (1, 1994, \"Intel\")");
            ExecuteQuery("INSERT INTO products VALUES (2, 2010, \"AMD\")");
            ExecuteQuery("INSERT INTO products VALUES (4, 2020, \"AMD\")");
            ExecuteQuery("INSERT INTO products VALUES (3, 2015, \"Intel\")");
        }
    }
}
