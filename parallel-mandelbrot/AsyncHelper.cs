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

        #region Synchronous

        public void GenerateFractalsPartitioned(int xlow, int xhigh, int ylow, int yhigh)
        {
            for(int j = ylow; j < yhigh; j++)
            {
                for(int i = xlow; i < xhigh; i++)
                {
                    double x_interpolate = interpolate(i, 0, X_MAX, X_LOW, X_HIGH);
                    double y_interpolate = interpolate(j, 0, Y_MAX, Y_LOW, Y_HIGH);

                    Complex complex = new Complex(x_interpolate, y_interpolate);

                    bool result = IsFractal(complex);
                }
            }
        }

        public async Task GenerateFractalsTask(int xlow, int xhigh, int ylow, int yhigh)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("running");
                GenerateFractalsPartitioned(xlow, xhigh, ylow, yhigh);
                Console.WriteLine("done");
            });
        }

        public async Task GenerateThreads()
        {
            Bitmap bitmap = new Bitmap(1920, 1080);
            BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width,
                bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytesPerPixel = 3;

            var partitions_width = partitions((int)X_MAX, 6);
            var partitions_height = partitions((int)Y_MAX, 10);

            //GenerateFractalsPartitioned(widthMid + 1, (int)X_MAX - 1, heightMid + 1, (int)Y_MAX - 1);

            IList<Task> tasks = new List<Task>();

            for (int i = 0; i < partitions_width.Count - 1; i++)
            {
                for (int j = 0; j < partitions_height.Count - 1; j++)
                {
                    int xlow = partitions_width[i];
                    int xhigh = partitions_width[i + 1];
                    int ylow = partitions_height[j];
                    int yhigh = partitions_height[j + 1];

                    tasks.Add(GenerateFractalsTask(xlow, xhigh, ylow, yhigh));
                }
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("here....");
        }

        public IList<int> partitions(int total, int n)
        {
            List<int> nums = new List<int> { 0 };
            int partition = total / n;

            int x = 0;
            int tot = total;
            while(tot > 0)
            {
                if (tot > partition)
                {
                    x += partition;
                    nums.Add(x);
                    tot -= partition;
                }
                else {
                    x += tot;
                    nums.Add(x);
                    tot -= tot;
                }
            }

            nums.ForEach(i => Console.Write(i + " "));
            return nums;
        }

        #endregion
    }
}
