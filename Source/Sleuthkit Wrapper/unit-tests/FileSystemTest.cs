using NUnit.Framework;
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
        [Test]
        public void TestExFAT()
        {
            String imagePath = @"\\philadelphia\TestShare\Sleuthkit Wrapper\exFAT.E01";
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));
            List<FileSystem> fileSystems = image.GetFileSystems().ToList();

            Assert.AreEqual(1, fileSystems.Count);
            Assert.AreEqual("ExFAT", fileSystems.First().Type.ToString());
        }

        [Test]
        public void TestDSS3()
        {
            String imagePath = @"I:\TestSets\Mixed\DATASET_SAFE_III\Images and exports\DSIII.E01";
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));

            foreach (FileSystem fs in image.GetFileSystems())
            {
                fs.WalkDirectories(DirWalkCallback);
            }

            Assert.GreaterOrEqual(1000, fileCount);
        }

        private int fileCount = 0;

        private WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, string path, IntPtr some_ptr)
        {
            fileCount += file.AppearsValid ? 1 : 0;
            return WalkReturnEnum.Continue;
        }
    }
}