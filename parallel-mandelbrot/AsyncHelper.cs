﻿using System;
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
        static int width = 1920;
        static int height = 1080;
        static double max_valx = 1.5;
        static double min_valx = -1 * max_valx;
        static double max_valy = 1.5;
        static double min_valy = -1 * max_valy;

        #endregion
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

        double interpolate(double x, double x1, double x2, double y1, double y2)
        {
            return y1 + ((x - x1) * (y2 - y1)) / (x2 - x1);
        }

        byte[] IsFractal_diff(int x, int y)
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

                x_mapped = real + orig_x;
                y_mapped = imaginary + orig_y;

                if(x_mapped * x_mapped + y_mapped * y_mapped > 16)
                {
                    break;
                }

                n++;
            }

            int pixelValue = (int)interpolate(n, 0, max_iter, 0, 255);
            //int pixelValue = 0;
            //if(n == max_iter)
            //{
                

            //}

            byte byteVal = Convert.ToByte(pixelValue);

            byte[] color_data = { byteVal, byteVal, byteVal };

            return color_data;

        }

        #region Synchronous

        public void GenerateFractalsPartitioned(int xlow, int xhigh, int ylow, int yhigh, byte[,] byteMap)
        {
            for(int j = ylow; j < yhigh; j++)
            {
                for(int i = xlow; i < xhigh; i++)
                {
                    //double x_interpolate = interpolate(i, 0, width, min_valx, max_valx);
                    //double y_interpolate = interpolate(j, 0, height, min_valy, max_valy);

                    //Complex complex = new Complex(x_interpolate, y_interpolate);

                    //bool result = IsFractal_NaiveImplementation(complex);

                    byte[] result = IsFractal_diff(i, j);

                    //if (result)
                    //{
                    int idx = j * width + i;
                    lock (byteMap)
                    {
                        byteMap[0, idx] = result[0];
                        byteMap[1, idx] = result[1];
                        byteMap[2, idx] = result[2];
                    }
                    //}
                    //Console.WriteLine($"{j} - {i}");
                }
            }
        }

        public async Task GenerateFractalsTask(int xlow, int xhigh, int ylow, int yhigh, byte[,] byteMap)
        {
            await Task.Run(() =>
            {
                //Console.WriteLine("running");
                GenerateFractalsPartitioned(xlow, xhigh, ylow, yhigh, byteMap);
                //Console.WriteLine("done");
            });
        }

        public async Task<byte[,]> GenerateThreads()
        {
            var partitions_width = partitions((int)width, 6);
            var partitions_height = partitions((int)height, 10);
            byte[,] byteMap = new byte[3, width * height];

            IList<Task> tasks = new List<Task>();

            for (int i = 0; i < partitions_width.Count - 1; i++)
            {
                for (int j = 0; j < partitions_height.Count - 1; j++)
                {
                    int xlow = partitions_width[i];
                    int xhigh = partitions_width[i + 1];
                    int ylow = partitions_height[j];
                    int yhigh = partitions_height[j + 1];

                    tasks.Add(GenerateFractalsTask(xlow, xhigh, ylow, yhigh, byteMap));
                }
            }

            await Task.WhenAll(tasks);

            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData imageData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int padding = imageData.Stride - 3 * width;
            

            Console.WriteLine("here....");

            return byteMap;
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

        public Bitmap BitmapFromByteArray(byte[,] byteMap, int width, int height)
        {
            Bitmap bitmap = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            int padding = bmpData.Stride - 3 * width;

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;
                for(int y = 0; y < height; y++)
                {
                    for(int x = 0; x < width; x++)
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
        #endregion
    }
}
