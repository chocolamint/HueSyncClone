using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace HueSyncClone.Core.Drawing
{
    public class ColorSelector
    {
        public IEnumerable<Color> SelectColor(Bitmap bitmap, int count)
        {
            var (thumb, width, height) = Resize(bitmap, 72);
            using (thumb)
            {
                var colors = GetColors(thumb, width, height);
                var xyzColors = colors.Select(x => XyzColor.FromRgb(x));
                var labColors = xyzColors.Select(x => CieLabColor.FromXyz(x));
                var labSpace = new CieLabSpace();
                var selections = KmeansPlusPlus(labColors.ToArray(), labSpace, count);

                return selections.Select(x => labSpace.GetCentroid(x)).Select(x => x.ToXyzColor().ToRgbColor());
            }
        }

        internal static List<TColor>[] KmeansPlusPlus<TColor>(IReadOnlyList<TColor> colors, ISpace<TColor> space, int count, int? seed = null)
        {
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            var centroids = new List<TColor> { colors[random.Next(colors.Count)] };
            foreach (var c in SelectCentroids().Take(count - 1))
            {
                centroids.Add(c);
            }

            var preClusters = new List<TColor>[count];
            while (true)
            {
                var clusters = Enumerable.Range(0, count).Select(_ => new List<TColor>()).ToArray();
                foreach (var color in colors)
                {
                    clusters[GetNearestCentroidIndex(color)].Add(color);
                }

                if (Enumerable.Range(0, count).All(i => preClusters[i] != null && clusters[i].SequenceEqual(preClusters[i])))
                {
                    return clusters;
                }

                for (var i = 0; i < count; i++)
                {
                    centroids[i] = space.GetCentroid(clusters[i]);
                    preClusters[i] = clusters[i];
                }
            }

            int GetNearestCentroidIndex(TColor color)
            {
                var minDistance = double.MaxValue;
                var minIndex = -1;
                for (var i = 0; i < centroids.Count; i++)
                {
                    var d = space.GetDistance(centroids[i], color);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        minIndex = i;
                    }
                }

                return minIndex;
            }

            IEnumerable<TColor> SelectCentroids()
            {
                var distances = colors.Select(x => centroids.Min(c => Math.Pow(space.GetDistance(x, c), 2))).ToArray();
                var sum = distances.Sum();

                while (true)
                {
                    var init = random.NextDouble();
                    var d = 0.0;
                    var i = 0;
                    for (; i < colors.Count; i++)
                    {
                        d += distances[i] / sum;
                        if (d > init) break;
                    }

                    yield return colors[i];
                }
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
