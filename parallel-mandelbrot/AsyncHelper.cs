using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace parallel_mandelbrot
{
    public class AsyncHelper
    {
        #region Constants
        
        static int max_iter = 250;
        //static int width = 1920;
        //static int height = 1080;
        static double max_valx = 1.5;
        static double min_valx = -1 * max_valx;
        static double max_valy = 1.5;
        static double min_valy = -1 * max_valy;

        #endregion

        #region Different Mandelbrot Calculation Methods

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

        byte[] IsFractal_diff(int x, int y, ImageGenerationModel model)
        {
            double x_mapped = interpolate(x, 0, model.width, model.minZoomX, model.maxZoomX);
            double y_mapped = interpolate(y, 0, model.height, model.minZoomY, model.maxZoomY);

            double orig_x = x_mapped;
            double orig_y = y_mapped;
            int n = 0;

            while (n < model.maxIterations)
            {
                var real = x_mapped * x_mapped - y_mapped * y_mapped;
                var imaginary = 2 * x_mapped * y_mapped;

                x_mapped = real + orig_x;
                y_mapped = imaginary + orig_y;

                if (x_mapped * x_mapped + y_mapped * y_mapped > 16)
                {
                    break;
                }

                n++;
            }

            int pixelValue = (int)interpolate(n, 0, model.maxIterations, 0, 255);
            byte byteVal = Convert.ToByte(pixelValue);

            byte[] color_data = { byteVal, byteVal, byteVal };

            return color_data;

        }

        #endregion        
        
        #region Tasks

        public async Task GenerateFractalsPartitioned(int xlow, int xhigh, int ylow, int yhigh, 
            byte[,] byteMap, ImageGenerationModel model)
        {
            await Task.Run(() =>
            {
                for (int j = ylow; j < yhigh; j++)
                {
                    for (int i = xlow; i < xhigh; i++)
                    {
                        byte[] result = IsFractal_diff(i, j, model);

                        int idx = j * model.width + i;
                        lock (byteMap)
                        {
                            byteMap[0, idx] = result[0];
                            byteMap[1, idx] = result[1];
                            byteMap[2, idx] = result[2];
                        }
                    }
                }
            });
        }

        public async Task<byte[,]> PrepareFractalByteMapAsync(ImageGenerationModel model)
        {
            var partitions_width = partitions(model.width, model.x_ThreadCount);
            var partitions_height = partitions(model.height, model.y_ThreadCount);
            byte[,] byteMap = new byte[3, model.width * model.height];

            IList<Task> tasks = new List<Task>();

            for (int i = 0; i < partitions_width.Count - 1; i++)
            {
                for (int j = 0; j < partitions_height.Count - 1; j++)
                {
                    int xlow = partitions_width[i];
                    int xhigh = partitions_width[i + 1];
                    int ylow = partitions_height[j];
                    int yhigh = partitions_height[j + 1];

                    tasks.Add(GenerateFractalsPartitioned(xlow, xhigh, ylow, yhigh, byteMap, model));
                }
            }

            await Task.WhenAll(tasks);

            return byteMap;
        }

        #endregion

        #region Helpers

        public IList<int> partitions(int total, int n)
        {
            List<int> nums = new List<int> { 0 };
            int partition = total / n;

            int x = 0;
            int tot = total;
            while (tot > 0)
            {
                if (tot > partition)
                {
                    x += partition;
                    nums.Add(x);
                    tot -= partition;
                }
                else
                {
                    x += tot;
                    nums.Add(x);
                    tot -= tot;
                }
            }

            nums.ForEach(i => Console.Write(i + " "));
            return nums;
        }

        public Bitmap BitmapFromByteArray(byte[,] byteMap, int width, int height)
        {
            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int padding = bmpData.Stride - 3 * width;

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int idx = y * width + x;
                        ptr[2] = byteMap[0, idx]; //R
                        ptr[1] = byteMap[1, idx]; //G
                        ptr[0] = byteMap[2, idx]; //B

                        ptr += 3;
                    }
                    ptr += padding; //pad each row
                }
            }

            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        double interpolate(double x, double x1, double x2, double y1, double y2)
        {
            return y1 + ((x - x1) * (y2 - y1)) / (x2 - x1);
        }

        #endregion

    }
}
