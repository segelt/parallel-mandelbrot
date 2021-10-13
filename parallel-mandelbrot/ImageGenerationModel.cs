using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parallel_mandelbrot
{
    public class ImageGenerationModel
    {
        public int width { get; set; }
        public int height { get; set; }
        public int x_ThreadCount { get; set; }
        public int y_ThreadCount { get; set; }
        public int maxIterations { get; set; }
        
        public double maxZoomX { get; set; }
        public double minZoomX { 
            get
            {
                return -1 * maxZoomX;
            }
        }
        public double maxZoomY { get; set; }
        public double minZoomY
        {
            get
            {
                return -1 * maxZoomY;
            }
        }
    }
}
