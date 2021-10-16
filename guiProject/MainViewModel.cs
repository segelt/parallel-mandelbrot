using guiProject.Bindings;
using parallel_mandelbrot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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

        private byte[,] generatedImage;

        private Bitmap _bitmap;
        public Bitmap bitmap { 
            get
            {
                return _bitmap;
            }
            set
            {
                bitmap = value;
                RaisePropertyChanged("bitmap");
            }
        }

        private BitmapImage _bitmapSource;
        public BitmapImage bitmapSource { 
            get
            {
                return _bitmapSource;
            }
            set
            {
                _bitmapSource = value;
                RaisePropertyChanged("bitmapSource");
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

            ButtonCommand = new RelayCommand(async o => await GenerateFractal());
        }

        private async Task GenerateFractal()
        {

            AsyncHelper asyncHelper = new AsyncHelper();
            generatedImage = await asyncHelper.PrepareFractalByteMapAsync(inputModel);
            _bitmap = asyncHelper.BitmapFromByteArray(generatedImage, inputModel.width, inputModel.height);

            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                BitmapImage bitmapimage = new BitmapImage();
                memory.Position = 0;
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                bitmapimage.Freeze();

                bitmapSource = bitmapimage;
            }
        }
    }
}
