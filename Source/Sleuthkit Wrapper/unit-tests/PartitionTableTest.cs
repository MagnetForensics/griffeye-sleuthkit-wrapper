namespace SleuthkitSharp_UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using SleuthKit;

    using Directory = SleuthKit.Directory;
    using File = SleuthKit.File;
    using SleuthKit.Structs;

    /// <summary>
    ///     This is a test class intended
    ///     to contain all Unit Tests testing the extraction of partition tables
    /// </summary>
    [TestFixture]
    public class PartitionTableTest
    {
        [Test]
        public void LibewfRegressionTest()
        {
            FileInfo file = new FileInfo(@"\\diskmaskinen\diskimages\Regression\AN-3871\image-clone4.E01");
            //DOS Partition Table
            //Offset Sector: 0
            //Units are in 512-byte sectors
            //      Slot      Start        End          Length       Description
            //000:  Meta      0000000000   0000000000   0000000001   Primary Table (#0)
            //001:  -------   0000000000   0000002047   0000002048   Unallocated
            //002:  000:000   0000002048   0000206847   0000204800   NTFS / exFAT (0x07)
            //003:  000:001   0000206848   0865615871   0865409024   NTFS / exFAT (0x07)
            //004:  Meta      0865615872   0974671871   0109056000   DOS Extended (0x05)
            //005:  Meta      0865615872   0865615872   0000000001   Extended Table (#1)
            //006:  -------   0865615872   0865617919   0000002048   Unallocated
            //007:  001:000   0865617920   0970475519   0104857600   NTFS / exFAT (0x07)
            //008:  Meta      0970475520   0970739711   0000264192   DOS Extended (0x05)
            //009:  Meta      0970475520   0970475520   0000000001   Extended Table (#2)
            //010:  -------   0970475520   0970477567   0000002048   Unallocated
            //011:  002:000   0970477568   0970739711   0000262144   Unknown Type (0x27)
            //012:  Meta      0970739712   0972574719   0001835008   DOS Extended (0x05)
            //013:  Meta      0970739712   0970739712   0000000001   Extended Table (#3)
            //014:  -------   0970739712   0970741759   0000002048   Unallocated
            //015:  003:000   0970741760   0972574719   0001832960   Unknown Type (0x27)
            //016:  Meta      0972574720   0974671871   0002097152   DOS Extended (0x05)
            //017:  Meta      0972574720   0972574720   0000000001   Extended Table (#4)
            //018:  -------   0972574720   0972576767   0000002048   Unallocated
            //019:  004:000   0972576768   0974671871   0002095104   Unknown Type (0x27)
            //020:  000:003   0974671872   0976771071   0002099200   Hibernation (0x12)
            //021:  -------   0976771072   0976773167   0000002096   Unallocated

            VolumeInfo[] volInfo = new VolumeInfo[]
            {
                new VolumeInfo()
                {
                    Start = 0,
                    Length = 1,
                    Allocated = true
                }
            };
            DiskImage diskImage = new DiskImage(file);
            using (VolumeSystem volumeSystem = diskImage.OpenVolumeSystem())
            {
                Assert.IsNotNull(volumeSystem, "Unable to read partition table");
                Assert.IsTrue(volumeSystem.Volumes.Any(), "Unable to read partition table");
                //Assert.AreEqual(, volumeSystem.PartitionCount);
                //Assert.AreEqual(, volumeSystem.AllocatedPartitionCount);
                //Assert.AreEqual(, volumeSystem.Volumes.Count());
                //List<Volume> volumes = volumeSystem.Volumes.ToList();
                //for (int i = 0; i < volumes.Count; i++)
                //{
                //    Assert.AreEqual(volInfo[i].Start, volumes[i].Offset,
                //        String.Format("Volume offset was not correct for volume {0}", i));
                //    Assert.AreEqual(volInfo[i].Length, volumes[i].Length,
                //        String.Format("Volume length was not correct for volume {0}", i));
                //    Assert.AreEqual(volInfo[i].Allocated, volumes[i].IsAllocated,
                //        String.Format("Volume allocation did not match for volume {0}", i));
                //}
            }
            
        }
    }
}