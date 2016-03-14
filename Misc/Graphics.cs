using FF6exped.Library.WriteableBitmapExt;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FF6exped.Misc
{
    public static class Graphics
    {
        public static int getBitmapStride(WriteableBitmap source)
        {
            int stride = -1;

            Action action = () =>
            {
                stride = source.PixelWidth * (source.Format.BitsPerPixel / 8);
            };

            Dispatcher.CurrentDispatcher.Invoke(action);

            return stride;
        }

        public static byte[] getBitmapArray(WriteableBitmap source)
        {
            byte[] data = null;

            int stride = getBitmapStride(source);
            data = new byte[stride * source.PixelHeight];
            source.CopyPixels(data, stride, 0);

            return data;
        }

        public static WriteableBitmap Magnify(WriteableBitmap wBmp, int factor)
        {
            WriteableBitmap b = wBmp;
            return b.Resize(wBmp.PixelWidth * factor, wBmp.PixelHeight * factor, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
        }
    }
}
