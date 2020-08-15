using NUnit.Framework;
using System.Drawing;

namespace DatabaseEngine.Tests
{
    public class PointerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Short()
        {
            Assert.AreEqual(256, new Pointer(1, 0).Short);
            Assert.AreEqual(257, new Pointer(1, 1).Short);
        }

        [Test]
        public void ShortToPointer()
        {
            short l = new Pointer(1, 2).Short;
            Pointer c = new Pointer(l);

            Assert.AreEqual(1, c.PageNumber);
            Assert.AreEqual(2, c.Index);
        }
    }
}
