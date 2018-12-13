using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HueSyncClone.Drawing
{
    public class ImageSlicer
    {
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
