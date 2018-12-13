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
    }
}
