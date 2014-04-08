using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.SleuthKit.Overwritten
{
    public class OverwrittenTest
    {
        private static String searchString;
        private static bool isNtfs;

        public static void Main(String[] args)
        {
            if (args.Length != 2)
            {
                PrintHelp();
            }
            else
            { 
                DiskImage image = new DiskImage(new System.IO.FileInfo(args[0]));
                searchString = args[1];
                IterateImage(image, DirectoryWalkCallback);
            }
        }

        private static void IterateImage(DiskImage image, DirWalkDelegate callback)
        {
            bool hasVolumes = false;

            using (VolumeSystem vs = image.OpenVolumeSystem())
            {
                if (vs != null && vs.PartitionCount > 0 && vs.Volumes.Count() > 0)
                {
                    hasVolumes = true;
                }
            }

            if (hasVolumes)
            {
                using (VolumeSystem volumeSystem = image.OpenVolumeSystem())
                {
                    foreach (Volume volume in volumeSystem.Volumes)
                    {
                        using (FileSystem filesystem = volume.OpenFileSystem())
                        {
                            isNtfs = filesystem.Type == FileSystemType.NTFS;
                            filesystem.WalkDirectories(callback);
                        }
                    }
                }
            }
            else
            {
                using (FileSystem filesystem = image.OpenFileSystem())
                {
                    isNtfs = filesystem.Type == FileSystemType.NTFS;
                    filesystem.WalkDirectories(callback);
                }
            }
        }

        private static WalkReturnEnum DirectoryWalkCallback(ref TSK_FS_FILE file, String directoryPath, IntPtr dataPtr)
        {
            if (file.Name.HasValue && file.Name.Value.ToString().Contains(searchString))
            {
                String fullpath = directoryPath.Replace('/', '\\') + file.Name.Value;
                Console.WriteLine(fullpath);

                TSK_FS_NAME name = file.Name.Value;

                Console.WriteLine("MetadataAppearsValid(): " + file.MetadataAppearsValid(isNtfs));

                Console.WriteLine("Name.LongName: " + name.LongName);
                Console.WriteLine("Name.ShortName: " + name.ShortName);
                Console.WriteLine("Name.Type: " + name.Type);
                Console.WriteLine("Name.Flags: " + name.Flags);
                Console.WriteLine("Name.MetadataAdress: " + name.MetadataAddress);
                Console.WriteLine("Name.MetadataSequence: " + name.MetadataSequence);
                Console.WriteLine("Name.ParentAdress: " + name.ParentAddress);
                Console.WriteLine("Name.ParentSequence: " + name.ParentSequence);

                Console.WriteLine(String.Empty);
            }

            return WalkReturnEnum.Continue;
        }

        private static void PrintHelp()
        {
            Console.Error.WriteLine("Usage: \n  sleuthkit-sharp-test.exe [Image Path] [Filename to search for]");
        }
    }
}
