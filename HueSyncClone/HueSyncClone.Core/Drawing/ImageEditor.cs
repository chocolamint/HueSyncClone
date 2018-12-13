using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueSyncClone.Drawing
{
    public class ImageEditor
    {
        public Bitmap Resize(Bitmap bitmap, int size)
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

                var rotateFlip = GetRotateFlip(bitmap);
                resized.RotateFlip(rotateFlip);
                
                return resized;
            }

            RotateFlipType GetRotateFlip(Image image)
            {
                var property = image.PropertyItems.FirstOrDefault(p => p.Id == 0x0112);

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


        public Bitmap[] SliceImage(Bitmap bitmap, int count)
        {
            var width = bitmap.Width / count;
            var height = bitmap.Height;

            var bitmaps = new Bitmap[count];
            for (var i = 0; i < count; i++)
            {
                var slice = new Bitmap(width, height);
                using (var g = Graphics.FromImage(slice))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(width * i, 0, width, height), GraphicsUnit.Pixel);
                }

                bitmaps[i] = slice;
            }
            return bitmaps;
        }
    }
}
