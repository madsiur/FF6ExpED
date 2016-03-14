using FF6exped.Font;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FF6exped.Misc
{
    public class TextEntry
    {
        public byte[] data { get; private set; }
        public string word { get; private set; }
        public WriteableBitmap image { get; set; }

        public TextEntry(byte[] data, SmallFont font, int size)
        {
            this.data = data;
            word = arrayToWord(data);
            image = buildBitmap(data, font, size);
        }

        public TextEntry(string word, SmallFont font, int size)
        {
            this.word = word;
            this.data = worToArray(word);
            image = buildBitmap(data, font, size);
        }

        public TextEntry(string word, WriteableBitmap wBmp, SmallFont font)
        {
            this.word = word;
            this.data = worToArray(word);
            image = buildComposedBitmap(wBmp, data, font);
        }

        public string arrayToWord(Byte[] array)
        {
            string word = "";

            for(int i = 0; i < array.Length; i++)
            {
                word += Tables.Battle[array[i] - 0x80];
            }

            return word;
        }

        public static byte[] worToArray(string word)
        {
            byte[] array = new byte[word.Length];
            bool found;
            int j;

            for(int i = 0; i < word.Length; i++)
            {
                found = false;
                j = 0x00;

                while(!found && j < 0x80)
                {
                    if(Tables.Battle[j].ToString().Equals(word.ElementAt(i).ToString()))
                    {
                        found = true;
                        array[i] = (byte)(j + 0x80);
                    }

                    j++;
                }

                if (!found)
                    array[i] = 0xFF;
            }

            return array;
        }

        private static WriteableBitmap buildBitmap(byte[] data, SmallFont font, int size)
        {
            WriteableBitmap bpm = null;
            int offset = 0;

            bpm = new WriteableBitmap(data.Length * 8, 8, 96, 96, PixelFormats.Pbgra32, null);

            for (int i = 0; i < data.Length; i++)
            {
                SmallFontCharacter source = null;

                source = font.charList[data[i]];
                byte[] buffer = Graphics.getBitmapArray(source.wBmp);
                int stride = Graphics.getBitmapStride(source.wBmp);
                bpm.WritePixels(new Int32Rect(offset, 0, Convert.ToInt16(source.wBmp.PixelWidth), Convert.ToInt16(source.wBmp.PixelHeight)), buffer, stride, 0);
                offset += 8;
            }

            bpm = Graphics.Magnify(bpm, size / 8);
            return bpm;
        }

        public static WriteableBitmap buildComposedBitmap(WriteableBitmap firstBmp, byte[] data, SmallFont font)
        {
            WriteableBitmap bmp = null;
            SmallFontCharacter source;
            int offset = 0;

            bmp = new WriteableBitmap((int)(data.Length * 8 + firstBmp.Width), 8, 96, 96, PixelFormats.Pbgra32, null);

            byte[] buffer = Graphics.getBitmapArray(firstBmp);
            int stride = Graphics.getBitmapStride(firstBmp);
            bmp.WritePixels(new Int32Rect(0, 0, Convert.ToInt16(firstBmp.PixelWidth), Convert.ToInt16(firstBmp.PixelHeight)), buffer, stride, 0);

            for (int i = 0; i < data.Length; i++)
            {
                source = null;
                source = font.charList[data[i]];
                byte[] buffer2 = Graphics.getBitmapArray(source.wBmp);
                int stride2 = Graphics.getBitmapStride(source.wBmp);
                bmp.WritePixels(new Int32Rect(offset + firstBmp.PixelWidth, 0, Convert.ToInt16(source.wBmp.PixelWidth), Convert.ToInt16(source.wBmp.PixelHeight)), buffer2, stride2, 0);
                offset += 8;
            }

            return bmp;
        }

        public class TextEntryList : List<TextEntry> 
        {
            SmallFont font;

            public TextEntryList(Rom rom, int offset, int width, int numElements, SmallFont font, int size)
            {
                this.font = font;
                buildFixedTextList(rom, offset, width, numElements, size);
            }

            private void buildFixedTextList(Rom rom, int offset, int width, int numElements, int size)
            {
                TextEntry tempTE;
                byte[] tempData;

                byte[] data = ByteUtils.GetBytes(rom.Content, offset, width * numElements);

                for(int i = 0; i < data.Length; i += width)
                {
                    tempData = ByteUtils.GetBytes(data, i, width);
                    tempTE = new TextEntry(tempData, font, size);
                    this.Add(tempTE);
                }
            }
        }
    }
}
