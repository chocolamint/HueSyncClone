using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace HueSyncClone.Core.Drawing
{
    public class ColorSelector
    {
        public void SelectColor(Bitmap bitmap, int count)
        {
            var (thumb, width, height) = Resize(bitmap, 72);
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
