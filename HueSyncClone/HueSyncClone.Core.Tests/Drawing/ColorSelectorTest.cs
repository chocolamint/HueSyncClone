using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HueSyncClone.Core.Drawing;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class ColorSelectorTest
    {
        [Theory]
        [InlineData("image1", 72, 54)]
        [InlineData("image2", 54, 72)]
        public void TestResize(string imageName, int expectedWidth, int expectedHeight)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var (thumb, width, height) = ColorSelector.Resize(bitmap, 72);
                using (thumb)
                {
                    // thumb.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), imageName + ".jpg"));
                    Assert.Equal(expectedWidth, width);
                    Assert.Equal(expectedHeight, height);
                }
            }
        }

        [Theory]
        [InlineData("image1_thumb", 72, 54, 2, 2, 58, 112, 191)]
        [InlineData("image2_thumb", 54, 72, 51, 38, 236, 125, 223)]
        public void TestGetColors(string imageName, int width, int height, int checkX, int checkY, int expectedR, int expectedG, int expectedB)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var colors = ColorSelector.GetColors(bitmap, width, height).ToList();

                Assert.Equal(width * height, colors.Count);
                var checkIndex = checkY * width + checkX;
                Assert.Equal(expectedR, colors[checkIndex].R);
                Assert.Equal(expectedG, colors[checkIndex].G);
                Assert.Equal(expectedB, colors[checkIndex].B);
            }
        }

        [Theory]
        [InlineData(255, 255, 255, 100, 0.00526049995830391, -0.010408184525267927)]
        [InlineData(211, 96, 21, 54.31101910924566, 41.78198588920223, 58.46714536715948)]
        public void TestRgbToLab(int red, int green, int blue, double l, double a, double b)
        {
            Assert.Equal(new CieLabColor(l, a, b),
                CieLabColor.FromXyz(XyzColor.FromRgb(Color.FromArgb(red, green, blue)))
            );
        }

        [Theory]
        [InlineData(100, 0.00526049995830391, -0.010408184525267927, 255, 255, 255)]
        [InlineData(54.31101910924566, 41.78198588920223, 58.46714536715948, 211, 96, 21)]
        public void TestLabToRgb(double l, double a, double b, int red, int green, int blue)
        {
            Assert.Equal(Color.FromArgb(red, green, blue),
                new CieLabColor(l, a, b).ToXyzColor().ToRgbColor()
            );
        }

        [Fact]
        public void TestKmeansPlusPlus()
        {
            var colors = new[]
            {
                Color.FromArgb(255, 255, 255),
                Color.FromArgb(200, 200, 200),
                Color.FromArgb(140, 24, 53),
                Color.FromArgb(0, 0, 0)
            };

            var selected = ColorSelector.KmeansPlusPlus(colors, new RgbSpace(), 2, 0);

            Assert.Equal(new[] { colors[2], colors[3] }, selected[0]);
            Assert.Equal(new[] { colors[0], colors[1] }, selected[1]);
        }

        [Fact]
        public void TestAll()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.image1.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var colors = new ColorSelector().SelectColor(bitmap, 4);
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
