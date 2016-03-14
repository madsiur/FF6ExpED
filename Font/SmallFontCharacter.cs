using System.Windows.Media;
using System.Windows.Media.Imaging;
using FF6exped.Library.WriteableBitmapExt;

namespace FF6exped.Font
{
    public class SmallFontCharacter
    {

        public byte[] data { get; set; }
        public WriteableBitmap wBmp { get; set; }
        public BitmapPalette pal { get; set; }

        public SmallFontCharacter(byte[] data, BitmapPalette pal)
        {
            this.data = data;
            this.pal = pal;
            createBitmap();
        }

        public void createBitmap()
        {
            wBmp = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, pal);
            byte[] curTile;
            int colIdx;

            unsafe
            {
                for (int j = 0; j < 8; j++)
                {
                    curTile = new byte[2];
                    curTile[0] = data[j * 2];
                    curTile[1] = data[j * 2 + 1];

                    for (int i = 0; i < 8; i++)
                    {
                        colIdx = (curTile[0] >> ((7 - i % 8))) & 1;
                        colIdx |= ((curTile[1] >> ((7 - i % 8))) << 1);
                        colIdx &= 0x03;

                        switch(colIdx)
                        {
                            case 0x00: wBmp.SetPixel(i, j, 0x00, 0x00, 0x80);
                                break;
                            case 0x01: wBmp.SetPixel(i, j, 0x00, 0x00, 0x00);
                                break;
                            case 0x02: wBmp.SetPixel(i, j, 0x80, 0x80, 0x80);
                                break;
                            case 0x03: wBmp.SetPixel(i, j, 0xFF, 0xFF, 0xFF);
                                break;
                        }
                    }
                }
            }
        }
    }
}
