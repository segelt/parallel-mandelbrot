using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace parallel_mandelbrot
{
    public class AsyncHelper
    {
        #region Constants
        
        static int MAX_ITER = 250;
        static double X_MAX = 1920;
        static double Y_MAX = 1080;
        static double X_LOW = -2.5;
        static double X_HIGH = 1.0555555;
        static double Y_LOW = -1;
        static double Y_HIGH = 1;

        #endregion

        double interpolate(double x, double x1, double x2, double y1, double y2)
        {
            return y1 + ((x - x1) * (y2 - y1)) / (x2 - x1);
        }

        async Task FractalTask(int x, int y, Bitmap bitmap, SemaphoreSlim throttler)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await throttler.WaitAsync();
                    double x_interpolate = interpolate(x, 0, X_MAX, X_LOW, X_HIGH);
                    double y_interpolate = interpolate(y, 0, Y_MAX, Y_LOW, Y_HIGH);

                    Complex complex = new Complex(x_interpolate, y_interpolate);

                    bool result = IsFractal(complex);
                    if (result)
                    {
                        lock (bitmap)
                        {
                            bitmap.SetPixel(x, y, Color.White);
                        }
                    }
                }
                finally
                {
                    throttler.Release();
                }
            });
        }

        bool IsFractal(Complex c)
        {
            Complex z = Complex.Zero;

            int n = 0;
            while (n < MAX_ITER)
            {
                z = Complex.Add(Complex.Multiply(z, z), c);
                if (Complex.Abs(z) > 4) break;

                n++;
            }
            return n > 20;

        }

        Task[] GenerateTasks(Bitmap bitmap)
        {
            IList<Task> tasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(10);
            for(int x = 0; x < X_MAX; x++)
            {
                for(int y = 0; y < Y_MAX; y++)
                {

                    tasks.Add(FractalTask(x,y, bitmap, throttler));
                }
            }

            return tasks.ToArray();
        }

        public async Task<Bitmap> GenerateFractal()
        {
            Bitmap bitmap = new Bitmap(1920, 1080);

            var tasks = GenerateTasks(bitmap);

            await Task.WhenAll(tasks);

            return bitmap;
        }
    }
}
