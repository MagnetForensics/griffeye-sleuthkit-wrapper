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
    }
}