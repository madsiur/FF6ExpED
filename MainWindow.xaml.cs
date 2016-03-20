using FF6exped.Font;
using FF6exped.Misc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FF6exped
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string APPNAME = "FF6 Expansion Editor";
        private int codeOffset = 0x320000;
        private byte[] ARRAY_C25F1E = { 0x20, 0x5A, 0x4B, 0xC9, 0x20 };
        private byte[] ARRAY_C25F2D = { 0xBF, 0x02, 0x30, 0xCF, 0xE2, 0x30 };

        private byte[] CHANGE_C25F1E = { 0x22, 0x00, 0x00, 0xF2, 0x80, 0x0D };
        private byte[] CHANGE_C22C3D = { 0x0A, 0x0A, 0x48, 0x22, 0x2D, 0x00, 0xF2, 0x68, 0xAA, 0x80, 0x01 };
        private byte[] CHANGE_F20000 = { 0xC2, 0x30, 0xB9, 0x01, 0x20, 0x0A, 0x0A, 0x0A, 0x48, 0xE2, 0x30, 0xAD, 0x6D, 0x1F, 0xEE, 0x6D, 0x1F, 0xAA, 0xBF, 0x00, 0xFD, 0xC0,
                                 0xC2, 0x10, 0xFA, 0xC9, 0x40, 0x90, 0x0B, 0xC9, 0x80, 0x90, 0x06, 0xC9, 0xC0, 0x90, 0x01, 0xE8, 0xE8, 0xE8, 0xBF, 0x02, 0x30, 0xCF,
                                 0x6B, 0x0A, 0xAA, 0xBF, 0x00, 0x30, 0xCF, 0x99, 0x08, 0x33, 0x6B };

        private byte firstDrop = 0x40;
        private byte secondDrop = 0x40;
        private byte thirdDrop = 0x40;
        private int dropTableOff;
        byte[] newTable;
        byte[] currentDrops;
        private bool reassign;
        int dropTableOffAbs;
        int codeOffsetAbs;

        private Rom rom = null;
        private SmallFont font;
        private string backupPath = "";

        private Collections collections;

        public MainWindow()
        {
            InitializeComponent();

            TabItemDrops.IsEnabled = false;
            TabItemSteal.IsEnabled = false;
            TabItemConfig.IsEnabled = false;
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.smc";
            dlg.Title = "Open a Final Fantasy 3 US ROM";
            dlg.Filter = "Super Nintendo ROM (*.smc)|*.smc";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    rom = new Rom(dlg.FileName);

                    if (rom.GameCode != "F6  ")
                    {
                        MessageBox.Show("The game code for this ROM is invalid. (game code: " + rom.GameCode + ")", APPNAME);
                    }
                    else if(rom.Version != "1.0")
                    {
                        MessageBox.Show("The game version is not 1.0. (rom version: " +rom.Version + ")", APPNAME);
                    }
                    else if(rom.RomLength != 0x380000 && rom.RomLength != 0x400000 && rom.RomLength != 0x500000 && rom.RomLength != 0x600000)
                    {
                        MessageBox.Show("The ROM is not expanded or ROM size is not conventional. You need at least a 28 MBit ROM. (rom size: 0x" + rom.RomLength.ToString("X6") + ")", APPNAME);
                    }
                    else
                    {
                        // Not working for now

                        /*if(Properties.Settings.Default.UseBackup)
                        {
                            string path = System.IO.Path.GetDirectoryName(dlg.FileName);
                            string backupName = "\\backup" + DateTime.Now.ToString("YYYYMMddhhmmss") + ".smc";
                            backupPath = System.IO.Path.Combine(path, backupName);

                            try
                            {
                                File.Copy(dlg.FileName, backupPath, true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Unable to create backup file. \n \n Exception: " + ex.Message.ToString(), APPNAME);
                            }
                        }*/

                        font = new SmallFont(rom);

                        // For validation
                        byte[] firstArray = ByteUtils.GetBytes(rom.Content, 0x025F1E, 5);
                        byte[] secondArray = ByteUtils.GetBytes(rom.Content, 0x025F2D, 6);


                        // Plan some empty space for future editor expansions
                        dropTableOff = codeOffset + (CHANGE_F20000.Length / 0x100) * 0x100 + 0x100;

                        // If config changes already made
                        if (firstArray[0] == 0x22)
                        {
                            codeOffset = ByteUtils.GetInt24(rom.Content, 0x025F1F);
                            codeOffsetAbs = codeOffset > 0xC00000 ? codeOffset - 0xC00000: codeOffset;

                            if(codeOffsetAbs < 0 || codeOffsetAbs > 0x5FFFFF)
                            {
                                MessageBox.Show("Wrong offset! Modified code cannot be found. (offset found: 0x" + codeOffset.ToString("X6") + ") \n\n The application will now close.", APPNAME);
                                //DeleteBackup();
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                int relocCodeLength = CHANGE_F20000.Length;
                                CHANGE_F20000 = ByteUtils.GetBytes(rom.Content, codeOffsetAbs, relocCodeLength);
                                dropTableOff = ByteUtils.GetInt24(CHANGE_F20000, 48);
                                dropTableOffAbs = dropTableOff > 0xC00000 ? dropTableOff - 0xC00000 : dropTableOff;
                                newTable = ByteUtils.GetBytes(rom.Content, dropTableOffAbs, 0xC00);

                                firstDrop = CHANGE_F20000[0x1A];
                                secondDrop = (byte)(CHANGE_F20000[0x1E] - firstDrop);
                                thirdDrop = (byte)(CHANGE_F20000[0x22] - secondDrop - firstDrop);

                                TabControlDrops.IsEnabled = true;
                                TabItemDrops.IsEnabled = true;
                                TabItemConfig.IsEnabled = true;
                                TbDropCode.IsEnabled = false;
                                TbDropTable.IsEnabled = false;
                                BtnExpand.IsEnabled = false;
                                InitCollections();
                                InitDrops();
                            }                           
                        }
                        // Check if rom data is same as vanilla (beginning and end of future changes)
                        else if(!firstArray.SequenceEqual(ARRAY_C25F1E) || !secondArray.SequenceEqual(ARRAY_C25F2D))
                        {
                            MessageBoxResult result = MessageBox.Show("Problem with code! ASM code from C2/5F1E to C2/C25F32 appears to be different from original code. This code requires to be partially overwritten in order to use the editor. Overwriting unknown/modified code might result in unexpected code behavior." + 
                                                                "\n\n Proceed anyway?", APPNAME, MessageBoxButton.YesNo);

                            if (result == MessageBoxResult.Yes)
                            {
                                TabItemConfig.IsEnabled = true;
                            }
                            else
                            {
                                MessageBox.Show("Application will now close.", APPNAME);
                                DeleteBackup();
                                Application.Current.Shutdown();
                            }
                        }
                        // Data is same as vanilla
                        else
                        {
                            MessageBox.Show("In order to use the editor you need to relocate some data and code in the configuration menu.", APPNAME);
                            TabItemConfig.IsEnabled = true;
                            TabControlDrops.IsEnabled = true;
                            TabControlDrops.SelectedIndex = 2;
                            TbDropTable.IsEnabled = true;
                            TbDropCode.IsEnabled = true;
                            BtnExpand.IsEnabled = true;
                            TabItemDrops.IsEnabled = false;
                        }

                        TbDropTable.Text = dropTableOff < 0x3FFFFF ? (dropTableOff + 0xC00000).ToString("X6") : dropTableOff.ToString("X6");
                        TbDropCode.Text = codeOffset < 0x3FFFFF ? (codeOffset + 0xC00000).ToString("X6") : codeOffset.ToString("X6");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to open ROM. \n \n Exception: " + ex.Message.ToString(), APPNAME);
                }
            }
        }

        private void InitCollections()
        {
            // Initiate the ViewModel
            collections = new Collections(rom, font);
            DataContext = collections;

            // Get Monster 0 drops
            currentDrops = ByteUtils.GetBytes(newTable, 4, 4);

            reassign = true;


            collections.setMonsterNamesList(CmbDropMonster);
            Dispatcher.Invoke(new Action(() => { CmbDropMonster.SelectedIndex = 0; }), DispatcherPriority.Loaded);
            CmbDropMonster.cmb.SelectionChanged += CmbDropMonster_SelectionChanged;
            
            collections.setItemNamesList(CmbDropA);
            Dispatcher.Invoke(new Action(() => { CmbDropA.SelectedIndex = currentDrops[0]; }), DispatcherPriority.Loaded);
            CmbDropA.cmb.SelectionChanged += CmbDropA_SelectionChanged;

            collections.setItemNamesList(CmbDropB);
            Dispatcher.Invoke(new Action(() => { CmbDropB.SelectedIndex = currentDrops[1]; }), DispatcherPriority.Loaded);
            CmbDropB.cmb.SelectionChanged += CmbDropB_SelectionChanged;

            collections.setItemNamesList(CmbDropC);
            Dispatcher.Invoke(new Action(() => { CmbDropC.SelectedIndex = currentDrops[2]; }), DispatcherPriority.Loaded);
            CmbDropC.cmb.SelectionChanged += CmbDropC_SelectionChanged;

            collections.setItemNamesList(CmbDropD);
            Dispatcher.Invoke(new Action(() => { CmbDropD.SelectedIndex = currentDrops[3]; }), DispatcherPriority.Loaded);
            CmbDropD.cmb.SelectionChanged += CmbDropD_SelectionChanged;
        }

        private void InitDrops()
        {
            int dropD = 256 - (firstDrop + secondDrop + thirdDrop);
            TbDropA.Text = ((int)firstDrop).ToString();
            TbDropB.Text = ((int)secondDrop).ToString();
            TbDropC.Text = ((int)thirdDrop).ToString();
            TbDropD.Text = dropD.ToString();

            LblDropA.Content = Math.Ceiling((firstDrop / 256.00) * 100.00).ToString();
            LblDropB.Content = Math.Ceiling((secondDrop / 256.00) * 100.00).ToString();
            LblDropC.Content = Math.Ceiling((thirdDrop / 256.00) * 100.00).ToString();
            LblDropD.Content = Math.Ceiling((dropD / 256.00) * 100.00).ToString();
        }

        private void CmbDropMonster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = CmbDropMonster.SelectedIndex;
            currentDrops = ByteUtils.GetBytes(newTable, CmbDropMonster.SelectedIndex * 8 + 4, 4);

            reassign = false;
            CmbDropA.SelectedIndex = currentDrops[0];
            CmbDropB.SelectedIndex = currentDrops[1];
            CmbDropC.SelectedIndex = currentDrops[2];
            CmbDropD.SelectedIndex = currentDrops[3];
            reassign = true;
        }

        private void CmbDropA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(reassign)
            {
                ByteUtils.SetByte(newTable, CmbDropMonster.SelectedIndex * 8 + 4, (byte)CmbDropA.SelectedIndex);
            }
        }

        private void CmbDropB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbDropMonster.SelectedIndex * 8 + 5, (byte)CmbDropB.SelectedIndex);
            }
        }

        private void CmbDropC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbDropMonster.SelectedIndex * 8 + 6, (byte)CmbDropC.SelectedIndex);
            }
        }

        private void CmbDropD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbDropMonster.SelectedIndex * 8 + 7, (byte)CmbDropD.SelectedIndex);
            }
        }

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(rom != null)
                {
                    ByteUtils.SetByte(CHANGE_F20000, 0x1A, firstDrop);
                    ByteUtils.SetByte(CHANGE_F20000, 0x1E, (byte)(firstDrop + secondDrop));
                    ByteUtils.SetByte(CHANGE_F20000, 0x22, (byte)(firstDrop + secondDrop + thirdDrop));
                    ByteUtils.SetBytes(rom.Content, codeOffsetAbs, CHANGE_F20000);
                    ByteUtils.SetBytes(rom.Content, dropTableOffAbs, newTable);
                    rom.WriteRom();
                }                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save ROM. \n \n Exception: " + ex.Message.ToString(), APPNAME);
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            //DeleteBackup();
            Application.Current.Shutdown();
        }

        private void DeleteBackup()
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to delete backup file. \n \n Exception: " + ex.Message.ToString(), APPNAME);
            }
        }

        private void BtnExpand_Click(object sender, RoutedEventArgs e)
        {
            codeOffset = int.Parse(TbDropCode.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            dropTableOff = int.Parse(TbDropTable.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            if ((dropTableOff >= codeOffset) && (dropTableOff - codeOffset) < 256)
            {
                MessageBox.Show("Please leave at least 0x100 bytes between beginning of code offset and table offset.", APPNAME);
                Keyboard.Focus(TbDropTable);
                TbDropTable.SelectAll();
            }
            else if ((codeOffset >= dropTableOff) && (codeOffset - dropTableOff) < 0xC00)
            {
                MessageBox.Show("Please leave at least 0xC00 bytes between beginning of table offset and code offset.", APPNAME);
                Keyboard.Focus(TbDropCode);
                TbDropCode.SelectAll();
            }
            else
            {
                int origTableOff = ByteUtils.GetInt24(rom.Content, 0x022C42);
                MessageBoxResult dialog = MessageBox.Show("This operation will move the Drop/Steal table to from 0x" + origTableOff.ToString("X6") + " to 0x" + dropTableOff.ToString("X6") + " and new code to 0x" + codeOffset.ToString("X6") + "!\n\nProceed?",
                    APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if(dialog == MessageBoxResult.Yes)
                {
                    origTableOff = origTableOff >= 0xC00000 ? origTableOff - 0xC00000 : origTableOff;
                    dropTableOffAbs = dropTableOff >= 0xC00000 ? dropTableOff - 0xC00000 : dropTableOff;
                    codeOffsetAbs = codeOffset >= 0xC00000 ? codeOffset - 0xC00000 : codeOffset;

                    byte[] originalTable = ByteUtils.GetBytes(rom.Content, origTableOff, 0x600);
                    newTable = new byte[0xC00];

                    for(int i = 0; i < originalTable.Length; i+=2)
                    {
                        newTable[i * 2] = originalTable[i];
                        newTable[i * 2 + 1] = originalTable[i + 1];
                        newTable[i * 2 + 2] = 0xFF;
                        newTable[i * 2 + 3] = 0xFF;
                    }

                    try
                    {
                        ByteUtils.SetInt24(CHANGE_C25F1E, 1, codeOffset);
                        ByteUtils.SetInt24(CHANGE_C22C3D, 4, codeOffset + 0x2D);
                        ByteUtils.SetInt24(CHANGE_F20000, 41, dropTableOff + 0x04);
                        ByteUtils.SetInt24(CHANGE_F20000, 48, dropTableOff);

                        ByteUtils.SetBytes(rom.Content, dropTableOffAbs, newTable);
                        ByteUtils.SetBytes(rom.Content, codeOffsetAbs, CHANGE_F20000);
                        ByteUtils.SetBytes(rom.Content, 0x025F1E, CHANGE_C25F1E);
                        ByteUtils.SetBytes(rom.Content, 0x022C3D, CHANGE_C22C3D);

                        MessageBox.Show("Operation succesfull!", APPNAME);

                        firstDrop = 0x40;
                        secondDrop = 0x40;
                        thirdDrop = 0x40;
                        
                        TabItemDrops.IsEnabled = true;
                        TbDropTable.IsEnabled = false;
                        TbDropCode.IsEnabled = false;
                        BtnExpand.IsEnabled = false;
                        InitCollections();
                        InitDrops();

                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Operation aborted. The application will now close.", APPNAME);
                        Application.Current.Shutdown();
                    }

                }
            }
        }

        private void CkBackup_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.UseBackup = (bool)CkBackup.IsChecked;
        }

        private void TbDropTable_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateOffset(TbDropTable.Text, 0xC00);

            if (value == -1)
            {
                TbDropTable.Text = dropTableOff < 0x400000 ? (dropTableOff + 0xC00000).ToString("X6"): dropTableOff.ToString("X6");
            }
            else
            {
                dropTableOff = ((value / 256) * 256);
                TbDropTable.Text = dropTableOff.ToString("X6");
            }
        }

        private void TbDropCode_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateOffset(TbDropCode.Text, 0x100);

            if(value == -1)
            {
                TbDropCode.Text = codeOffset < 0x400000 ? (codeOffset + 0xC00000).ToString("X6") : codeOffset.ToString("X6");
            }
            else
            {
                codeOffset = ((value / 256) * 256);
                TbDropCode.Text = codeOffset.ToString("X6");
            }
        }

        private int ValidateOffset(string value, int borderValue)
        {
            int offset;

            if (!int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offset))
            {
                MessageBox.Show("Noob! This is not a hexadecimal number !", APPNAME);
                offset = -1;
            }
            else
            {
                int absOffset = offset >= 0xC00000 ? offset - 0xC00000 : offset;
                if(offset > 0x5FFFFF && offset < 0xC00000)
                {
                    MessageBox.Show("Noob! Invalid offset !", APPNAME);
                    offset = -1;
                }
                else if(absOffset > (rom.RomLength - borderValue))
                {
                    MessageBox.Show("Noob! Invalid offset! Data would overflow your ROM !", APPNAME);
                    offset = -1;
                }
                else if ((offset >= 0x020000 && offset <= 0x02FFFF) || (offset >= 0xC20000 && offset <= 0xC2FFFF))
                {
                    MessageBox.Show("Error! Offset must be into another bank than $C2 due to the use of JSL instructions.", APPNAME);
                    offset = -1;
                }
                else
                {
                    offset = offset < 0x400000 ? offset + 0xC00000 : offset;
                }
            }

            return offset;
        }

        private void BtnDropMonsForward_Click(object sender, RoutedEventArgs e)
        {
            if (CmbDropMonster.SelectedIndex == CmbDropMonster.cmb.Items.Count - 1)
            {
                CmbDropMonster.SelectedIndex = 0;
            }
            else
            {
                CmbDropMonster.SelectedIndex++;
            }
        }

        private void BtnDropMonsBack_Click(object sender, RoutedEventArgs e)
        {
            if (CmbDropMonster.SelectedIndex == 0)
            {
                CmbDropMonster.SelectedIndex = CmbDropMonster.cmb.Items.Count - 1;
            }
            else
            {
                CmbDropMonster.SelectedIndex--;
            }
        }

        private void TbDropA_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateDrop(TbDropA.Text);

            if(value == -1)
            {
                TbDropA.Text = ((int)firstDrop).ToString();
            }
            else if(!ValidateTotal())
            {
                TbDropA.Text = ((int)firstDrop).ToString();
            }
            else
            {
                firstDrop = (byte)value;
            }
        }

        private void TbDropB_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateDrop(TbDropB.Text);

            if (value == -1)
            {
                TbDropB.Text = ((int)secondDrop).ToString();
            }
            else if (!ValidateTotal())
            {
                TbDropB.Text = ((int)secondDrop).ToString();
            }
            else
            {
                secondDrop = (byte)value;
            }
        }

        private void TbDropC_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateDrop(TbDropC.Text);

            if (value == -1)
            {
                TbDropC.Text = ((int)thirdDrop).ToString();
            }
            else if (!ValidateTotal())
            {
                TbDropC.Text = ((int)thirdDrop).ToString();
            }
            else
            {
                thirdDrop = (byte)value;
            }
        }

        private bool ValidateTotal()
        {
            bool binOk = false;

            int dropA = int.Parse(TbDropA.Text);
            int dropB = int.Parse(TbDropB.Text);
            int dropC = int.Parse(TbDropC.Text);
            int total = dropA + dropB + dropC;
            int dropD = 256 - total;

            if (total <= 255)
            {
                binOk = true;
                TbDropD.Text = dropD.ToString();

                LblDropA.Content = Math.Ceiling((dropA / 256.00) * 100).ToString();
                LblDropB.Content = Math.Ceiling((dropB / 256.00) * 100).ToString();
                LblDropC.Content = Math.Ceiling((dropC / 256.00) * 100).ToString();
                LblDropD.Content = Math.Ceiling((dropD / 256.00) * 100).ToString();
            }
            else
            {
                MessageBox.Show("Error! Total of three first drop chances must not exceed 255 !", APPNAME);
            }

            return binOk;
        }

        private int ValidateDrop(string text)
        {
            int value;

            if(!int.TryParse(text, out value))
            {
                MessageBox.Show("Noob! '" + text + "' is not a number!", APPNAME);
                value = -1;
            }
            else if (value < 0 || value > 256)
            {
                MessageBox.Show("Noob! Value cannot be lower than 0 or higher than 256!", APPNAME);
                value = -1;
            }

            return value;
        }
    }
}
