using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime;
using System.Runtime.InteropServices;

namespace DediLib.Imaging
{
    /// <summary>
    /// Represents a ARGB image
    /// </summary>
    public class FastImageArgb
    {
        /// <summary>
        /// Process pixel delegate
        /// </summary>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        /// <param name="colorRead">read color value</param>
        /// <returns>color to write</returns>
        public delegate int ProcessPixelDelegate(int x, int y, int colorRead);

        private int[] _buffer = new int[0];
        /// <summary>
        /// Image buffer
        /// </summary>
        public int[] Buffer => _buffer;

        private int _height;
        /// <summary>
        /// Gets the image height
        /// </summary>
        public int Height => _height;

        private int _width;
        /// <summary>
        /// Gets the image width
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Constructor
        /// </summary>
        protected FastImageArgb()
        {
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~FastImageArgb()
        {
            _buffer = null;
            _height = 0;
            _width = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="width">image width</param>
        /// <param name="height">image height</param>
        [TargetedPatchingOptOut("")]
        public FastImageArgb(int width, int height)
        {
            _buffer = new int[width * height];
            _height = height;
            _width = width;
        }

        /// <summary>
        /// Creates a ARGB image from image
        /// </summary>
        /// <param name="image">image to be converted</param>
        /// <returns>ARGB image</returns>
        [TargetedPatchingOptOut("")]
        public FastImageArgb(Image image)
            : this(new Bitmap(image))
        {
        }

        /// <summary>
        /// Creates a ARGB image from bitmap
        /// </summary>
        /// <param name="bitmap">bitmap to be converted</param>
        /// <returns>ARGB image</returns>
        [TargetedPatchingOptOut("")]
        public FastImageArgb(Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException(nameof(bitmap));
            _height = bitmap.Height;
            _width = bitmap.Width;
            _buffer = new int[_width * _height];

            BitmapData bitmapData = null;
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var scan = bitmapData.Scan0;

                var pos = 0;
                for (var y = 0; y < _height; y++)
                {
                    Marshal.Copy(scan, _buffer, pos, _width);
                    pos += _width;
                    scan = (IntPtr)((long)scan + bitmapData.Stride);
                }
            }
            finally
            {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Gets the alpha value of an ARGB value
        /// </summary>
        /// <param name="argbValue">ARGB value</param>
        /// <returns>alpha value</returns>
        [TargetedPatchingOptOut("")]
        public static byte GetAValue(int argbValue)
        {
            return (byte)(argbValue >> 24);
        }

        /// <summary>
        /// Gets the blue value of an ARGB value
        /// </summary>
        /// <param name="argbValue">ARGB value</param>
        /// <returns>blue value</returns>
        [TargetedPatchingOptOut("")]
        public static byte GetBValue(int argbValue)
        {
            return (byte)(argbValue & 0x000000FF);
        }

        /// <summary>
        /// Gets the green value of an ARGB value
        /// </summary>
        /// <param name="argbValue">ARGB value</param>
        /// <returns>green value</returns>
        [TargetedPatchingOptOut("")]
        public static byte GetGValue(int argbValue)
        {
            return (byte)((argbValue & 0x0000FF00) >> 8);
        }

        /// <summary>
        /// Gets the red value of an ARGB value
        /// </summary>
        /// <param name="argbValue">ARGB value</param>
        /// <returns>red value</returns>
        [TargetedPatchingOptOut("")]
        public static byte GetRValue(int argbValue)
        {
            return (byte)((argbValue & 0x00FF0000) >> 16);
        }

        /// <summary>
        /// Gets the ARGB value of components
        /// </summary>
        /// <param name="a">alpha value</param>
        /// <param name="r">red value</param>
        /// <param name="g">greeb vakue</param>
        /// <param name="b">blue value</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("")]
        public static int GetArgbValue(byte a, byte r, byte g, byte b)
        {
            return a << 24 | r << 16 | g << 8 | b;
        }

        /// <summary>
        /// Process complete image
        /// </summary>
        /// <param name="processPixelDelegate"></param>
        [TargetedPatchingOptOut("")]
        public void Process(ProcessPixelDelegate processPixelDelegate)
        {
            var index = 0;
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var color = _buffer[index];
                    var newColor = processPixelDelegate(x, y, color);
                    if (newColor != color) _buffer[index] = newColor;
                    index++;
                }
            }
        }

        /// <summary>
        /// Creates a bitmap
        /// </summary>
        /// <returns>bitmap</returns>
        [TargetedPatchingOptOut("")]
        public Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = null;
            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                var scan = bitmapData.Scan0;

                var pos = 0;
                for (var y = 0; y < _height; y++)
                {
                    Marshal.Copy(_buffer, pos, scan, _width);
                    pos += _width;
                    scan = (IntPtr)((long)scan + bitmapData.Stride);
                }
            }
            finally
            {
                if (bitmapData != null) bitmap.UnlockBits(bitmapData);
            }
            return bitmap;
        }
    }
}
