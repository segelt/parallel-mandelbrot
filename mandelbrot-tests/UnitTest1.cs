using Microsoft.VisualStudio.TestTools.UnitTesting;
using parallel_mandelbrot;
using System.Collections.Generic;
using System.Linq;

namespace mandelbrot_tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void PartitionTests()
        {

            AsyncHelper asyncHelper = new AsyncHelper();

            //Count = 5
            //    [0]: 0
            //    [1]: 270
            //    [2]: 540
            //    [3]: 810
            //    [4]: 1080
            var partitionResults = asyncHelper.partitions(1080, 4);

            Assert.AreEqual(partitionResults.Count, 5);
            Assert.IsTrue(partitionResults.SequenceEqual(new List<int> { 0, 270, 540, 810, 1080 }));

        }
    }
}
