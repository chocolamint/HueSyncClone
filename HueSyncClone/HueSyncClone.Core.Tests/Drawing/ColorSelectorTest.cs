using System;
using System.Drawing;
using System.IO;
using HueSyncClone.Core.Drawing;
using Xunit;

namespace HueSyncClone.Drawing
{
    public class ColorSelectorTest
    {
        [Fact]
        public void Test()
        {
            var selector = new ColorSelector();
            using (var stream = GetType().Assembly.GetManifestResourceStream("HueSyncClone.Images.image1.jpg"))
            using (var bitmap = (Bitmap)Image.FromStream(stream))
            using (var thumb = selector.SelectColor(bitmap, 0))
            {
                thumb.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "test.jpg"));
            }
        }
    }
}
