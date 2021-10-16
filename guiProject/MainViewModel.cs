using guiProject.Bindings;
using parallel_mandelbrot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace guiProject
{
    public class MainViewModel : ViewModelBase
    {
        private WriteableBitmap writeableBmp;
        public ICommand ButtonCommand { get; set; }

        private ImageGenerationModel _inputModel;
        public ImageGenerationModel inputModel
        {
            get
            {
                return _inputModel;
            }
            set
            {
                _inputModel = value;
            }
        }

        public MainViewModel()
        {
            _inputModel = new ImageGenerationModel
            {
                width = 1920,
                height = 1080,
                x_ThreadCount = 6,
                y_ThreadCount = 10,
                maxIterations = 250,
                maxZoomX = 3.5,
                maxZoomY = 3.5
            };

            ButtonCommand = new RelayCommand(o => TriggerButtonAction());
        }

        private void TriggerButtonAction()
        {
            Console.WriteLine("Button clicked..");
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    writeableBmp = new WriteableBitmap((int)bitmapCanvas.ActualWidth,
        //        (int)bitmapCanvas.ActualHeight,
        //        96,
        //        96,
        //        PixelFormats.Rgb24, null);

        //    Image generatedImage = new Image { Source = writeableBmp };

        //    bitmapCanvas.Children.Add(generatedImage);

        //}

        private void drawPixel()
        {
            try
            {
                // Reserve the back buffer for updates.
                writeableBmp.Lock();

                for (int i = 0; i < 40; i++)
                {
                    int row = i;
                    int column = i;
                    unsafe
                    {
                        // Get a pointer to the back buffer.
                        IntPtr pBackBuffer = writeableBmp.BackBuffer;

                        // Find the address of the pixel to draw.
                        pBackBuffer += row * writeableBmp.BackBufferStride;
                        pBackBuffer += column * 4;

                        // Compute the pixel's color.
                        int color_data = 255 << 16; // R
                        color_data |= 128 << 8;   // G
                        color_data |= 255 << 0;   // B

                        // Assign the color data to the pixel.
                        *((int*)pBackBuffer) = color_data;
                    }

                    // Specify the area of the bitmap that changed.
                    writeableBmp.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                }
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBmp.Unlock();
            }
        }
    }
}
