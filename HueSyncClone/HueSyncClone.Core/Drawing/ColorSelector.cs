using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;

namespace HueSyncClone.Core.Drawing
{
    public class ColorSelector
    {
        public Bitmap SelectColor(Bitmap bitmap, int count)
        {
            return Resize(bitmap, 72);
        }

        private static Bitmap Resize(Bitmap bitmap, int size)
        {
            var originalWidth = bitmap.Width;
            var originalHeight = bitmap.Height;

            if (originalWidth <= size && originalHeight <= size) return bitmap;

            var resizedWidth = originalWidth > originalHeight ? size : originalWidth * size / originalHeight;
            var resizedHeight = originalHeight > originalWidth ? size : originalHeight * size / originalWidth;
            var resized = new Bitmap(resizedWidth, resizedHeight);

            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, resizedWidth, resizedHeight);

                return resized;
            }
        }
    }
}
