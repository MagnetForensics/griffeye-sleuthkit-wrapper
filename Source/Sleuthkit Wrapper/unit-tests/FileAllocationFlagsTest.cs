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
            const string path = @"\\netcleantech.local\dfs\TestData\Automatic Tests\Griffeye\SleuthkitWrapper\USB-Disk-image-FAT.E01";
            Dictionary<FileAllocationFlags, int> result = CountFilesPerFlagsInImage(path);

            Assert.AreEqual(2, result[FileAllocationFlags.None]);
            Assert.AreEqual(2, result[FileAllocationFlags.Deleted]);
            Assert.AreEqual(2, result[FileAllocationFlags.Overwritten]);
        }

        private Dictionary<FileAllocationFlags, int> CountFilesPerFlagsInImage(String imagePath)
        {
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));

            int failedFsCount = 0;

            Dictionary<FileAllocationFlags, int> result = new Dictionary<FileAllocationFlags, int>();
            result.Add(FileAllocationFlags.None, 0);
            result.Add(FileAllocationFlags.Deleted, 0);
            result.Add(FileAllocationFlags.Overwritten, 0);

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

                result[FileAllocationFlags.None] += counter.NoneCount;
                result[FileAllocationFlags.Deleted] += counter.DeletedCount;
                result[FileAllocationFlags.Overwritten] += counter.OverwrittenCount;
            }
            catch (Exception)
            {
                failedFsCount++;
            }
        }

        private class FileFlagCounter
        {
            private FileSystem fileSystem;

            public int NoneCount { get; private set; }
            public int DeletedCount { get; private set; }
            public int OverwrittenCount { get; private set; }

            internal FileFlagCounter(FileSystem fileSystem)
            {
                this.fileSystem = fileSystem;
            }

            public WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, string path, IntPtr some_ptr)
            {
                if (file.AppearsValid)
                {
                    FileAllocationFlags flags = fileSystem.GetFileAllocationFlags(file);

                    switch (flags)
                    {
                        case FileAllocationFlags.None: NoneCount++; break;
                        case FileAllocationFlags.Deleted: DeletedCount++; break;
                        case FileAllocationFlags.Overwritten: OverwrittenCount++; break;
                    }
                }

                return WalkReturnEnum.Continue;
            }
        }
    }
}