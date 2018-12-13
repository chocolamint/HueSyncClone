using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class ImageEditorTest
    {
        [Theory]
        [InlineData("image1", Skip = "")]
        public void TestSliceImage(string imageName)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                var slices = new ImageEditor().SliceImage(bitmap, 4);
                foreach (var (slice, index) in slices.Select((x, i) => (x, i)))
                {
                    using (slice)
                    {
                        slice.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{imageName}_{index}.jpg"));
                    }
                }
            }
        }

        [Theory]
        [InlineData("image1", Skip = "")]
        [InlineData("image2", Skip = "")]
        public void TestResize(string imageName)
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream($"HueSyncClone.Images.{imageName}.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            {
                using (var thumb = new ImageEditor().Resize(bitmap, 72))
                {
                    thumb.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{imageName}.jpg"));
                }
            }
        }
    }
}
