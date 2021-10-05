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

            Bitmap bitmap = await asyncHelper.GenerateFractal();
            bitmap.Save("test.png", ImageFormat.Png);
            Console.ReadLine();
        }
    }
}
