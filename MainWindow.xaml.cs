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

        // Vanilla arrays
        private byte[] ARRAY_C25F1E = { 0x20, 0x5A, 0x4B, 0xC9, 0x20, 0xC2 };
        private byte[] ARRAY_C22C3F = { 0x48, 0xAA, 0xBF, 0x00, 0x30, 0xCF, 0x99, 0x08, 0x33 };

        // 0.1.1 version changes
        private byte[] ARRAY2_C22C3F = { 0x48, 0x22, 0x2D, 0x00, 0xF2, 0x68, 0xAA, 0x80, 0x01 };

        // Vanilla arrays (steal)
        private byte[] ARRAY_C239AA = { 0xC2, 0x20, 0xB9, 0x08, 0x33, 0x1A, 0xE2, 0x21, 0xF0 };
        private byte[] ARRAY_C239B4 = { 0xEE, 0x01, 0x34, 0xBD, 0x18, 0x3B };
        private byte[] ARRAY_C239D8 = { 0x5A, 0x20, 0x5A, 0x4B, 0xC9, 0x20, 0x90, 0x01, 0xC8, 0xB9 };
        private byte[] ARRAY_C239F5 = { 0xA9, 0xFF, 0x99, 0x08, 0x33, 0x99 };

        // 0.1.1 version code
        private byte[] CHANGE_OLD_F20000 = { 0xC2, 0x30, 0xB9, 0x01, 0x20, 0x0A, 0x0A, 0x0A, 0x48, 0xE2, 0x30, 0xAD, 0x6D, 0x1F, 0xEE, 0x6D,
                                             0x1F, 0xAA, 0xBF, 0x00, 0xFD, 0xC0, 0xC2, 0x10, 0xFA, 0xC9, 0x40, 0x90, 0x0B, 0xC9, 0x80, 0x90,
                                             0x06, 0xC9, 0xC0, 0x90, 0x01, 0xE8, 0xE8, 0xE8, 0xBF, 0x02, 0x30, 0xCF, 0x6B, 0x0A, 0xAA, 0xBF,
                                             0x00, 0x30, 0xCF, 0x99, 0x08, 0x33, 0x6B };

        // Changes (current version)
        private byte[] CHANGE_C25F1E = { 0x22, 0x00, 0x00, 0xF2, 0x80, 0x0D };
        private byte[] CHANGE_C22C3F = { 0xAA, 0x0A, 0x99, 0x08, 0x33, 0x48, 0xDA, 0x80, 0x04 };
        private byte[] CHANGE_C239AA = { 0x22, 0x31, 0x00, 0xF2, 0xF0, 0x53, 0x22, 0x38, 0x00, 0xF2, 0xF0, 0x4D, 0x22, 0x55, 0x00, 0xF2 };
        private byte[] CHANGE_C239D8 = { 0xDA, 0x22, 0x5C, 0x00, 0xF2, 0xE2, 0x10, 0xFA, 0x80, 0x03 };
        private byte[] CHANGE_C239F5 = { 0x22, 0x7F, 0x00, 0xF2, 0x80, 0x02 };
        private byte[] CHANGE_F20000 = { 0xC2, 0x30, 0xB9, 0x01, 0x20, 0x0A, 0x0A, 0x0A, 0x48, 0x20, 0x23, 0x00, 0xC2, 0x10, 0xFA, 0xC9, // 0xF20000
                                         0x40, 0x90, 0x0B, 0xC9, 0x80, 0x90, 0x06, 0xC9, 0xC0, 0x90, 0x01, 0xE8, 0xE8, 0xE8, 0xBF, 0x04, // 0xF20010
                                         0x01, 0xF2, 0x6B, 0xE2, 0x20, 0xAD, 0x6D, 0x1F, 0xEE, 0x6D, 0x1F, 0xAA, 0xBF, 0x00, 0xFD, 0xC0, // 0xF20020
                                         0x60, 0xC2, 0x30, 0xB9, 0x08, 0x33, 0x1A, 0x6B, 0xDA, 0x3A, 0xAA, 0xBF, 0x00, 0x01, 0xF2, 0x1A, // 0xF20030
                                         0xE2, 0x21, 0xD0, 0x0B, 0xC2, 0x20, 0xBF, 0x02, 0x01, 0xF2, 0xFA, 0x1A, 0xE2, 0x31, 0x6B, 0xFA, // 0xF20040
                                         0xE2, 0x10, 0x6B, 0xEA, 0xEA, 0xEE, 0x01, 0x34, 0xBD, 0x18, 0x3B, 0x6B, 0x20, 0x23, 0x00, 0x48, // 0xF20050
                                         0xC2, 0x30, 0xB9, 0x08, 0x33, 0xAA, 0xE2, 0x20, 0x68, 0xC9, 0x40, 0x90, 0x0B, 0xC9, 0x80, 0x90, // 0xF20060
                                         0x06, 0xC9, 0xC0, 0x90, 0x01, 0xE8, 0xE8, 0xE8, 0xBF, 0x00, 0x01, 0xF2, 0x6B, 0x48, 0xC2, 0x20,
                                         0xA9, 0xFF, 0xFF, 0x99, 0x08, 0x33, 0xE2, 0x20, 0x68, 0x6B };

        // Drop rates
        private byte dropLvl1 = 0x40;
        private byte dropLvl2 = 0x40;
        private byte dropLvl3 = 0x40;

        // Steal rates
        private byte stealLvl1 = 0x40;
        private byte stealLvl2 = 0x40;
        private byte stealLvl3 = 0x40;

        // offsets ($F20000 code and drop/steal table)
        private int dropTableOff;
        private int dropTableOffAbs;
        private int codeOffsetAbs;

        // modified rates
        private byte[] currentDrops;
        private byte[] currentSteals;

        // new drop/steal table
        private byte[] newTable;

        // prevent code from running twice
        private bool reassign;

        private bool fromOldVersion = false;
        
        // Rom object
        private Rom rom = null;

        // Font (ComboBoxes)
        private SmallFont font;

        // Used for backup features (not active)
        private string backupPath = "";

        private Collections collections;

        public MainWindow()
        {
            InitializeComponent();

            TabItemDrops.IsEnabled = false;
            TabItemSteal.IsEnabled = false;
            TabItemConfig.IsEnabled = false;
        }

        #region ROM Opening

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
                        byte[] valArray2C3F = ByteUtils.GetBytes(rom.Content, 0x022C3F, 9);
                        byte[] valArray5F1E = ByteUtils.GetBytes(rom.Content, 0x025F1E, 6);
                        byte[] valArray39AA = ByteUtils.GetBytes(rom.Content, 0x0239AA, 9);
                        byte[] valArray39B4 = ByteUtils.GetBytes(rom.Content, 0x0239B4, 6);
                        byte[] valArray39D8 = ByteUtils.GetBytes(rom.Content, 0x0239D8, 10);
                        byte[] valArray39F5 = ByteUtils.GetBytes(rom.Content, 0x0239F5, 6);


                        // Plan some empty space for future editor expansions
                        dropTableOff = codeOffset + (CHANGE_F20000.Length / 0x100) * 0x100 + 0x100;

                        // if expansion already made
                        if(CHANGE_C22C3F.SequenceEqual(valArray2C3F))
                        {
                            codeOffset = ByteUtils.GetInt24(rom.Content, 0x025F1F);
                            codeOffsetAbs = codeOffset > 0xC00000 ? codeOffset - 0xC00000 : codeOffset;

                            if (codeOffsetAbs < 0 || codeOffsetAbs > 0x5FFFFF)
                            {
                                MessageBox.Show("Wrong offset! Modified code cannot be found. (offset found: 0x" + codeOffset.ToString("X6") + ") \n\n The application will now close.", APPNAME);
                                //DeleteBackup();
                                Application.Current.Shutdown();
                            }
                            else
                            {
                                int relocCodeLength = CHANGE_F20000.Length;
                                CHANGE_F20000 = ByteUtils.GetBytes(rom.Content, codeOffsetAbs, relocCodeLength);
                                dropTableOff = ByteUtils.GetInt24(CHANGE_F20000, 0x3C);
                                dropTableOffAbs = dropTableOff > 0xC00000 ? dropTableOff - 0xC00000 : dropTableOff;
                                newTable = ByteUtils.GetBytes(rom.Content, dropTableOffAbs, 0xC00);

                                if (dropTableOffAbs < 0 || dropTableOffAbs > 0x5FFFFF)
                                {
                                    MessageBox.Show("Wrong offset! Modified table cannot be found. (offset found: 0x" + dropTableOff.ToString("X6") + ") \n\n The application will now close.", APPNAME);
                                    //DeleteBackup();
                                    Application.Current.Shutdown();
                                }

                                dropLvl1 = CHANGE_F20000[0x10];
                                dropLvl2 = (byte)(CHANGE_F20000[0x14] - dropLvl1);
                                dropLvl3 = (byte)(CHANGE_F20000[0x18] - dropLvl2 - dropLvl1);

                                stealLvl1 = CHANGE_F20000[0x6A];
                                stealLvl2 = (byte)(CHANGE_F20000[0x6E] - stealLvl1);
                                stealLvl3 = (byte)(CHANGE_F20000[0x72] - stealLvl2 - stealLvl1);

                                InitCollections();
                                InitDrops();
                                InitSteals();

                                TabControlDrops.IsEnabled = true;
                                TabItemSteal.IsEnabled = true;
                                TabItemDrops.IsEnabled = true;
                                TabItemConfig.IsEnabled = true;
                                TbDropCode.IsEnabled = false;
                                TbDropTable.IsEnabled = false;
                                BtnExpand.IsEnabled = false;

                            }
                        }
                        // If we come from version 0.1.1 and changes are not made
                        else if (valArray2C3F[1] == 0x22)
                        {
                            bool isValid = true;
                            string beginOff = "";
                            string endOff = "";

                            if (!ARRAY2_C22C3F.SequenceEqual(valArray2C3F))
                            {
                                beginOff = "C2/2C3F";
                                endOff = "C2/2C48";
                                isValid = false;
                            }

                            if (!CHANGE_C25F1E.SequenceEqual(valArray5F1E))
                            {
                                beginOff = "C2/5F1E";
                                endOff = "C2/5F23";
                                isValid = false;
                            }

                            if (!ARRAY_C239AA.SequenceEqual(valArray39AA))
                            {
                                beginOff = "C2/39AA";
                                endOff = "C2/39B2";
                                isValid = false;
                            }

                            if (!ARRAY_C239B4.SequenceEqual(valArray39B4))
                            {
                                beginOff = "C2/39B4";
                                endOff = "C2/39B9";
                                isValid = false;
                            }

                            if (!ARRAY_C239D8.SequenceEqual(valArray39D8))
                            {
                                beginOff = "C2/39D8";
                                endOff = "C2/39E1";
                                isValid = false;
                            }

                            /*if (!ARRAY_C239F5.SequenceEqual(valArray39F5))
                            {
                                beginOff = "C2/39F5";
                                endOff = "C2/39FC";
                                isValid = false;
                            }*/

                            if (!isValid)
                            {
                                MessageBoxResult result = MessageBox.Show(String.Format("Problem with code! ASM code from {0} to {1} appears to be different from original code. This code requires to be partially overwritten in order to use the editor. Overwriting unknown/modified code might result in unexpected code behavior." +
                                                                "\n\n Proceed anyway?", beginOff, endOff), APPNAME, MessageBoxButton.YesNo);

                                if (result == MessageBoxResult.No)
                                {
                                    MessageBox.Show("Application will now close.", APPNAME);
                                    //DeleteBackup();
                                    Application.Current.Shutdown();
                                }
                                else
                                {
                                    isValid = true;
                                }
                            }

                            if(isValid)
                            {
                                codeOffset = ByteUtils.GetInt24(rom.Content, 0x025F1F);
                                codeOffsetAbs = codeOffset > 0xC00000 ? codeOffset - 0xC00000 : codeOffset;

                                if (codeOffsetAbs < 0 || codeOffsetAbs > 0x5FFFFF)
                                {
                                    MessageBox.Show("Wrong offset! Modified code cannot be found. (offset found: 0x" + codeOffset.ToString("X6") + ") \n\n The application will now close.", APPNAME);
                                    //DeleteBackup();
                                    Application.Current.Shutdown();
                                }
                                else
                                {
                                    int relocCodeLength = CHANGE_OLD_F20000.Length;
                                    CHANGE_OLD_F20000 = ByteUtils.GetBytes(rom.Content, codeOffsetAbs, relocCodeLength);
                                    dropTableOff = ByteUtils.GetInt24(CHANGE_OLD_F20000, 0x30);
                                    dropTableOffAbs = dropTableOff > 0xC00000 ? dropTableOff - 0xC00000 : dropTableOff;
                                    newTable = ByteUtils.GetBytes(rom.Content, dropTableOffAbs, 0xC00);

                                    if (dropTableOffAbs < 0 || dropTableOffAbs > 0x5FFFFF)
                                    {
                                        MessageBox.Show("Wrong offset! Modified table cannot be found. (offset found: 0x" + dropTableOff.ToString("X6") + ") \n\n The application will now close.", APPNAME);
                                        //DeleteBackup();
                                        Application.Current.Shutdown();
                                    }

                                    dropLvl1 = CHANGE_OLD_F20000[0x1A];
                                    dropLvl2 = (byte)(CHANGE_OLD_F20000[0x1E] - dropLvl1);
                                    dropLvl3 = (byte)(CHANGE_OLD_F20000[0x22] - dropLvl2 - dropLvl1);

                                    TabControlDrops.IsEnabled = true;
                                    TabItemSteal.IsEnabled = false;
                                    TabItemDrops.IsEnabled = false;
                                    TabItemConfig.IsEnabled = true;
                                    TbDropCode.IsEnabled = false;
                                    TbDropTable.IsEnabled = false;
                                    BtnExpand.IsEnabled = true;

                                    fromOldVersion = true;

                                    MessageBox.Show("This is your first time using version 0.2 of the editor. In order to make steal items editable you must finish the expansion by clicking the 'Expand' button in the configuration menu.", APPNAME);
                                    TabControlDrops.SelectedIndex = 2;

                                }
                            }
                        }
                        // Check if rom data is same as vanilla
                        else if(ARRAY_C25F1E.SequenceEqual(valArray5F1E))
                        {
                            bool isValid = true;
                            string beginOff = "";
                            string endOff = "";

                            if (!ARRAY_C22C3F.SequenceEqual(valArray2C3F))
                            {
                                beginOff = "C2/2C3F";
                                endOff = "C2/2C48";
                                isValid = false;
                            }

                            if (!ARRAY_C239AA.SequenceEqual(valArray39AA))
                            {
                                beginOff = "C2/39AA";
                                endOff = "C2/39B2";
                                isValid = false;
                            }

                            if (!ARRAY_C239B4.SequenceEqual(valArray39B4))
                            {
                                beginOff = "C2/39B4";
                                endOff = "C2/39B9";
                                isValid = false;
                            }

                            if (!ARRAY_C239D8.SequenceEqual(valArray39D8))
                            {
                                beginOff = "C2/39D8";
                                endOff = "C2/39E1";
                                isValid = false;
                            }

                            if (!ARRAY_C239F5.SequenceEqual(valArray39F5))
                            {
                                beginOff = "C2/39F5";
                                endOff = "C2/39FC";
                                isValid = false;
                            }

                            if (!isValid)
                            {
                                MessageBoxResult result = MessageBox.Show(String.Format("Problem with code! ASM code from {0} to {1} appears to be different from original code. This code requires to be partially overwritten in order to use the editor. Overwriting unknown/modified code might result in unexpected code behavior." +
                                                                "\n\n Proceed anyway?", beginOff, endOff), APPNAME, MessageBoxButton.YesNo);

                                if (result == MessageBoxResult.No)
                                {
                                    MessageBox.Show("Application will now close.", APPNAME);
                                    //DeleteBackup();
                                    Application.Current.Shutdown();
                                }
                                else
                                {
                                    isValid = true;
                                }
                            }

                            if (isValid)
                            {
                                MessageBox.Show("In order to use the editor you need to relocate some data and code in the configuration menu.", APPNAME);
                                TabItemConfig.IsEnabled = true;
                                TabControlDrops.IsEnabled = true;
                                TabControlDrops.SelectedIndex = 2;
                                TbDropTable.IsEnabled = true;
                                TbDropCode.IsEnabled = true;
                                BtnExpand.IsEnabled = true;
                                TabItemDrops.IsEnabled = false;
                                TabItemSteal.IsEnabled = false;
                            }

                        }
                        // else, we can't work with the ROM.
                        else
                        {
                            MessageBox.Show("The editor cannot work with your ROM. The relevant code is not the vanilla one or is not the modified code from version 0.1.1 or 0.2.\n\nApplication will now close.", APPNAME);
                            //DeleteBackup();
                            Application.Current.Shutdown();
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

        #endregion

        #region Init

        private void InitCollections()
        {
            // Initiate the ViewModel
            collections = new Collections(rom, font);
            DataContext = collections;

            // Get Monster 0 drops
            currentDrops = ByteUtils.GetBytes(newTable, 4, 4);
            currentSteals = ByteUtils.GetBytes(newTable, 0, 4);

            reassign = true;

            // Drop Editor
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

            // Steal Editor
            collections.setMonsterNamesList(CmbStealMonster);
            Dispatcher.Invoke(new Action(() => { CmbStealMonster.SelectedIndex = 0; }), DispatcherPriority.Loaded);
            CmbStealMonster.cmb.SelectionChanged += CmbStealMonster_SelectionChanged;

            collections.setItemNamesList(CmbStealA);
            Dispatcher.Invoke(new Action(() => { CmbStealA.SelectedIndex = currentSteals[0]; }), DispatcherPriority.Loaded);
            CmbStealA.cmb.SelectionChanged += CmbStealA_SelectionChanged;

            collections.setItemNamesList(CmbStealB);
            Dispatcher.Invoke(new Action(() => { CmbStealB.SelectedIndex = currentSteals[1]; }), DispatcherPriority.Loaded);
            CmbStealB.cmb.SelectionChanged += CmbStealB_SelectionChanged;

            collections.setItemNamesList(CmbStealC);
            Dispatcher.Invoke(new Action(() => { CmbStealC.SelectedIndex = currentSteals[2]; }), DispatcherPriority.Loaded);
            CmbStealC.cmb.SelectionChanged += CmbStealC_SelectionChanged;

            collections.setItemNamesList(CmbStealD);
            Dispatcher.Invoke(new Action(() => { CmbStealD.SelectedIndex = currentSteals[3]; }), DispatcherPriority.Loaded);
            CmbStealD.cmb.SelectionChanged += CmbStealD_SelectionChanged;
        }

        #endregion

        #region Drop Editor Controls

        private void InitDrops()
        {
            int dropD = 256 - (dropLvl1 + dropLvl2 + dropLvl3);
            TbDropA.Text = ((int)dropLvl1).ToString();
            TbDropB.Text = ((int)dropLvl2).ToString();
            TbDropC.Text = ((int)dropLvl3).ToString();
            TbDropD.Text = dropD.ToString();

            LblDropA.Content = Math.Ceiling((dropLvl1 / 256.00) * 100.00).ToString();
            LblDropB.Content = Math.Ceiling((dropLvl2 / 256.00) * 100.00).ToString();
            LblDropC.Content = Math.Ceiling((dropLvl3 / 256.00) * 100.00).ToString();
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

        #endregion

        #region Steal Editor Controls

        private void InitSteals()
        {
            int stealD = 256 - (stealLvl1 + stealLvl2 + stealLvl3);
            TbStealA.Text = ((int)stealLvl1).ToString();
            TbStealB.Text = ((int)stealLvl2).ToString();
            TbStealC.Text = ((int)stealLvl3).ToString();
            TbStealD.Text = stealD.ToString();

            LblStealA.Content = Math.Ceiling((stealLvl1 / 256.00) * 100.00).ToString();
            LblStealB.Content = Math.Ceiling((stealLvl2 / 256.00) * 100.00).ToString();
            LblStealC.Content = Math.Ceiling((stealLvl3 / 256.00) * 100.00).ToString();
            LblStealD.Content = Math.Ceiling((stealD / 256.00) * 100.00).ToString();
        }

        private void CmbStealMonster_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = CmbStealMonster.SelectedIndex;
            currentSteals = ByteUtils.GetBytes(newTable, CmbStealMonster.SelectedIndex * 8, 4);

            reassign = false;
            CmbStealA.SelectedIndex = currentSteals[0];
            CmbStealB.SelectedIndex = currentSteals[1];
            CmbStealC.SelectedIndex = currentSteals[2];
            CmbStealD.SelectedIndex = currentSteals[3];
            reassign = true;
        }

        private void CmbStealA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbStealMonster.SelectedIndex * 8, (byte)CmbStealA.SelectedIndex);
            }
        }

        private void CmbStealB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbStealMonster.SelectedIndex * 8 + 1, (byte)CmbStealB.SelectedIndex);
            }
        }

        private void CmbStealC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbStealMonster.SelectedIndex * 8 + 2, (byte)CmbStealC.SelectedIndex);
            }
        }

        private void CmbStealD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (reassign)
            {
                ByteUtils.SetByte(newTable, CmbStealMonster.SelectedIndex * 8 + 3, (byte)CmbStealD.SelectedIndex);
            }
        }

        #endregion

        #region Menu Functions

        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(rom != null)
                {
                    ByteUtils.SetByte(CHANGE_F20000, 0x10, dropLvl1);
                    ByteUtils.SetByte(CHANGE_F20000, 0x14, (byte)(dropLvl1 + dropLvl2));
                    ByteUtils.SetByte(CHANGE_F20000, 0x18, (byte)(dropLvl1 + dropLvl2 + dropLvl3));

                    ByteUtils.SetByte(CHANGE_F20000, 0x6A, stealLvl1);
                    ByteUtils.SetByte(CHANGE_F20000, 0x6E, (byte)(stealLvl1 + stealLvl2));
                    ByteUtils.SetByte(CHANGE_F20000, 0x72, (byte)(stealLvl1 + stealLvl2 + stealLvl3));

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

        #endregion

        #region Configuration Controls

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

                bool multiSteal = true;

                int origTableOff = ByteUtils.GetInt24(rom.Content, 0x022C42);
                MessageBoxResult dialog = MessageBox.Show("This operation will  apply new code in case you were using version 0.1.1 or move the Drop/Steal table to 0x" + dropTableOff.ToString("X6") + " and new code to 0x" + codeOffset.ToString("X6") + " if this is your first editor use with this ROM.\n\nProceed?",
                    APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (dialog == MessageBoxResult.Yes)
                {
                    dropTableOffAbs = dropTableOff >= 0xC00000 ? dropTableOff - 0xC00000 : dropTableOff;
                    codeOffsetAbs = codeOffset >= 0xC00000 ? codeOffset - 0xC00000 : codeOffset;

                    if (!fromOldVersion)
                    {
                        origTableOff = origTableOff >= 0xC00000 ? origTableOff - 0xC00000 : origTableOff;

                        byte[] originalTable = ByteUtils.GetBytes(rom.Content, origTableOff, 0x600);
                        newTable = new byte[0xC00];

                        for (int i = 0; i < originalTable.Length; i += 2)
                        {
                            newTable[i * 2] = originalTable[i];
                            newTable[i * 2 + 1] = originalTable[i + 1];
                            newTable[i * 2 + 2] = 0xFF;
                            newTable[i * 2 + 3] = 0xFF;
                        }
                    }

                    if (rom.Content[0x023A03] == 0xA9)
                    {
                        MessageBoxResult dialogFix = MessageBox.Show("You seems to not have applied Izmogelmo's Multi-Steal Fix. While this editor can work with or without the fix, it is recommended to not apply the Multi-Steal fix after expanding the data and applied the code. It is therefore recommended to apply the fix before proceeding further. \n\nProceed anyway?",
                        APPNAME, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                        if (dialogFix == MessageBoxResult.Yes)
                        {
                            multiSteal = false;
                        }
                        else
                        {
                            MessageBox.Show("Operation aborted. The application will now close.", APPNAME);
                            Application.Current.Shutdown();
                        }
                    }

                    try
                    {
                        if (!multiSteal)
                        {
                            ByteUtils.SetBytes(rom.Content, dropTableOffAbs, newTable);
                            ByteUtils.SetByte(CHANGE_C239AA, 0x05, 0x51);
                            ByteUtils.SetByte(CHANGE_C239AA, 0x0B, 0x4B);

                            dropLvl1 = 0x40;
                            dropLvl2 = 0x40;
                            dropLvl3 = 0x40;
                        }

                        ByteUtils.SetInt24(CHANGE_C25F1E, 1, codeOffset);
                        ByteUtils.SetInt24(CHANGE_C239AA, 1, codeOffset + 0x31);
                        ByteUtils.SetInt24(CHANGE_C239AA, 7, codeOffset + 0x38);
                        ByteUtils.SetInt24(CHANGE_C239AA, 13, codeOffset + 0x55);
                        ByteUtils.SetInt24(CHANGE_C239D8, 2, codeOffset + 0x5C);
                        ByteUtils.SetInt24(CHANGE_C239F5, 1, codeOffset + 0x7D);
                        ByteUtils.SetInt24(CHANGE_F20000, 0x1F, dropTableOff + 4);
                        ByteUtils.SetInt24(CHANGE_F20000, 0x3C, dropTableOff);
                        ByteUtils.SetInt24(CHANGE_F20000, 0x47, dropTableOff + 2);
                        ByteUtils.SetInt24(CHANGE_F20000, 0x79, dropTableOff);

                        byte jsrHigh = (byte)((codeOffset / 0x100) % 0x100);
                        ByteUtils.SetByte(CHANGE_F20000, 0x0B, jsrHigh);
                        ByteUtils.SetByte(CHANGE_F20000, 0x5E, jsrHigh);


                        ByteUtils.SetBytes(rom.Content, codeOffsetAbs, CHANGE_F20000);
                        ByteUtils.SetBytes(rom.Content, 0x025F1E, CHANGE_C25F1E);
                        ByteUtils.SetBytes(rom.Content, 0x022C3F, CHANGE_C22C3F);
                        ByteUtils.SetBytes(rom.Content, 0x0239AA, CHANGE_C239AA);
                        ByteUtils.SetBytes(rom.Content, 0x0239D8, CHANGE_C239D8);
                        ByteUtils.SetBytes(rom.Content, 0x0239F5, CHANGE_C239F5);

                        MessageBox.Show("Operation succesfull!", APPNAME);
                        
                        stealLvl1 = 0x40;
                        stealLvl2 = 0x40;
                        stealLvl3 = 0x40;

                        TabItemDrops.IsEnabled = true;
                        TabItemSteal.IsEnabled = true;
                        TbDropTable.IsEnabled = false;
                        TbDropCode.IsEnabled = false;
                        BtnExpand.IsEnabled = false;
                        InitCollections();
                        InitDrops();
                        InitSteals();

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

        #endregion

        #region Configuration TextBoxes

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

        #endregion

        #region Editors Buttons

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

        private void BtnStealMonsForward_Click(object sender, RoutedEventArgs e)
        {
            if (CmbStealMonster.SelectedIndex == CmbStealMonster.cmb.Items.Count - 1)
            {
                CmbStealMonster.SelectedIndex = 0;
            }
            else
            {
                CmbStealMonster.SelectedIndex++;
            }
        }

        private void BtnStealMonsBack_Click(object sender, RoutedEventArgs e)
        {
            if (CmbStealMonster.SelectedIndex == 0)
            {
                CmbStealMonster.SelectedIndex = CmbStealMonster.cmb.Items.Count - 1;
            }
            else
            {
                CmbStealMonster.SelectedIndex--;
            }
        }

        #endregion

        #region Editors TextBoxes

        private void TbDropA_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbDropA.Text);

            if(value == -1)
            {
                TbDropA.Text = ((int)dropLvl1).ToString();
            }
            else if(!ValidateDropTotal())
            {
                TbDropA.Text = ((int)dropLvl1).ToString();
            }
            else
            {
                dropLvl1 = (byte)value;
            }
        }

        private void TbDropB_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbDropB.Text);

            if (value == -1)
            {
                TbDropB.Text = ((int)dropLvl2).ToString();
            }
            else if (!ValidateDropTotal())
            {
                TbDropB.Text = ((int)dropLvl2).ToString();
            }
            else
            {
                dropLvl2 = (byte)value;
            }
        }

        private void TbDropC_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbDropC.Text);

            if (value == -1)
            {
                TbDropC.Text = ((int)dropLvl3).ToString();
            }
            else if (!ValidateDropTotal())
            {
                TbDropC.Text = ((int)dropLvl3).ToString();
            }
            else
            {
                dropLvl3 = (byte)value;
            }
        }

        private void TbStealA_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbStealA.Text);

            if (value == -1)
            {
                TbStealA.Text = ((int)stealLvl1).ToString();
            }
            else if (!ValidateStealTotal())
            {
                TbStealA.Text = ((int)stealLvl1).ToString();
            }
            else
            {
                stealLvl1 = (byte)value;
            }
        }

        private void TbStealB_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbStealB.Text);

            if (value == -1)
            {
                TbStealB.Text = ((int)stealLvl2).ToString();
            }
            else if (!ValidateStealTotal())
            {
                TbStealB.Text = ((int)stealLvl2).ToString();
            }
            else
            {
                stealLvl2 = (byte)value;
            }
        }

        private void TbStealC_LostFocus(object sender, RoutedEventArgs e)
        {
            int value = ValidateRate(TbStealC.Text);

            if (value == -1)
            {
                TbStealC.Text = ((int)stealLvl3).ToString();
            }
            else if (!ValidateStealTotal())
            {
                TbStealC.Text = ((int)stealLvl3).ToString();
            }
            else
            {
                stealLvl3 = (byte)value;
            }
        }

        #endregion

        #region TextBox Validation

        private bool ValidateDropTotal()
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

        private bool ValidateStealTotal()
        {
            bool binOk = false;

            int stealA = int.Parse(TbStealA.Text);
            int stealB = int.Parse(TbStealB.Text);
            int stealC = int.Parse(TbStealC.Text);
            int total = stealA + stealB + stealC;
            int stealD = 256 - total;

            if (total <= 255)
            {
                binOk = true;
                TbStealD.Text = stealD.ToString();

                LblStealA.Content = Math.Ceiling((stealA / 256.00) * 100).ToString();
                LblStealB.Content = Math.Ceiling((stealB / 256.00) * 100).ToString();
                LblStealC.Content = Math.Ceiling((stealC / 256.00) * 100).ToString();
                LblStealD.Content = Math.Ceiling((stealD / 256.00) * 100).ToString();
            }
            else
            {
                MessageBox.Show("Error! Total of three first steal chances must not exceed 255 !", APPNAME);
            }

            return binOk;
        }

        private int ValidateRate(string text)
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

        #endregion
    }
}
