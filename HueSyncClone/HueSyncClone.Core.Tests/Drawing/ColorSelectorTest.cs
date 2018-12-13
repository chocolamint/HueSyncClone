using System;
using System.Drawing;
using System.IO;
using HueSyncClone.Core.Drawing;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class ColorSelectorTest
    {
        [Theory]
        [InlineData("image1", 72, 54)]
        [InlineData("image2", 54, 72)]
        public void Test(string imageName, int expectedWidth, int expectedHeight)
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
    }
}
