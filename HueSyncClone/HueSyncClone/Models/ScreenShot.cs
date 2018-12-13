using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HueSyncClone.Models
{
    public class ScreenShot
    {
        private const int SRCCOPY = 13369376;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

        /// <summary>
        /// プライマリスクリーンの画像を取得する
        /// </summary>
        /// <returns>プライマリスクリーンの画像</returns>
        public static Bitmap CaptureScreen()
        {
            var disDC = GetDC(IntPtr.Zero);
            try
            {
                var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                var g = Graphics.FromImage(bmp);
                try
                {
                    var hDC = g.GetHdc();
                    try
                    {
                        BitBlt(hDC, 0, 0, bmp.Width, bmp.Height, disDC, 0, 0, SRCCOPY);
                    }
                    finally
                    {
                        g.ReleaseHdc(hDC);
                    }
                }
                finally
                {
                    g.Dispose();
                }

                return bmp;
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, disDC);
            }
        }
    }
}
