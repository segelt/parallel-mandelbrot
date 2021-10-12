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
        
        static int max_iter = 250;
        static double width = 1920;
        static double height = 1080;
        static double max_valx = 2.5;
        static double min_valx = -1 * max_valx;
        static double max_valy = 2.5;
        static double min_valy = -1 * max_valy;

        #endregion

        double interpolate(double x, double x1, double x2, double y1, double y2)
        {
            return y1 + ((x - x1) * (y2 - y1)) / (x2 - x1);
        }

        bool IsFractal_NaiveImplementation(Complex c)
        {
            Complex z = Complex.Zero;

            int n = 0;
            while (n < max_iter)
            {
                z = Complex.Add(Complex.Multiply(z, z), c);
                if (Complex.Abs(z) > 4) break;

                n++;
            }
            return n > 20;
        }

        bool IsFractal_diff(int x, int y)
        {
            double x_mapped = interpolate(x, 0, width, min_valx, max_valx);
            double y_mapped = interpolate(y, 0, height, min_valy, max_valy);

            double orig_x = x_mapped;
            double orig_y = y_mapped;
            int n = 0;

            while (n < max_iter)
            {
                var real = x_mapped * x_mapped - y_mapped * y_mapped;
                var imaginary = 2* x_mapped * y_mapped;

                x_mapped = real * real + orig_x;
                y_mapped = imaginary * imaginary + orig_y;

                if(x_mapped * x_mapped + y_mapped * y_mapped > 4)
                {
                    return false;
                }

                n++;
            }

            return true;

        }

        #region Synchronous

        public void GenerateFractalsPartitioned(int xlow, int xhigh, int ylow, int yhigh)
        {
            for(int j = ylow; j < yhigh; j++)
            {
                for(int i = xlow; i < xhigh; i++)
                {
                    //double x_interpolate = interpolate(i, 0, width, min_valx, max_valx);
                    //double y_interpolate = interpolate(j, 0, height, min_valy, max_valy);

                    //Complex complex = new Complex(x_interpolate, y_interpolate);

                    //bool result = IsFractal_NaiveImplementation(complex);

                    bool result = IsFractal_diff(i, j);
                }
            }
        }

        public async Task GenerateFractalsTask(int xlow, int xhigh, int ylow, int yhigh)
        {
            await Task.Run(() =>
            {
                //Console.WriteLine("running");
                GenerateFractalsPartitioned(xlow, xhigh, ylow, yhigh);
                //Console.WriteLine("done");
            });
        }

        public async Task GenerateThreads()
        {
            Bitmap bitmap = new Bitmap(1920, 1080);
            BitmapData imageData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width,
                bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytesPerPixel = 3;

            var partitions_width = partitions((int)width, 6);
            var partitions_height = partitions((int)height, 10);

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
