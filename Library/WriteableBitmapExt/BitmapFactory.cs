using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FF6exped.Library.WriteableBitmapExt
{
    /// <summary>
    /// Cross-platform factory for WriteableBitmaps
    /// </summary>
    public static class BitmapFactory
    {
        /// <summary>
        /// Creates a new WriteableBitmap of the specified width and height
        /// </summary>
        /// <remarks>For WPF the default DPI is 96x96 and PixelFormat is Pbgra32</remarks>
        /// <param name="pixelWidth"></param>
        /// <param name="pixelHeight"></param>
        /// <returns></returns>
        public static WriteableBitmap New(int pixelWidth, int pixelHeight)
        {
            return new WriteableBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32, null);
        }

        /// <summary>
        /// Converts the input BitmapSource to the Pbgra32 format WriteableBitmap which is internally used by the WriteableBitmapEx.
        /// </summary>
        /// <param name="source">The source bitmap.</param>
        /// <returns></returns>
        public static WriteableBitmap ConvertToPbgra32Format(BitmapSource source)
        {
            // Convert to Pbgra32 if it's a different format
            if (source.Format == PixelFormats.Pbgra32)
            {
                return new WriteableBitmap(source);
            }

            var formatedBitmapSource = new FormatConvertedBitmap();
            formatedBitmapSource.BeginInit();
            formatedBitmapSource.Source = source;
            formatedBitmapSource.DestinationFormat = PixelFormats.Pbgra32;
            formatedBitmapSource.EndInit();
            return new WriteableBitmap(formatedBitmapSource);
        }
    }
}
