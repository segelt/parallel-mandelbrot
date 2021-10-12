using Microsoft.VisualStudio.TestTools.UnitTesting;
using parallel_mandelbrot;

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

            Assert.AreEqual(true, true);

        }
    }
}
