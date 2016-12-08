using NUnit.Framework;
using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;

namespace SleuthkitSharp_UnitTests
{
    [TestFixture]
    public class FileAllocationFlagsTest
    {
        [Test]
        public void TestFileAllocationFlags()
        {
            const string path = @"\\netcleantech.local\dfs\TestData\Automatic Tests\Griffeye\SleuthkitWrapper\G-Recycle-2.E01";
            Dictionary<FileAllocationFlags, int> result = CountFilesPerFlagsInImage(path);

            Assert.AreEqual(267, result[FileAllocationFlags.None]);
            Assert.AreEqual(70, result[FileAllocationFlags.Deleted]);
            Assert.AreEqual(46, result[FileAllocationFlags.Deleted | FileAllocationFlags.Overwritten]);
        }

        private Dictionary<FileAllocationFlags, int> CountFilesPerFlagsInImage(String imagePath)
        {
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));

            int failedFsCount = 0;

            Dictionary<FileAllocationFlags, int> result = new Dictionary<FileAllocationFlags, int>();

            if (image.HasVolumes)
            {
                VolumeSystem vs = image.OpenVolumeSystem();
                foreach (Volume v in vs.Volumes)
                {
                    CountInFileSystem(v.OpenFileSystem(), result, ref failedFsCount);
                }
            }
            else
            {
                foreach (FileSystem fs in image.GetFileSystems())
                {
                    CountInFileSystem(fs, result, ref failedFsCount);
                }
            }

            return result;
        }

        private void CountInFileSystem(FileSystem fs, Dictionary<FileAllocationFlags, int> result, ref int failedFsCount)
        {
            try
            {
                FileFlagCounter counter = new FileFlagCounter(fs);
                fs.WalkDirectories(counter.DirWalkCallback);

                foreach (KeyValuePair<FileAllocationFlags, int> kvp in counter.Count)
                {
                    if (!result.ContainsKey(kvp.Key))
                    {
                        result.Add(kvp.Key, kvp.Value);
                    }
                    else
                    {
                        result[kvp.Key] += kvp.Value;
                    }
                }
            }
            catch (Exception)
            {
                failedFsCount++;
            }
        }

        private class FileFlagCounter
        {
            private FileSystem fileSystem;

            public Dictionary<FileAllocationFlags, int> Count;

            internal FileFlagCounter(FileSystem fileSystem)
            {
                this.fileSystem = fileSystem;
                Count = new Dictionary<FileAllocationFlags, int>();
            }

            public WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, string path, IntPtr some_ptr)
            {
                if (file.AppearsValid)
                {
                    FileAllocationFlags flags = fileSystem.GetFileAllocationFlags(file);

                    if (!Count.ContainsKey(flags))
                    {
                        Count.Add(flags, 1);
                    }
                    else
                    {
                        Count[flags]++;
                    }
                }

                return WalkReturnEnum.Continue;
            }
        }
    }
}