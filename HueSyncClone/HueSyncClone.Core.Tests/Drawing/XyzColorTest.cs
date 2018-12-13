using System.Drawing;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class XyzColorTest
    {
        [Theory]
        [InlineData(new[] { 255, 255, 255 }, new[] { 0.9505, 1, 1.089 })]
        [InlineData(new[] { 211, 96, 21 }, new[] { 0.3118, 0.2227, 0.0336})]
        [InlineData(new[] { 48, 225, 112 }, new[] { 0.3107, 0.5565, 0.2443})]
        [InlineData(new[] { 0, 0, 0 }, new[] { 0.0, 0.0, 0.0 })]
        public void RgbToXyz(int[] rgb, double[] xyz)
        {
            var rgbColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);

            Assert.Equal(
                new XyzColor(xyz[0], xyz[1], xyz[2]),
                XyzColor.FromRgb(rgbColor)
            );
        }

        [Theory]
        [InlineData(new[] { 255, 255, 255 }, new[] { 0.9505, 1, 1.089 })]
        [InlineData(new[] { 211, 96, 21 }, new[] { 0.3118, 0.2227, 0.0336 })]
        [InlineData(new[] { 48, 225, 112 }, new[] { 0.3107, 0.5565, 0.2443 })]
        [InlineData(new[] { 0, 0, 0 }, new[] { 0.0, 0.0, 0.0 })]
        public void XyzToRgb(int[] rgb, double[] xyz)
        {
            var rgbColor = Color.FromArgb(rgb[0], rgb[1], rgb[2]);

            Assert.Equal(
                rgbColor,
                new XyzColor(xyz[0], xyz[1], xyz[2]).ToRgbColor()
            );
        }
    }
}
