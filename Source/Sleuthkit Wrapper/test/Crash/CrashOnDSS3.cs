using SleuthKit;
using SleuthKit.Structs;
using System;

namespace Org.SleuthKit.Crash
{
    public class CrashOnDSS3
    {
        public static void Main(String[] args)
        {
            //String imagePath = @"I:\TestSets\Mixed\DATASET_SAFE_III\Images and exports\DSIII.E01";
            String imagePath = @"I:\TestSets\ForensicImages\DATASAFE_3-2014.E01";
            DiskImage image = new DiskImage(new System.IO.FileInfo(imagePath));

            if (image.HasVolumes)
            {
                VolumeSystem vs = image.OpenVolumeSystem();
                foreach (Volume v in vs.Volumes)
                {
                    try
                    {
                        Console.WriteLine(String.Format("Desc: {0}", v.Description));
                        FileSystem fs = v.OpenFileSystem();
                        fs.WalkDirectories(DirWalkCallback);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("FAIL");
                    }
                }
            }
            else
            {
                try
                {
                    foreach (FileSystem fs in image.GetFileSystems())
                    {
                        fs.WalkDirectories(DirWalkCallback);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("FAIL");
                }
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