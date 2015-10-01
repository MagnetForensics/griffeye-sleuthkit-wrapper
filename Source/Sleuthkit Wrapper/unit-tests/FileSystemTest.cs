using NUnit.Framework;
using SleuthKit;
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

#if Bit64

        [Test]
        public void TestCrashingImage()
        {
            String imagePath = @"\\philadelphia\TestShare\Sleuthkit Wrapper\DATASAFE_3-2014.E01";
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));
            FileCounter counter = new FileCounter();
            int failCount = 0;
            String labels = String.Empty;

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

            Assert.GreaterOrEqual(counter.FileCount, 1000);
            Assert.AreEqual(2, failCount);
            Assert.AreNotEqual(String.Empty, labels);
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
#endif
    }
}