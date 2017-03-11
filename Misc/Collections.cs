using FF6exped.CustomControls.CustomComboBox;
using FF6exped.Font;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF6exped.Misc
{
    class Collections
    {
        private ObservableCollection<ComboBoxInfo> monsterNames;
        private ObservableCollection<ComboBoxInfo> itemNames;

        public TextEntry.TextEntryList monsterList;
        private TextEntry.TextEntryList ItemList;
        private Rom rom;
        private SmallFont font;

        public Collections(Rom rom, SmallFont font)
        {
            this.rom = rom;
            this.font = font;
            monsterNames = new ObservableCollection<ComboBoxInfo>();
            itemNames = new ObservableCollection<ComboBoxInfo>();
            createMonsterNameList();
            createItemNameList();
        }

        public void OnWindowClosing()
        {
            monsterNames.Clear();
            itemNames.Clear();

            ItemList.Clear();
            monsterList.Clear();

            monsterNames = null;
            itemNames = null;

            monsterList = null;
            ItemList = null;
        }

        public void setMonsterNamesList(CustomComboBox cmb)
        {
            cmb.ItemSource = monsterNames;
        }

        public void setItemNamesList(CustomComboBox cmb)
        {
            cmb.ItemSource = itemNames;
        }

        private void createMonsterNameList()
        {
            monsterList = new TextEntry.TextEntryList(rom, 0x0FC050, 10, 384, font, 8);

            for (int i = 0; i < 384; i++)
            {
                monsterNames.Add(new ComboBoxInfo { ImageBitmap = monsterList.ElementAt(i).image });
            }
            
        }

        private void createItemNameList()
        {
            ItemList = new TextEntry.TextEntryList(rom, 0x12B300, 13, 255, font, 8);

            for (int i = 0; i < 255; i++)
            {
                itemNames.Add(new ComboBoxInfo { ImageBitmap = ItemList.ElementAt(i).image });
            }

            ItemList.Add(new TextEntry(" Nothing", font, 8));
            itemNames.Add(new ComboBoxInfo { ImageBitmap = ItemList.ElementAt(255).image });
        }
    }
}
