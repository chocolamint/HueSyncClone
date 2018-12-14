using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HueSyncClone.Hue;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class ColorSelectorTest
    {
        [Theory]
        [InlineData("image1_thumb", 72, 54, 2, 2, 58, 112, 191)]
        [InlineData("image2_thumb", 54, 72, 51, 38, 236, 125, 223)]
        public void TestGetColors(string imageName, int width, int height, int checkX, int checkY, int expectedR, int expectedG, int expectedB)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var colors = ColorPicker.GetColors(bitmap).ToList();

                Assert.Equal(width * height, colors.Count);
                var checkIndex = checkY * width + checkX;
                Assert.Equal(expectedR, colors[checkIndex].R);
                Assert.Equal(expectedG, colors[checkIndex].G);
                Assert.Equal(expectedB, colors[checkIndex].B);
            }
        }

        [Fact]
        public void TestKmeansPlusPlus()
        {
            var colors = new[]
            {
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(200, 200, 200),
                Color.FromArgb(140, 24, 53),
                Color.FromArgb(0, 0, 0),
            };

            var clustered = ColorPicker.KmeansPlusPlus(colors, new RgbSpace(), 2, 0).ToArray();

            Assert.Equal(new[]
            {
                (Color.FromArgb(255, 255, 255), 1),
                (Color.FromArgb(254, 254, 254), 1),
                (Color.FromArgb(0, 0, 0), 0),
                (Color.FromArgb(0, 0, 0), 0),
            }, clustered);
        }

        [Theory]
        [InlineData("image1", Skip = "")]
        [InlineData("image2", Skip = "")]
        public void TestResultImage(string imageName)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            using (var thumb = new ImageEditor().Resize(bitmap, 72))
            {
                var colors = ColorPicker.GetColors(thumb);
                var xyzColors = colors.Select(x => XyzColor.FromRgb(x));
                var labSpace = new CieLabSpace();
                var labColors = xyzColors.Select(x => CieLabColor.FromXyz(x)).ToArray();
                var picked = ColorPicker.KmeansPlusPlus(labColors, labSpace, 4, 0).ToList();
                var clusteres = picked.GroupBy(x => x.Cluster, x => x.Color).ToDictionary(x => x.Key, x => labSpace.GetCentroid(x.ToArray()));
                var width = thumb.Width;
                var height = thumb.Height;

                using (var result = new Bitmap(width, height))
                {
                    var bitmapData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    try
                    {
                        var pixels = new byte[bitmapData.Stride * height];
                        Marshal.Copy(bitmapData.Scan0, pixels, 0, pixels.Length);
                        var i = 0;
                        for (var y = 0; y < height; y++)
                        {
                            for (var x = 0; x < width; x++)
                            {
                                var pos = y * bitmapData.Stride + x * 3;
                                var color = clusteres[picked[i++].Cluster];
                                var rgb = color.ToXyzColor().ToRgbColor();
                                pixels[pos] = rgb.B;
                                pixels[pos + 1] = rgb.G;
                                pixels[pos + 2] = rgb.R;
                            }
                        }
                        Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
                    }
                    finally
                    {
                        result.UnlockBits(bitmapData);
                    }

                    result.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{imageName}_clustered.jpg"));
                }
            }
        }

        [Theory]
        [InlineData("image1", "#578dd6", "#ddcab3", "#4f4446")]
        [InlineData("image2", "#4a1b86", "#150823", "#c4b2dc")]
        public void TestAll(string imageName, string expected1, string expected2, string expected3)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var colors = new ColorPicker(0).PickColors(bitmap, 3).ToArray();

                string ToHexString(Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

                Assert.Equal(3, colors.Length);
                Assert.Equal(expected1, ToHexString(colors[0]));
                Assert.Equal(expected2, ToHexString(colors[1]));
                Assert.Equal(expected3, ToHexString(colors[2]));
            }
        }

        private struct RgbSpace : ISpace<Color>
        {
            public double GetDistance(Color x, Color y)
                => Math.Sqrt(Math.Pow(x.R - y.R, 2) + Math.Pow(x.G - y.G, 2) + Math.Pow(x.B - y.B, 2));

            public Color GetCentroid(IReadOnlyCollection<Color> xs)
                => Color.FromArgb((int)xs.Average(x => x.R), (int)xs.Average(x => x.G), (int)xs.Average(x => x.B));
        }
    }
}
