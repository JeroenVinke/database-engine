using Compiler.LexicalAnalyer;
using NUnit.Framework;
using System.Collections.Generic;

namespace DatabaseEngine.Tests
{
    public class SetTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Intersect()
        {
            new LexicalAnalyzer(LexicalLanguage.GetLanguage(), "\"AMD\"").GetNextToken();

            TableDefinition productsTable = new TableDefinition()
            {
                Name = "Product"
            };
            productsTable.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Model", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Type", Type = ValueType.String });

            Set products1 = new Set(productsTable);
            products1.Add(new object[] { "A", "10001", "PC" });
            products1.Add(new object[] { "B", "10001", "PC" });
            products1.Add(new object[] { "B", "10002", "PC" });
            products1.Add(new object[] { "B", "10002", "Laptop" });

            Set products2 = new Set(productsTable);
            products2.Add(new object[] { "A", "10001", "PC" });
            products2.Add(new object[] { "B", "10002", "Laptop" });
            products2.Add(new object[] { "C", "10003", "Laptop" });

            Set intersection = products1.Intersect(products2);

            Assert.AreEqual(2, intersection.Count());
        }

        [Test]
        public void Union()
        {
            TableDefinition productsTable = new TableDefinition()
            {
                Name = "Product"
            };
            productsTable.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Model", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Type", Type = ValueType.String });

            Set products1 = new Set(productsTable);
            products1.Add(new object[] { "A", "10001", "PC" });
            products1.Add(new object[] { "B", "10001", "PC" });

            Set products2 = new Set(productsTable);
            products2.Add(new object[] { "C", "10003", "Laptop" });

            Set union = products1.Union(products2);

            Assert.AreEqual(3, union.Count());
        }

        [Test]
        public void Project()
        {
            TableDefinition productsTable = new TableDefinition()
            {
                Name = "Product"
            };
            productsTable.Add(new AttributeDefinition() { Name = "Maker", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Model", Type = ValueType.String });
            productsTable.Add(new AttributeDefinition() { Name = "Type", Type = ValueType.String });

            Set products = new Set(productsTable);
            products.Add(new object[] { "A", "10001", "PC" });
            products.Add(new object[] { "B", "10001", "PC" });

            Set projection = products.Projection(new List<string> { "Maker" });

            Assert.AreEqual(2, projection.Count());
            Assert.AreEqual(1, projection.First().Entries.Count);
            Assert.IsTrue(projection.First().Entries[0].AttributeDefinition.Name == "Maker");
        }


        //private static CustomTuple CreateProductTuple(Relation relation, string maker, string model, string type)
        //{
        //    CustomTuple tuple = new CustomTuple(relation);

        //    tuple.Add(maker);
        //    tuple.Add(model);
        //    tuple.Add(type);

        //    return tuple;
        //}
    }
}