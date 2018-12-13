﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace HueSyncClone.Drawing
{
    public class ColorPicker
    {
        private readonly int? _randomSeed;

        public ImageEditor ImageEditor { get; set; } = new ImageEditor();

        public ColorPicker()
        {

        }

        public ColorPicker(int randomSeed)
        {
            _randomSeed = randomSeed;
        }

        public IEnumerable<Color> PickColors(Bitmap bitmap, int count)
        {
            using (var thumb = ImageEditor.Resize(bitmap, 72))
            {
                var colors = GetColors(thumb);
                var xyzColors = colors.Select(x => XyzColor.FromRgb(x));
                var labSpace = new CieLabSpace();
                var labColors = xyzColors.Select(x => CieLabColor.FromXyz(x)).ToArray();

                var palette = labColors.Distinct().ToArray();
                if (palette.Length < count) return palette.Select(x => x.ToXyzColor().ToRgbColor());

                var selections = KmeansPlusPlus(labColors, labSpace, count, _randomSeed);

                return selections.GroupBy(x => x.Cluster, x => x.Color)
                                 .OrderByDescending(x => x.Count())
                                 .Select(x => labSpace.GetCentroid(x.ToArray()))
                                 .Select(x => x.ToXyzColor().ToRgbColor());
            }
        }

        internal static IEnumerable<Color> GetColors(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

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

            int GetBytesPerPixelFromPixelFormat(PixelFormat pixelFormat)
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
        }

        internal static (TColor Color, int Cluster)[] KmeansPlusPlus<TColor>(TColor[] colors, ISpace<TColor> space, int count, int? seed = null)
        {
            var points = colors.Select(x => (Color: x, Cluster: -1)).ToArray();
            var random = seed.HasValue ? new Random(seed.Value) : new Random();

            var initialCentroidIndex = random.Next(points.Length);
            var centroids = new List<(TColor Color, int index)> { (points[initialCentroidIndex].Color, initialCentroidIndex) };
            foreach (var c in SelectCentroids().Take(count - 1))
            {
                centroids.Add(c);
            }
            
            var preClusters = new List<TColor>[count];
            while (true)
            {
                var clusters = Enumerable.Range(0, count).Select(_ => new List<TColor>()).ToArray();
                for (var i = 0; i < points.Length; i++)
                {
                    var point = points[i];
                    var clusterIndex = GetNearestCentroidIndex(point.Color);
                    points[i].Cluster = clusterIndex;
                    clusters[clusterIndex].Add(point.Color);
                }

                if (Enumerable.Range(0, count).All(i => preClusters[i] != null && clusters[i].SequenceEqual(preClusters[i])))
                {
                    return points;
                }

                for (var i = 0; i < count; i++)
                {
                    var newCentroid = space.GetCentroid(clusters[i]);
                    centroids[i] = (newCentroid, centroids[i].index);
                    // move centroid
                    points[centroids[i].index].Color = newCentroid;
                    preClusters[i] = clusters[i];
                }
            }

            int GetNearestCentroidIndex(TColor color)
            {
                var minDistance = double.MaxValue;
                var minIndex = -1;
                for (var i = 0; i < centroids.Count; i++)
                {
                    var d = space.GetDistance(centroids[i].Color, color);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        minIndex = i;
                    }
                }

                return minIndex;
            }

            IEnumerable<(TColor color, int index)> SelectCentroids()
            {
                var distances = points.Select(x => centroids.Min(c => Math.Pow(space.GetDistance(x.Color, c.Color), 2))).ToArray();
                var sum = distances.Sum();

                while (true)
                {
                    var init = random.NextDouble();
                    var d = 0.0;
                    for (var i = 0; i < points.Length; i++)
                    {
                        d += distances[i] / sum;
                        if (d > init)
                        {
                            // duplicate
                            if (centroids.Any(c => Math.Abs(space.GetDistance(points[i].Color, c.Color)) < double.Epsilon)) break;

                            yield return (points[i].Color, i);
                            break;
                        }
                    }
                }
            }
        }
    }
}
