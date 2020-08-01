using NUnit.Framework;

namespace DatabaseEngine.Tests
{
    public class BTreeTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /*[Test]
        public void MaxN()
        {
            BPlusTreeNode node = new BPlusTreeNode();
            node.AddValue(10);
            node.AddValue(15);
            node.AddValue(20);

            Assert.AreEqual(3, node.Values.Count);
            //Assert.AreEqual(0, node.Children.Count);
        }

        [Test]
        public void SortedChildren()
        {
            BPlusTreeNode node = new BPlusTreeNode();
            node.AddValue(15);
            node.AddValue(10);
            node.AddValue(20);

            Assert.AreEqual(10, node.Values[0]);
            Assert.AreEqual(15, node.Values[1]);
            Assert.AreEqual(20, node.Values[2]);
        }

        [Test]
        public void Split()
        {
            BPlusTreeNode node = new BPlusTreeNode();
            node.AddValue(10);
            node.AddValue(15);
            node.AddValue(20);
            node.AddValue(30);

            Assert.AreEqual(1, node.Values.Count);
            //Assert.AreEqual(2, node.Children.Count);
        }

        [Test]
        public void AddPropogation()
        {
            BPlusTreeNode node = new BPlusTreeNode();
            node.AddValue(10);
            node.AddValue(20);
            node.AddValue(30);
            node.AddValue(40);
            node.AddValue(50);
            node.AddValue(60);
            node.AddValue(70);
            node.AddValue(80);
            node.AddValue(90);
            node.AddValue(100);

            string s = node.ToDot();
        }*/
    }
}
