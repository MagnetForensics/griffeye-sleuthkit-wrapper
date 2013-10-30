namespace SleuthkitSharp_UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using SleuthKit;

    /// <summary>
    /// This is a test class for DiskImageTest and is intended
    /// to contain all DiskImageTest Unit Tests
    /// </summary>
    [TestFixture]
    public class DiskImageTest
    {
        #region Static Fields

        /// <summary>
        /// The file count.
        /// </summary>
        private static int fileCount;

        #endregion

        #region Fields

        /// <summary>
        /// The disk image.
        /// </summary>
        private DiskImage diskImage;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// A test for GetFileSystems
        /// </summary>
        [Test]
        public void GetFileSystemsTest()
        {
            IEnumerable<FileSystem> actual = this.diskImage.GetFileSystems();
            IEnumerable<FileSystem> fileSystems = actual as FileSystem[] ?? actual.ToArray();
            Assert.AreEqual(3, fileSystems.Count());
            foreach (FileSystem fileSystem in fileSystems)
            {
                fileSystem.Dispose();
            }
        }

        /// <summary>
        /// A test for OpenFileSystem
        /// </summary>
        [Test]
        public void OpenFileSystemTest()
        {
            const FileSystemType Fstype = FileSystemType.Autodetect;
            const long Offset = 16384;
            using (FileSystem actual = this.diskImage.OpenFileSystem(Fstype, Offset))
            {
                Assert.AreEqual(3915744, actual.BlockCount);
                Assert.AreEqual(512, actual.BlockSize);
                Assert.AreEqual(0, actual.FirstBlock);
                Assert.AreEqual(3915743, actual.LastBlock);

                fileCount = 0;
                actual.WalkDirectories(DirectoryWalkCallback, DirWalkFlags.Recurse | DirWalkFlags.Allocated);
                Assert.AreEqual(30, fileCount);

                fileCount = 0;
                actual.WalkDirectories(DirectoryWalkCallback, DirWalkFlags.Recurse | DirWalkFlags.Unallocated);
                Assert.AreEqual(30, fileCount);
            }
        }

        /// <summary>
        /// A test for OpenRead
        /// </summary>
        [Test]
        public void OpenReadTest()
        {
            using (Stream stream = this.diskImage.OpenRead())
            {
                Assert.IsNotNull(stream);
                Assert.AreEqual(2004877312, stream.Length);
            }
        }

        /// <summary>
        /// A test for OpenVolumeSystem
        /// </summary>
        [Test]
        public void OpenVolumeSystemTest()
        {
            using (VolumeSystem volumeSystem = this.diskImage.OpenVolumeSystem())
            {
                Assert.AreEqual(3, volumeSystem.PartitionCount);
                Assert.AreEqual(1, volumeSystem.AllocatedPartitionCount);
                Assert.AreEqual(3, volumeSystem.Volumes.Count());
            }
        }

        /// <summary>
        /// A test for SectorSize
        /// </summary>
        [Test]
        public void SectorSizeTest()
        {
            Assert.AreEqual(512, this.diskImage.SectorSize);
        }

        /// <summary>
        /// Sets up disk image tests.
        /// </summary>
        [SetUp]
        public void SetUpDiskImageTests()
        {
            var file = new FileInfo(ConfigurationManager.AppSettings["E01Path"]);
            this.diskImage = new DiskImage(file);
        }

        /// <summary>
        /// A test for Size
        /// </summary>
        [Test]
        public void SizeTest()
        {
            Assert.AreEqual(2004877312, this.diskImage.Size);
        }

        /// <summary>
        /// Tears down disk image tests.
        /// </summary>
        [TearDown]
        public void TearDownDiskImageTests()
        {
            if (this.diskImage != null)
            {
                this.diskImage.Dispose();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Callback function that is called for each file name during directory walk. (FileSystem.WalkDirectories)
        /// </summary>
        /// <param name="file">
        /// The file struct.
        /// </param>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <param name="dataPtr">
        /// Pointer to data that is passed to the callback function each time.
        /// </param>
        /// <returns>
        /// Value to control the directory walk.
        /// </returns>
        private static WalkReturnEnum DirectoryWalkCallback(ref FileStruct file, string directoryPath, IntPtr dataPtr)
        {
            if (file.Name.ToString().Contains("jpg"))
            {
                fileCount++;
            }

            return WalkReturnEnum.Continue;
        }

        #endregion
    }
}