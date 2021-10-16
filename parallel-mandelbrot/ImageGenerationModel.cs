using guiProject;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace parallel_mandelbrot
{
    public class ImageGenerationModel : ObservableBase
    {
        public int width { get; set; }
        public int height { get; set; }

        private int _x_ThreadCount;
        public int x_ThreadCount { 
            get
            {
                return _x_ThreadCount;
            }
            set
            {
                _x_ThreadCount = value;
                RaisePropertyChanged("x_ThreadCount");
            }
        }

        private int _y_ThreadCount;
        public int y_ThreadCount { 
            get
            {
                return _y_ThreadCount;
            }
            set
            {
                _y_ThreadCount = value;
                RaisePropertyChanged("y_ThreadCount");
            }
        }

        private int _maxIterations;
        public int maxIterations { 
            get
            {
                return _maxIterations;
            }
            set
            {
                _maxIterations = value;
                RaisePropertyChanged("maxIterations");
            }
        }
        public string maxIterationsStr { get
            {
                return maxIterations.ToString("0.00");
            }
        }

        private double _maxZoomX;
        public double maxZoomX { 
            get
            {
                return _maxZoomX;
            }
            set
            {
                _maxZoomX = value;
                RaisePropertyChanged("maxZoomX");
                //OnPropertyChanged();
            }
        }
        public double minZoomX { 
            get
            {
                return -1 * maxZoomX;
            }
        }

        private double _maxZoomY;
        public double maxZoomY { 
            get
            {
                return _maxZoomY;
            }
            set
            {
                _maxZoomY = value;
                RaisePropertyChanged("maxZoomY");
                //OnPropertyChanged();
            }
        }
        public double minZoomY
        {
            get
            {
                return -1 * maxZoomY;
            }
        }
    }
}
