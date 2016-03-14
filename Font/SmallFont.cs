using FF6exped.Misc;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FF6exped.Font
{
    public class SmallFont
    {
        public List<SmallFontCharacter> charList { get; set; }
        public BitmapPalette pal { get; private set; }

        public SmallFont(Rom rom)
        {
            charList = new List<SmallFontCharacter>();
            initializeElements();
            fillList(rom);
        }

        public void initializeElements()
        {
            Color blue = Color.FromArgb(0x00, 0x00, 0x00, 0x80);
            Color white = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF);
            Color black = Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            Color grey = Color.FromArgb(0x00, 0x80, 0x80, 0x80);

            Color[] colorList = new Color[4];
            colorList[0] = blue;
            colorList[1] = white;
            colorList[2] = black;
            colorList[3] = grey;
            pal = new BitmapPalette(colorList);
        }

        public void fillList(Rom rom)
        {
            byte[] data;
            SmallFontCharacter c;
            string a;

            for(int i = 0; i < 256; i++)
            {
                if(i == 0x81)
                    a  = i.ToString();

                data = ByteUtils.GetBytes(rom.Content, 0x047FC0 + (i * 16), 16);
                c = new SmallFontCharacter(data, pal);
                charList.Add(c);
            }
        }
    }
}
