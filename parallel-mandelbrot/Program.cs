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

            byte[,] byteMap = await asyncHelper.GenerateThreads();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Bitmap bitmap = asyncHelper.BitmapFromByteArray(byteMap, 1920, 1080);

            bitmap.Save($"test_{DateTime.Now.ToString("ddMMyyyy_hhmmss")}.png", ImageFormat.Png);
            Console.WriteLine(elapsedMs);
            Console.ReadLine();
        }
    }
}
