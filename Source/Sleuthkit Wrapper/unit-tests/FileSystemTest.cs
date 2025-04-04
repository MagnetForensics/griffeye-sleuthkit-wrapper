﻿using NUnit.Framework;
using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SleuthkitSharp_UnitTests
{
    [TestFixture]
    internal class FileSystemTest
    {
        //000
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\000\DSIII_disk_Ext4_Guymager_SplitSize2047MiB.000")]

        //DD
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\DD\DSIII_disk_Ext4_Guymager.dd")]

        //DMG
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\DMG\DSIII_USB_HFSplus_DiskUtility.dmg")]

        //E01 other than EnCase
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_disk_Ext4_Guymager_Exx.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_disk_FAT_OSF.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_disk_FAT_OSF_BestCompressed.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_disk_NTFS_OSF.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_disk_NTFS_OSF_BestCompressed.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_USB_exFAT_OSF.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\E01_OtherThanEnCase\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.E01")]

        //EnCase\Evidence
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_disk_Ext4_dd_BlockSize512_dd_EnCase_Uncompressed.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_disk_Ext4_Guymager_dd_EnCase_Compressed.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_disk_FAT_dd_BlockSize512_raw_EnCase_Uncompressed.E01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_disk_NTFS_OSF_img_EnCase_Uncompressed.Ex01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_DVD_ISO9660_disks_ISO_EnCase_compressed.Ex01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_USB_UFS1-44BSD_disks_img_EnCase_Compressed.Ex01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Compressed.E01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Compressed.Ex01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Uncompressed.E01")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Evidence\DSIII_DVD_ISO9660_EnCase_Uncompressed.Ex01")]

        //IMG
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_Ext2_dd_SingleBlock.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_Ext4_dd_SingleBlock.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_FAT_dd_BlockSize512.iso")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_FAT_dd_BlockSize2048.iso")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_FAT_OSF.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_NTFS_dd_BlockSize512.iso")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_disk_NTFS_OSF.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_DVD_ISO9660_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_exFAT_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_exFAT_OSF.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_exFATHFSplus_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_Ext2_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_Ext3_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_Ext4_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_FAT_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_HFSplus_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_HFSplus_OSF.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_NTFS_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_NTFSExt4FATExt2_disks_FourPartitions.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_UFS1-44BSD_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_UFS2_disks.img")]

        //RAW
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\RAW\DSIII_disk_FAT_dd_BlockSize512.raw")]
        public void AssertDS3Image(String imagePath)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.GreaterOrEqual(fileCount.FileCount, 1000);
            Assert.GreaterOrEqual(fileCount.MetadataIsValidCount, 1000);
        }

        //IMG
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII-partly_folders_YAFFS2_YAFFEY.img")]
        public void AssertDS3PartlyImage(String imagePath)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.GreaterOrEqual(fileCount.FileCount, 100);
        }

        //RAW
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\IMG\DSIII_USB_Ext4_disks.img")]
        public void AssertDS3ImageExactCount(String imagePath)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.AreEqual(5346, fileCount.FileCount);
            Assert.AreEqual(4630, fileCount.MetadataIsValidCount);
        }

        //APFS
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\APFS\apfs.dmg", 23)]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\APFS\unencrypted_apfs.dmg", 15)]
        public void AssertApfsImage(String imagePath, int expectedFileCount)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.AreEqual(expectedFileCount, fileCount.FileCount);
        }

        //EnCase\Logical - All failing
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_disk_Ext4_dd_BlockSize512_dd_EnCase_Compressed.Lx01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_disk_Ext4_Guymager_dd_EnCase_Uncompressed.Lx01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_disk_FAT_dd_BlockSize512_raw_EnCase_Compressed.Lx01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_disk_NTFS_OSF_img_EnCase_Compressed.L01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_DVD_ISO9660_disks_ISO_EnCase_Unompressed.L01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\EnCase\Logical\DSIII_USB_UFS1-44BSD_disks_img_EnCase_Uncompressed.L01")]

        //NOK - Still not supported
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_disk_FAT_OSF.aff")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_disk_NTFS_OSF.aff")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_USB_exFAT_OSF.aff")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_USB_HFS_disks.img")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_USB_HFS_disks_img_EnCase_Uncompressed.E01")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII_USB_NTFSExt4FATExt2_OSF_FourPartitions.aff")]
        //[TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\NOK\DSIII-partly_folders_YAFFS2_YAFFEY_img_EnCase_Uncompressed.Ex01")]

        //DMG
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\DMG\DSIII_USB_HFSplus_DiskUtility_Compressed.dmg")]
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\DSIII\DMG\DSIII_USB_HFSplus_DiskUtility_ReadOnly.dmg")]
        public void AssertFailingDS3Image(String imagePath)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.AreEqual(0, fileCount.FileCount);
        }

        // Swedish characters
        [TestCase(@"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\Regression\AN-9401\TestSwedishFolderName.E01")]
        public void TestSwedishCharacters(String imagePath)
        {
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out var labels, out List<string> directories);

            Assert.AreEqual(54, fileCount.FileCount);
            Assert.IsTrue(directories.Contains("ÅÄÖ/"));
        }

        [Test]
        public void AndroidFileSystem()
        {
            var imagePath = @"\\winfiles.griffeye.net\dfs\TestData\Test sets\Automation sets\Forensic images\Regression\AN-12189\2picstest.E01";
            var fileCount = CountFilesInImageAndGetLabels(imagePath, out _, out _, FileSystemType.FATAndroid);

            Assert.AreEqual(7, fileCount.FileCount);
            Assert.AreEqual(2, fileCount.MetadataIsValidCount);
        }

        private FileCounter CountFilesInImageAndGetLabels(String imagePath, out String labels, out List<string> directories,
            FileSystemType fileSystem = FileSystemType.Autodetect)
        {
            using (DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath)))
            {
                FileCounter counter = new FileCounter();
                int failCount = 0;
                labels = String.Empty;

                try
                {
                    if (image.HasVolumes)
                    {
                        using (VolumeSystem vs = image.OpenVolumeSystem())
                        {
                            foreach (Volume v in vs.Volumes)
                            {
                                if (!v.IsAllocated)
                                {
                                    continue;
                                }

                                bool poolOpened = false;
                                using (var openPool = v.OpenPool(fileSystem))
                                {
                                    if (openPool != null)
                                    {
                                        foreach (var tskPoolVolumeInfo in openPool.GetVolumeInfos())
                                        {
                                            try
                                            {
                                                using (FileSystem fs = openPool.OpenFileSystem(tskPoolVolumeInfo, fileSystem))
                                                {
                                                    labels += labels == String.Empty ? fs.Label : ", " + fs.Label;

                                                    fs.WalkDirectories(counter.DirWalkCallback);
                                                    poolOpened = true;
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                failCount++;
                                            }
                                        }
                                    }
                                }

                                try
                                {
                                    if (!poolOpened)
                                    {
                                        using (FileSystem fs = v.OpenFileSystem(fileSystem))
                                        {
                                            labels += labels == String.Empty ? fs.Label : ", " + fs.Label;

                                            fs.WalkDirectories(counter.DirWalkCallback);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    failCount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var fs = image.OpenFileSystem(fileSystem);
                            labels += labels == String.Empty ? fs.Label : ", " + fs.Label;
                            fs.WalkDirectories(counter.DirWalkCallback);
                        }
                        catch (Exception)
                        {
                            failCount++;
                        }
                    }
                }
                catch (Exception) { }

                directories = counter.Directories;

                return counter;
            }
        }

        private class FileCounter
        {
            public int FileCount { get; private set; }

            public int MetadataIsValidCount { get; private set; }

            public List<string> Directories { get; } = new List<string>();

            public List<string> Files { get; } = new List<string>();

            public WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, IntPtr utf8_path, IntPtr some_ptr)
            {
                var directoryName = utf8_path.Utf8ToUtf16();
                Directories.Add(directoryName);

                var metadataValid = MetadataIsValid(file, FileSystemType.Autodetect);

                MetadataIsValidCount += metadataValid ? 1 : 0;
                FileCount += file.AppearsValid ? 1 : 0;
                var tskFsName = file.Name;
                if (tskFsName != null)
                {
                    Files.Add(tskFsName.Value.GetName());
                }

                return WalkReturnEnum.Continue;
            }

            // This is logic copied from analyze.
            private static bool MetadataIsValid(TSK_FS_FILE file, FileSystemType fsType)
            {
                if (file.Metadata.HasValue && file.Name.HasValue &&
                    (file.Metadata.Value.MetadataType == MetadataType.Regular ||
                     file.Metadata.Value.MetadataType == MetadataType.Symlink))
                {
                    //If the filesystem has a list of file names in the metadata, check it to see if they match.
                    //  If not, metadata is overwritten and file should not be included.
                    IEnumerable<TSK_FS_META_NAME_LIST> nameList = file.Metadata.Value.NameList;
                    if (nameList.Any())
                    {
                        string maybeLongName = (file.Name.Value.LongName ?? string.Empty).TrimEnd(new char[] { '\0' });
                        string maybeShortName =
                            (file.Name.Value.ShortName ?? string.Empty).TrimEnd(new char[] { '\0' });

                        ulong maybeParentAddress = file.Name.Value.ParentAddress;

                        foreach (TSK_FS_META_NAME_LIST name in nameList)
                        {
                            string actualName = name.Name;

                            if (FileSystemType.NTFS == fsType && name.ParentAddress != maybeParentAddress)
                            {
                                continue;
                            }
                            else if (FileSystemType.ExFAT == fsType)
                            {
                                int nullIndex = actualName.IndexOf('\0');
                                if (-1 != nullIndex)
                                {
                                    actualName = actualName.Substring(0, nullIndex);
                                }
                            }

                            if (actualName.Equals(maybeShortName, StringComparison.Ordinal) ||
                                actualName.Equals(maybeLongName, StringComparison.Ordinal))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }
    }
}