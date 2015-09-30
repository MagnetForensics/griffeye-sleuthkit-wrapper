using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Org.SleuthKit.Crash
{
    public class CrashOnDSS3
    {
        public static void Main(String[] args)
        {
            String imagePath = @"I:\TestSets\Mixed\DATASET_SAFE_III\Images and exports\DSIII.E01";
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));

            List<FileSystem> fileSystems = image.GetFileSystems().ToList();
            foreach (FileSystem fs in image.GetFileSystems())
            {
                fs.WalkDirectories(DirWalkCallback);
            }

            Console.WriteLine(String.Format("Found {0} files.", fileCount));
            Console.ReadLine();
        }

        private static int fileCount = 0;

        private static WalkReturnEnum DirWalkCallback(ref TSK_FS_FILE file, string path, IntPtr some_ptr)
        {
            fileCount += file.AppearsValid ? 1 : 0;
            return WalkReturnEnum.Continue;
        }
    }
}