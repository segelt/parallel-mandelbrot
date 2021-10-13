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

            ImageGenerationModel inputModel = new ImageGenerationModel
            {
                width = 1920,
                height = 1080,
                x_ThreadCount = 6,
                y_ThreadCount = 10,
                maxIterations = 250,
                maxZoomX = 3.5,
                maxZoomY = 3.5
            };

            byte[,] byteMap = await asyncHelper.PrepareFractalByteMapAsync(inputModel);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Bitmap bitmap = asyncHelper.BitmapFromByteArray(byteMap, inputModel.width, inputModel.height);

            bitmap.Save($"test_{DateTime.Now.ToString("ddMMyyyy_hhmmss")}.png", ImageFormat.Png);
            Console.WriteLine(elapsedMs);
            Console.ReadLine();
        }
    }
}
