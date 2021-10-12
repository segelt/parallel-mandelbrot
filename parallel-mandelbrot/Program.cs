using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Threading.Tasks;

namespace parallel_mandelbrot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AsyncHelper asyncHelper = new AsyncHelper();

            var watch = System.Diagnostics.Stopwatch.StartNew();

            await asyncHelper.GenerateThreads();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            //bitmap.Save("test.png", ImageFormat.Png);
            Console.ReadLine();
        }
    }
}
