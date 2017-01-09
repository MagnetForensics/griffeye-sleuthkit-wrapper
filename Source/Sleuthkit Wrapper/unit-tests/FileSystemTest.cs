using NUnit.Framework;
using SleuthKit;
using SleuthKit.Structs;
using System;

namespace SleuthkitSharp_UnitTests
{
    [TestFixture]
    internal class FileSystemTest
    {
        //000
        [TestCase(@"\\DISKMASKINEN\DiskImages\000\DSIII_disk_Ext4_Guymager_SplitSize2047MiB.000")]

        //DD
        [TestCase(@"\\DISKMASKINEN\DiskImages\DD\DSIII_disk_Ext4_Guymager.dd")]

        //DMG
        [TestCase(@"\\diskmaskinen\DiskImages\DMG\DSIII_USB_HFSplus_DiskUtility.dmg")]

        //E01 other than EnCase
        [TestCase(@"\\DISKMASKINEN\DiskImages\E01_OtherThanEnCase\DSIII_disk_Ext4_Guymager_Exx.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\E01_OtherThanEnCase\DSIII_disk_FAT_OSF.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\E01_OtherThanEnCase\DSIII_disk_FAT_OSF_BestCompressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\E01_OtherThanEnCase\DSIII_disk_NTFS_OSF.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\E01_OtherThanEnCase\DSIII_disk_NTFS_OSF_BestCompressed.E01")]
        [TestCase(@"\\diskmaskinen\DiskImages\E01_OtherThanEnCase\DSIII_USB_exFAT_OSF.E01")]
        [TestCase(@"\\diskmaskinen\DiskImages\E01_OtherThanEnCase\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.E01")]

        //EnCase\Evidence
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_disk_Ext4_dd_BlockSize512_dd_EnCase_Uncompressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_disk_Ext4_Guymager_dd_EnCase_Compressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_disk_FAT_dd_BlockSize512_raw_EnCase_Uncompressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_disk_NTFS_OSF_img_EnCase_Uncompressed.Ex01")] //
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_DVD_ISO9660_disks_ISO_EnCase_compressed.Ex01")] //
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_USB_UFS1-44BSD_disks_img_EnCase_Compressed.Ex01")] //
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Compressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Compressed.Ex01")] //
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Uncompressed.E01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Uncompressed.Ex01")] //

        //IMG
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_Ext2_dd_SingleBlock.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_Ext4_dd_SingleBlock.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_FAT_dd_BlockSize512.iso")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_FAT_dd_BlockSize2048.iso")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_FAT_OSF.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_NTFS_dd_BlockSize512.iso")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_disk_NTFS_OSF.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_DVD_ISO9660_disks.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_exFAT_disks.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_exFAT_OSF.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_exFATHFSplus_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_Ext2_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_Ext3_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_Ext4_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_FAT_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_HFSplus_disks.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_HFSplus_OSF.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_NTFS_disks.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_NTFSExt4FATExt2_disks_FourPartitions.img")]
        [TestCase(@"\\diskmaskinen\DiskImages\IMG\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_UFS1-44BSD_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII_USB_UFS2_disks.img")]

        //RAW
        [TestCase(@"\\DISKMASKINEN\DiskImages\RAW\DSIII_disk_FAT_dd_BlockSize512.raw")]
        public void AssertDS3Image(String imagePath)
        {
            String labels;
            int fileCount = CountFilesInImageAndGetLabels(imagePath, out labels);

            Assert.GreaterOrEqual(fileCount, 1000);
        }

        //IMG
        [TestCase(@"\\DISKMASKINEN\DiskImages\IMG\DSIII-partly_folders_YAFFS2_YAFFEY.img")]
        public void AssertDS3PartlyImage(String imagePath)
        {
            String labels;
            int fileCount = CountFilesInImageAndGetLabels(imagePath, out labels);

            Assert.GreaterOrEqual(fileCount, 100);
        }

        //EnCase\Logical - All failing
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_disk_Ext4_dd_BlockSize512_dd_EnCase_Compressed.Lx01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_disk_Ext4_Guymager_dd_EnCase_Uncompressed.Lx01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_disk_FAT_dd_BlockSize512_raw_EnCase_Compressed.Lx01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_disk_NTFS_OSF_img_EnCase_Compressed.L01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_DVD_ISO9660_disks_ISO_EnCase_Unompressed.L01")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\EnCase\Logical\DSIII_USB_UFS1-44BSD_disks_img_EnCase_Uncompressed.L01")]

        //NOK - Still not supported
        [TestCase(@"\\DISKMASKINEN\DiskImages\NOK\DSIII_disk_FAT_OSF.aff")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\NOK\DSIII_disk_NTFS_OSF.aff")]
        [TestCase(@"\\diskmaskinen\DiskImages\NOK\DSIII_USB_exFAT_OSF.aff")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\NOK\DSIII_USB_HFS_disks.img")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\NOK\DSIII_USB_HFS_disks_img_EnCase_Uncompressed.E01")]
        [TestCase(@"\\diskmaskinen\DiskImages\NOK\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.aff")]
        [TestCase(@"\\DISKMASKINEN\DiskImages\NOK\DSIII-partly_folders_YAFFS2_YAFFEY_img_EnCase_Uncompressed.Ex01")]

        //DMG
        [TestCase(@"\\diskmaskinen\DiskImages\DMG\DSIII_USB_HFSplus_DiskUtility_Compressed.dmg")]
        [TestCase(@"\\diskmaskinen\DiskImages\DMG\DSIII_USB_HFSplus_DiskUtility_ReadOnly.dmg")]

        public void AssertFailingDS3Image(String imagePath)
        {
            String labels;
            int fileCount = CountFilesInImageAndGetLabels(imagePath, out labels);

            Assert.AreEqual(fileCount, 0);
        }

        private int CountFilesInImageAndGetLabels(String imagePath, out String labels)
        {
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));
            FileCounter counter = new FileCounter();
            int failCount = 0;
            labels = String.Empty;

            try
            {
                if (image.HasVolumes)
                {
                    VolumeSystem vs = image.OpenVolumeSystem();
                    foreach (Volume v in vs.Volumes)
                    {
                        try
                        {
                            FileSystem fs = v.OpenFileSystem();
                            labels += labels == String.Empty ? fs.Label : ", " + fs.Label;

                            fs.WalkDirectories(counter.DirWalkCallback);
                        }
                        catch (Exception)
                        {
                            failCount++;
                        }
                    }
                }
                else
                {
                    try
                    {
                        foreach (FileSystem fs in image.GetFileSystems())
                        {
                            labels += labels == String.Empty ? fs.Label : ", " + fs.Label;
                            fs.WalkDirectories(counter.DirWalkCallback);
                        }
                    }
                    catch (Exception)
                    {
                        failCount++;
                    }
                }
            }
            catch (Exception) { }

            return counter.FileCount;
        }

        private class FileCounter
        {
            public int FileCount { get; private set; }

            public WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, string path, IntPtr some_ptr)
            {
                FileCount += file.AppearsValid ? 1 : 0;
                return WalkReturnEnum.Continue;
            }
        }
    }
}