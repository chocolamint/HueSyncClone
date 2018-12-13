using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HueSyncClone.Core.Drawing
{
    public class ColorSelector
    {
        public void SelectColor(Bitmap bitmap, int count)
        {
            var (thumb, width, height) = Resize(bitmap, 72);
            using (thumb)
            {
                var colors = GetColors(thumb, width, height);
            }
        }

        internal static IEnumerable<Color> GetColors(Bitmap bitmap, int width, int height)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            try
            {
                var stride = bitmapData.Stride;
                var pixels = new byte[stride * height];
                Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var bytesPerPixel = GetBytesPerPixelFromPixelFormat(bitmap.PixelFormat);
                        var position = x * bytesPerPixel + stride * y;
                        var b = pixels[position + 0];
                        var g = pixels[position + 1];
                        var r = pixels[position + 2];
                        yield return Color.FromArgb(r, g, b);
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private static int GetBytesPerPixelFromPixelFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Canonical:
                    return 4;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format16bppArgb1555:
                    return 2;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return 4;
                case PixelFormat.Format48bppRgb:
                    return 6;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null);
            }
        }

        internal static (Bitmap thumb, int width, int height) Resize(Bitmap bitmap, int size)
        {
            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;

            if (originalWidth <= size && originalHeight <= size) return (bitmap, originalWidth, originalHeight);

            var resizedWidth = originalWidth > originalHeight ? size : originalWidth * size / originalHeight;
            var resizedHeight = originalHeight > originalWidth ? size : originalHeight * size / originalWidth;
            var resized = new Bitmap(resizedWidth, resizedHeight);

            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, resizedWidth, resizedHeight);

                var rotateFlip = GetRotateFlip(bitmap);
                resized.RotateFlip(rotateFlip);
                var rotate = (int)rotateFlip % 2 == 1;

                return (resized, !rotate ? resizedWidth : resizedHeight, !rotate ? resizedHeight : resizedWidth);
            }
        }

        private static RotateFlipType GetRotateFlip(Bitmap bitmap)
        {
            var property = bitmap.PropertyItems.FirstOrDefault(p => p.Id == 0x0112);

            if (property != null)
            {
                var orientation = BitConverter.ToUInt16(property.Value, 0);
                switch (orientation)
                {
                    case 1:
                        return RotateFlipType.RotateNoneFlipNone;
                    case 2:
                        return RotateFlipType.RotateNoneFlipX;
                    case 3:
                        return RotateFlipType.Rotate180FlipNone;
                    case 4:
                        return RotateFlipType.RotateNoneFlipY;
                    case 5:
                        return RotateFlipType.Rotate270FlipY;
                    case 6:
                        return RotateFlipType.Rotate90FlipNone;
                    case 7:
                        return RotateFlipType.Rotate90FlipY;
                    case 8:
                        return RotateFlipType.Rotate270FlipNone;
                }
            }
            return RotateFlipType.RotateNoneFlipNone;
        }
    }
}
