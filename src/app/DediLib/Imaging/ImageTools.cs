using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DediLib.Imaging
{
    /// <summary>
    /// Image helper class
    /// </summary>
    public static class ImageTools
    {
        /// <summary>
        /// Creates a thumbnail image in memory
        /// </summary>
        /// <param name="image">Original image</param>
        /// <param name="thumbnailWidth">Thumbnail width</param>
        /// <param name="thumbnailHeight">Thumbnail height</param>
        /// <param name="cropImage">Crops the image to keep the aspect ratio</param>
        /// <returns></returns>
        public static Image CreateThumbnail(Image image, int thumbnailWidth, int thumbnailHeight, bool cropImage)
        {
            var newWidth = thumbnailWidth;
            var newHeight = thumbnailHeight;

            // crop image center according aspect ratio
            Image croppedImage;
            if (cropImage)
            {
                if (thumbnailWidth / (double)thumbnailHeight > image.Width / (double)image.Height)
                {
                    var h = (int)(image.Width / (thumbnailWidth / (double)thumbnailHeight));
                    croppedImage = CropImage(image, 0, (int)((image.Height - (double)h) / 2), image.Width, (int)((image.Height + (double)h) / 2));
                }
                else
                {
                    var w = (int)(image.Height * (thumbnailWidth / (double)thumbnailHeight));
                    croppedImage = CropImage(image, (int)((image.Width - (double)w) / 2), 0, (int)((image.Width + (double)w) / 2), image.Height);
                }
            }
            else
            {
                croppedImage = image;

                // maintain aspect ratio
                if ((image.Width / (double)thumbnailWidth) > (image.Height / (double)thumbnailHeight))
                {
                    newHeight = (int)(thumbnailWidth / (double)image.Width * image.Height);
                }
                else
                {
                    newWidth = (int)(thumbnailHeight / (double)image.Height * image.Width);
                }
                if (newWidth < 1) newWidth = 1;
                if (newHeight < 1) newHeight = 1;
            }

            // TODO: Maintain GIF transparency

            var res = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            using (var gRes = Graphics.FromImage(res))
            {
                gRes.CompositingQuality = CompositingQuality.HighQuality;
                gRes.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gRes.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gRes.SmoothingMode = SmoothingMode.HighQuality;

                gRes.DrawImage(croppedImage, 0, 0, newWidth, newHeight);
            }
            if (cropImage) croppedImage.Dispose();

            return res;
        }

        /// <summary>
        /// Crops an image to the specified coordinates
        /// </summary>
        /// <param name="source">Source image</param>
        /// <param name="x1">Left X value</param>
        /// <param name="y1">Top Y value</param>
        /// <param name="x2">Right X value</param>
        /// <param name="y2">Bottom Y value</param>
        /// <returns>Cropped image</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Right X value must be greater than left X value
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Bottom Y value must be greater than top Y value
        /// </exception>
        public static Bitmap CropImage(Image source, int x1, int y1, int x2, int y2)
        {
            if ((x1 < 0) || (x1 > source.Width)) throw new ArgumentOutOfRangeException("x1");
            if ((y1 < 0) || (y1 > source.Height)) throw new ArgumentOutOfRangeException("y1");
            if ((x2 < 0) || (x2 > source.Width)) throw new ArgumentOutOfRangeException("x2");
            if ((y2 < 0) || (y2 > source.Height)) throw new ArgumentOutOfRangeException("y2");

            var width = x2 - x1;
            var height = y2 - y1;
            if (width <= 0) throw new ArgumentOutOfRangeException("x2", x2, "Right X value must be greater than left X value");
            if (height <= 0) throw new ArgumentOutOfRangeException("y2", y2, "Bottom Y value must be greater than top Y value");

            var bmpSource = new Bitmap(source);
            var bmpRes = new Bitmap(width, height);

            BitmapData bdSource = null;
            BitmapData bdRes = null;

            try
            {
                bdSource = bmpSource.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                bdRes = bmpRes.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                var scanSource = (IntPtr)((long)bdSource.Scan0 + (y1 * source.Width + x1) * 4);
                var scanRes = bdRes.Scan0;

                var buf = new byte[width * 4];
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(scanSource, buf, 0, buf.Length);
                    Marshal.Copy(buf, 0, scanRes, buf.Length);
                    scanSource = (IntPtr)((long)scanSource + bdSource.Stride);
                    scanRes = (IntPtr)((long)scanRes + bdRes.Stride);
                }
            }
            finally
            {
                if (bdSource != null) bmpSource.UnlockBits(bdSource);
                if (bdRes != null) bmpRes.UnlockBits(bdRes);
            }

            return bmpRes;
        }

        /// <summary>
        /// Gets the pixel hash value of an image using the SHA1 algorithm
        /// </summary>
        /// <param name="image">Image to determine the hash of</param>
        /// <returns>Long hash value</returns>
        /// <exception cref="ArgumentNullException">image</exception>
        public static ulong GetImagePixelHashLong(Image image)
        {
            var hash = GetImagePixelHashSha1(image);
            ulong hashLong = 0;
            for (short i = 0; i < 8; i++) hashLong |= (ulong)hash[i] << (short)(i * 8);
            return hashLong;
        }

        /// <summary>
        /// Gets the pixel hash value of an image using the SHA1 algorithm
        /// </summary>
        /// <param name="image">Image to determine the hash of</param>
        /// <returns>Hash value</returns>
        /// <exception cref="ArgumentNullException">image</exception>
        public static byte[] GetImagePixelHashSha1(Image image)
        {
            if (image == null) throw new ArgumentNullException("image");

            byte[] hash;
            SHA1 sha1 = SHA1.Create("SHA1");

            int width = image.Width;
            int height = image.Height;
            var bmpSource = new Bitmap(image);
            BitmapData bdSource = null;
            var mem = new MemoryStream();
            try
            {
                bdSource = bmpSource.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                IntPtr scanSource = bdSource.Scan0;

                var buf = new byte[width * 4];
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(scanSource, buf, 0, buf.Length);
                    mem.Write(buf, 0, buf.Length);
                    scanSource = (IntPtr)((long)scanSource + bdSource.Stride);
                }

                mem.Position = 0;
                hash = sha1.ComputeHash(mem);
            }
            finally
            {
                if (bdSource != null) bmpSource.UnlockBits(bdSource);
                mem.SetLength(0);
                mem.Close();
            }
            return hash;
        }

        /// <summary>
        /// Embeds a watermark text into image
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="watermarkText">watermark text</param>
        /// <param name="fontFamily">font family</param>
        /// <param name="fontSize">font size</param>
        public static void EmbedWatermarkText(Image image, string watermarkText, string fontFamily, int fontSize)
        {
            if (string.IsNullOrEmpty(watermarkText)) return;

            using (var g = Graphics.FromImage(image))
            {
                var w = image.Width;
                var h = image.Height;

                using (var font = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                {
                    // draw text watermark at the bottom right corner
                    var sizeF = g.MeasureString(watermarkText, font);

                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.DrawString(watermarkText, font, SystemBrushes.Highlight, w - sizeF.Width - 5, h - sizeF.Height - 5);
                }
            }
        }

        /// <summary>
        /// Tries to find the content area within a bordered image
        /// </summary>
        /// <param name="source">Source image</param>
        /// <returns></returns>
        public static Rectangle FindBorderContentArea(Image source)
        {
            var width = source.Width;
            var height = source.Height;

            var bufImage = new FastImageArgb(new Bitmap(source));

            var borderLeft = 0;
            var borderTop = 0;
            var borderRight = 0;
            var borderBottom = 0;

            // get border pixels
            var pixelLeftBorder = bufImage.Buffer[(height / 2) * width];
            var pixelTopBorder = bufImage.Buffer[width / 2];
            var pixelRightBorder = bufImage.Buffer[(height / 2 + 1) * width - 1];
            var pixelBottomBorder = bufImage.Buffer[height * width - width / 2];

            // get initial border sizes
            {
                // get initial border left size
                var anchor = (height / 2) * width;
                for (var i = 0; i < width / 2; i++)
                {
                    anchor++;
                    if (IsPixelEqual(pixelLeftBorder, bufImage.Buffer[anchor])) continue;
                    borderLeft = i;
                    break;
                }

                // get initial border right size
                anchor = ((height / 2) + 1) * width;
                for (var i = 0; i < width / 2; i++)
                {
                    anchor--;
                    if (IsPixelEqual(pixelRightBorder, bufImage.Buffer[anchor])) continue;
                    borderRight = i;
                    break;
                }

                // get initial border top size
                anchor = width / 2;
                for (var i = 0; i < height / 2; i++)
                {
                    anchor += width;
                    if (IsPixelEqual(pixelTopBorder, bufImage.Buffer[anchor])) continue;
                    borderTop = i;
                    break;
                }

                // get initial border top size
                anchor = height * width - width / 2;
                for (var i = 0; i < height / 2; i++)
                {
                    anchor -= width;
                    if (IsPixelEqual(pixelBottomBorder, bufImage.Buffer[anchor])) continue;
                    borderBottom = i;
                    break;
                }
            }

            // find borders
            var steps = Math.Max(width / 2, height / 2);
            for (var j = 1; j < steps; j++)
            {
                int anchor;

                if (j < height / 2)
                {
                    // get border left size
                    anchor = (height / 2 - j) * width;
                    for (var i = 0; i < borderLeft; i++)
                    {
                        anchor++;
                        if (IsPixelEqual(pixelLeftBorder, bufImage.Buffer[anchor])) continue;
                        borderLeft = i;
                        break;
                    }
                    anchor = (height / 2 + j) * width;
                    for (var i = 0; i < borderLeft; i++)
                    {
                        anchor++;
                        if (IsPixelEqual(pixelLeftBorder, bufImage.Buffer[anchor])) continue;
                        borderLeft = i;
                        break;
                    }

                    // get border right size
                    anchor = ((height / 2) - j + 1) * width;
                    for (var i = 0; i < borderRight; i++)
                    {
                        anchor--;
                        if (IsPixelEqual(pixelRightBorder, bufImage.Buffer[anchor])) continue;
                        borderRight = i;
                        break;
                    }
                    anchor = ((height / 2) + j + 1) * width;
                    for (var i = 0; i < borderRight; i++)
                    {
                        anchor--;
                        if (IsPixelEqual(pixelRightBorder, bufImage.Buffer[anchor])) continue;
                        borderRight = i;
                        break;
                    }
                }

                if (j < width / 2)
                {
                    // get border top size
                    anchor = width / 2 - j;
                    for (var i = 0; i < borderTop; i++)
                    {
                        anchor += width;
                        if (IsPixelEqual(pixelTopBorder, bufImage.Buffer[anchor])) continue;
                        borderTop = i;
                        break;
                    }
                    anchor = width / 2 + j;
                    for (var i = 0; i < borderTop; i++)
                    {
                        anchor += width;
                        if (IsPixelEqual(pixelTopBorder, bufImage.Buffer[anchor])) continue;
                        borderTop = i;
                        break;
                    }

                    // get border top size
                    anchor = height * width - width / 2 - j;
                    for (var i = 0; i < borderBottom; i++)
                    {
                        anchor -= width;
                        if (IsPixelEqual(pixelBottomBorder, bufImage.Buffer[anchor])) continue;
                        borderBottom = i;
                        break;
                    }
                    anchor = height * width - width / 2 + j;
                    for (var i = 0; i < borderBottom; i++)
                    {
                        anchor -= width;
                        if (!IsPixelEqual(pixelBottomBorder, bufImage.Buffer[anchor]))
                        {
                            borderBottom = i;
                            break;
                        }
                    }
                }
            }

            // ignore borders on blank image
            if ((borderLeft + borderRight >= width) || (borderTop + borderBottom >= height)) return new Rectangle(0, 0, width, height);

            return new Rectangle(borderLeft, borderTop, width - borderRight - borderLeft, height - borderBottom - borderTop);
        }

        /// <summary>
        /// Tests if two pixels are equal with a particular tolerance
        /// </summary>
        /// <param name="pixel1">Pixel 1</param>
        /// <param name="pixel2">Pixel 2</param>
        /// <returns>true, if the pixels are equal</returns>
        private static bool IsPixelEqual(long pixel1, long pixel2)
        {
            var distance = 0;

            var diff = (byte)((byte)pixel1 - (byte)pixel2);
            distance += diff * diff;
            diff = (byte)((byte)(pixel1 >> 8) - (byte)(pixel2 >> 8));
            distance += diff * diff;
            diff = (byte)((byte)(pixel1 >> 16) - (byte)(pixel2 >> 16));
            distance += diff * diff;
            diff = (byte)((byte)(pixel1 >> 24) - (byte)(pixel2 >> 24));
            distance += diff * diff;

            return (distance < 25);
        }

        /// <summary>
        /// Prepares encoder parameters for quality
        /// </summary>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        /// <returns></returns>
        private static KeyValuePair<ImageCodecInfo, EncoderParameters> PrepareJpegEncoderQuality(int quality)
        {
            if (quality < 0 || quality > 100) throw new ArgumentOutOfRangeException("quality", "Quality must be between 0 and 100.");

            // Encoder parameter for image quality
            var qualityParam = new EncoderParameter(Encoder.Quality, quality);
            // Jpeg image codec
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            if (jpegCodec == null) throw new NotSupportedException("Internal JPEG codec not found");

            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            return new KeyValuePair<ImageCodecInfo, EncoderParameters>(jpegCodec, encoderParams);
        }

        /// <summary>
        /// Saves an image as JPEG with quality setting
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="fileName">file name</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        public static void SaveAsJpeg(this Image image, string fileName, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            image.Save(fileName, pair.Key, pair.Value);
        }

        /// <summary>
        /// Saves an image as JPEG with quality setting
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="stream">stream to write to</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        public static void SaveAsJpeg(this Image image, Stream stream, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            image.Save(stream, pair.Key, pair.Value);
        }

        /// <summary>
        /// Saves a bitmap as JPEG with quality setting
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="fileName">file name</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        public static void SaveAsJpeg(this Bitmap bitmap, string fileName, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            bitmap.Save(fileName, pair.Key, pair.Value);
        }

        /// <summary>
        /// Saves a bitmap as JPEG with quality setting
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="stream">stream to write to</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        public static void SaveAsJpeg(this Bitmap bitmap, Stream stream, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            bitmap.Save(stream, pair.Key, pair.Value);
        }

        /// <summary>
        /// Saves a bitmap as JPEG with quality setting
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        /// <returns>array of image data</returns>
        public static byte[] SaveAsJpeg(this Image image, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            using (var mem = new MemoryStream())
            {
                image.Save(mem, pair.Key, pair.Value);
                var buf = new byte[mem.Length];
                mem.Position = 0;
                mem.Read(buf, 0, buf.Length);
                return buf;
            }
        }

        /// <summary>
        /// Saves a bitmap as JPEG with quality setting
        /// </summary>
        /// <param name="bitmap">bitmap</param>
        /// <param name="quality">JPEG quality (0 to 100)</param>
        /// <returns>array of image data</returns>
        public static byte[] SaveAsJpeg(this Bitmap bitmap, int quality)
        {
            var pair = PrepareJpegEncoderQuality(quality);

            using (var mem = new MemoryStream())
            {
                bitmap.Save(mem, pair.Key, pair.Value);
                var buf = new byte[mem.Length];
                mem.Position = 0;
                mem.Read(buf, 0, buf.Length);
                return buf;
            }
        }

        /// <summary>
        /// Returns the image codec with the given mime type
        /// </summary>
        public static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats
            var codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec
            return codecs.FirstOrDefault(t => t.MimeType == mimeType);
        }
    }
}