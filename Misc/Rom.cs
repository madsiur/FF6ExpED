using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FF6exped.Misc
{  
    public class Rom
    {
        public String Filename { get; private set; }
        public String Title { get; private set; }
        public String RomSize { get; private set; }
        public String Version { get; private set; }
        public String strHeader { get; private set; }
        public string GameCode { get; private set; }
        public byte[] Content { get; set; }
        public byte[] Header { get; private set; }
        public int RomLength { get; private set; }
        public byte[] RomHash { get; private set; }
        public int CheckSum { get; private set; }
        public String strCheckSum { get; private set; }
        public String MD5CheckSum { get; private set; }

        private const int HI_ROM_SUBS = 0xC00000;

        // Monster Data
        public int monsNameOff;
        public int monsAtkNamesOff;
        public int monsAiPtrOff;
        public int monsAiDataOff;
        public int monsAiDataBank;
        public int monsItemOff;
        public int monsStatOff;
        public int monsControlOff;
        public int monsSketchOff;
        public int monsVisSpecOff;
        public int monsAtkAppOff;

        // Monster Graphics
        public int mons8bitCompDataPtrOff;
        public int mons16bitCompDataPtrOff;
        public int compDataBank;
        public int mons8bitCompDataOff;
        public int mons16bitCompDataOff;
        public int monsGfxOff;

        public Rom(String fileName)
        {
            this.Filename = fileName;
            ReadRom();
            ReadRomInfo();
        }

        private void ReadRomInfo()
        {
            GameCode = GetGameCode();
            Title = GetRomName();
            strCheckSum = GetRomChecksum();
            CreateNewMD5Checksum();
            MD5CheckSum = GetMD5CheckSum();
            RomSize = GetRomSize();
            Version = "1." + GetRomVersion();
            Title += " v" + Version;
        }

        public bool CheckRomExpansion()
        {
            if (Content.Length == 0x400000)
                return true;
            else
                return false;

        }

        public string GetGameCode()
        {
            Encoding encoding = Encoding.UTF8;

            return encoding.GetString(ByteUtils.GetBytes(Content, 0xFFB2, 4));
        }

        public string GetRomName()
        {
            Encoding encoding = Encoding.UTF8;

            return encoding.GetString(ByteUtils.GetBytes(Content, 0xFFC0, 21)).Trim();
        }

        public string GetMD5CheckSum()
        {
            return ByteUtils.ByteArrayToString(RomHash);
        }

        public string GetRomSize()
        {
            string romsize;

            switch(RomLength)
            {
                case 0x300000:
                case 0x300200:
                    romsize = "24 MBit";
                    break;
                case 0x380000:
                case 0x380200:
                    romsize = "28 MBit";
                    break;
                case 0x400000:
                case 0x400200:
                    romsize = "32 MBit";
                    break;
                case 0x600000:
                case 0x600200:
                    romsize = "48 MBit";
                    break;
                default:
                    romsize = "Unknown size";
                    break;
            }

            return romsize;
        }

        public string GetRomVersion()
        {
            return ByteUtils.GetBytes(Content, 0xFFBD, 1)[0].ToString();
        }

        public string GetRomChecksum()
        {
            int chunk0 = 0;
            int chunk1 = 0;
            for (int i = 0; i < Content.Length; i++)
            {
                if (i < 0x200000)
                    chunk0 += Content[i];
                else
                    chunk1 += Content[i];
            }

            CheckSum = (chunk0 + chunk1 + chunk1) & 0xFFFF;

            if ((ushort)CheckSum == ByteUtils.GetShort(Content, 0x00FFDE))
                return "0x" + CheckSum.ToString("X") + " (OK)";
            else
                return "0x" + CheckSum.ToString("X") + " (FAIL)";
        }

        public ushort GetRomChecksumBin()
        {
            int chunk0 = 0;
            int chunk1 = 0;

            for (int i = 0; i < Content.Length; i++)
            {
                if (i < 0x200000)
                    chunk0 += Content[i];
                else
                    chunk1 += Content[i];
            }

            CheckSum = (chunk0 + chunk1 + chunk1) & 0xFFFF;

            return (ushort)CheckSum;
        }

        public void SetRomChecksum()
        {
            int chunk0 = 0;
            int chunk1 = 0;

            for (int i = 0; i < Content.Length; i++)
            {
                if (i < 0x200000)
                    chunk0 += Content[i];
                else
                    chunk1 += Content[i];
            }

            CheckSum = (chunk0 + chunk1 + chunk1) & 0xFFFF;

            ByteUtils.SetShort(Content, 0xFFDE, (int)(CheckSum & 0xFFFF));
            ByteUtils.SetShort(Content, 0xFFDC, (int)(CheckSum ^ 0xFFFF));
        }

        public void CalculateAndSetNewRomChecksum()
        {
            int check = 0;
            for (int i = 0; i < Content.Length; i++)
                check += Content[i];
            check &= 0xFFFF;
            ByteUtils.SetShort(Content, 0x00FFDE, (ushort)check);
        }
        
        public bool VerifyMD5Checksum()
        {
            MD5 md5Hasher = MD5.Create();
            byte[] hash;

            if (RomHash != null)
                hash = md5Hasher.ComputeHash(Content);
            else
                return true;

            for (int i = 0; i < RomHash.Length && i < hash.Length; i++)
            {
                if (RomHash[i] != hash[i])
                    return false;
            }

            return true;
        }

        public void CreateNewMD5Checksum()
        {
            MD5 md5Hasher = MD5.Create();
            if (Content != null)
                RomHash = md5Hasher.ComputeHash(Content);
        }

        public bool WriteRom()
        {
            try
            {
                SetRomChecksum();
                AddHeader();
                BinaryWriter binWriter = new BinaryWriter(File.Open(Filename, FileMode.Create));
                binWriter.Write(Content);
                binWriter.Close();

                /*if (Settings.Default.CreateBackupROMSave)
                {
                    DateTime currentTime = DateTime.Now;
                    string backup = " (save @ " +
                        currentTime.Year.ToString("d4") + currentTime.Month.ToString("d2") + currentTime.Day.ToString("d2") + "_" +
                        currentTime.Hour.ToString("d2") + "h" + currentTime.Minute.ToString("d2") + "m" + currentTime.Second.ToString("d2") + "s" +
                        ").bak";
                    BinaryWriter bw;
                    if (settings.BackupROMDirectory == "")
                    {
                        bw = new BinaryWriter(File.Create(fileName + backup));
                        bw.Write(rom);
                        bw.Close();
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(settings.BackupROMDirectory);
                        if (di.Exists)
                        {
                            bw = new BinaryWriter(File.Create(settings.BackupROMDirectory + GetFileNameWithoutPath() + backup));
                            bw.Write(rom);
                            bw.Close();
                        }
                        else
                            MessageBox.Show("Could not create backup ROM.\n\nThe backup ROM directory has been moved, renamed, or no longer exists.", "ZONE DOCTOR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }*/

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("FF6EME was unable to write to the file.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                RemoveHeader();
                return false;
            }
        }

        public bool ReadRom()
        {
            try
            {
                FileInfo fInfo = new FileInfo(Filename);
                RomLength = (int)fInfo.Length;
                FileStream fStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fStream);
                Content = br.ReadBytes((int)RomLength);
                br.Close();
                fStream.Close();

                /*if (settings.CreateBackupROM)
                {
                    DateTime currentTime = DateTime.Now;
                    string backup = " (open @ " +
                        currentTime.Year.ToString("d4") + currentTime.Month.ToString("d2") + currentTime.Day.ToString("d2") + "_" +
                        currentTime.Hour.ToString("d2") + "h" + currentTime.Minute.ToString("d2") + "m" + currentTime.Second.ToString("d2") + "s" +
                        ").bak";
                    BinaryWriter bw;
                    if (settings.BackupROMDirectory == "")
                    {
                        bw = new BinaryWriter(File.Create(fileName + backup));
                        bw.Write(rom);
                        bw.Close();
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(settings.BackupROMDirectory);
                        if (di.Exists)
                        {
                            bw = new BinaryWriter(File.Create(settings.BackupROMDirectory + GetFileNameWithoutPath() + backup));
                            bw.Write(rom);
                            bw.Close();
                        }
                        else
                            MessageBox.Show("Could not create backup ROM.\n\nThe backup ROM directory has been moved, renamed, or no longer exists.", "ZONE DOCTOR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }*/

                RemoveHeader();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("FF6EME was unable to load the rom.\n\n" + e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        public bool RemoveHeader()
        {
            Header = null;

            if ((RomLength & 0x200) != 0x200)
                return false;

            try
            {
                RomLength -= 0x200;
                Header = ByteUtils.GetBytes(Content, 0, 0x200);
                Content = ByteUtils.GetBytes(Content, 0x200, RomLength);
                
                return true;
            }
            catch
            {
                MessageBox.Show("Error removing header; please remove manually.");
                return false;
            }
        }

        public bool AddHeader()
        {
            if (Header == null) 
                return false;

            try
            {
                RomLength += 0x200;
                byte[] temp = new byte[RomLength];
                Header.CopyTo(temp, 0);
                Content.CopyTo(temp, 0x200);
                Content = temp;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void GetOffsets()
        {
            // Names
            monsNameOff = ByteUtils.GetInt24(Content, 0x022C57) - HI_ROM_SUBS;

            // Special Attack Names
            byte[] atkNames = new byte[3];
            atkNames[0] = ByteUtils.GetBytes(Content, 0x01973C, 1)[0];
            atkNames[1] = ByteUtils.GetBytes(Content, 0x019751, 1)[0];
            atkNames[2] = ByteUtils.GetBytes(Content, 0x019750, 1)[0];
            monsAtkNamesOff = ByteUtils.GetInt24Reversed(atkNames, 0) - HI_ROM_SUBS;

            // AI Pointers
            monsAiPtrOff = ByteUtils.GetInt24(Content, 0x022D79) - HI_ROM_SUBS;

            // AI Data
            monsAiDataOff = ByteUtils.GetInt24(Content, 0x022D89) - HI_ROM_SUBS;

            // AI bank

            byte test = ByteUtils.GetBytes(Content, 0x022D8B, 1)[0];
            monsAiDataBank = (int)(ByteUtils.GetBytes(Content, 0x022D8B, 1)[0] << 16) - HI_ROM_SUBS;

            // Stats
            monsStatOff = ByteUtils.GetInt24(Content, 0x022D26) - HI_ROM_SUBS;

            // Control Commands
            monsControlOff = ByteUtils.GetInt24(Content, 0x02063E) - HI_ROM_SUBS;

            // Sketch Commands
            monsSketchOff = ByteUtils.GetInt24(Content, 0x023B5B) - HI_ROM_SUBS;

            // Visual Specs
            monsVisSpecOff = ByteUtils.GetInt24(Content, 0x01208D) - HI_ROM_SUBS;

            // Special Attacks Appearances
            monsAtkAppOff = ByteUtils.GetInt24(Content, 0x022D50) - HI_ROM_SUBS;

            // Items Stolen/Dropped
            monsItemOff = ByteUtils.GetInt24(Content, 0x022C42) - HI_ROM_SUBS;

            // 8bit Composition Data Pointer
            mons8bitCompDataPtrOff = ByteUtils.GetInt24(Content, 0x01217B) - HI_ROM_SUBS;

            // 16bit Composition Data Pointer
            mons16bitCompDataPtrOff = ByteUtils.GetInt24(Content, 0x0121A1) - HI_ROM_SUBS;

            // Composition Data Bank
            compDataBank = (int)(ByteUtils.GetBytes(Content, 0x012187, 1)[0] << 16) - HI_ROM_SUBS;

            // 8bit Composition Data
            mons8bitCompDataOff = ByteUtils.GetShort(Content, mons8bitCompDataPtrOff);
            mons8bitCompDataOff += compDataBank;

            // 16bit Composition Data
            mons16bitCompDataOff = ByteUtils.GetShort(Content, mons16bitCompDataPtrOff);
            mons16bitCompDataOff += compDataBank;

            // Graphics
            byte[] monsGfx = new byte[3];
            monsGfx[0] = ByteUtils.GetBytes(Content, 0x01211B, 1)[0];
            monsGfx[1] = ByteUtils.GetBytes(Content, 0x012115, 1)[0];
            monsGfx[2] = ByteUtils.GetBytes(Content, 0x01210F, 1)[0];
            monsGfxOff = ByteUtils.GetInt24Reversed(monsGfx, 0) - HI_ROM_SUBS;
        }

        public void ExpandRom()
        {
            byte[] temp = new byte[0x400000];

            Buffer.BlockCopy(Content, 0, temp, 0, Content.Length);

            for (int i = Content.Length - 1; i < 0x400000; i++)
                temp[i] = 0x00;

            Content = temp;
            RomLength = Content.Length;
        }



    }
}
