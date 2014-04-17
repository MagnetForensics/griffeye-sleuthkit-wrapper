using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Org.SleuthKit.Overwritten
{
    public class OverwrittenTest
    {
        private static String searchString;

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

        private static void IterateImage(DiskImage image, DirWalkPtrDelegate callback)
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
                            if (filesystem.Type == FileSystemType.HFS)
                            {
                                HFS_INFO info = filesystem.debugHfs();

                                List<TSK_FS_ATTR> attr = info.CatalogAttributes.ToList();

                                Console.WriteLine(attr.Count);
                            }

                            filesystem.WalkDirectories(callback);
                        }
                    }
                }
            }
            else
            {
                using (FileSystem filesystem = image.OpenFileSystem())
                {
                    filesystem.WalkDirectories(callback);
                }
            }
        }

        private static WalkReturnEnum DirectoryWalkCallback(IntPtr filePtr, String directoryPath, IntPtr dataPtr)
        {
            TSK_FS_FILE file = ((TSK_FS_FILE)Marshal.PtrToStructure(filePtr, typeof(TSK_FS_FILE)));

            if (file.Name.HasValue && file.Name.Value.ToString().Contains(searchString))
            {
                String fullpath = directoryPath.Replace('/', '\\') + file.Name.Value;
                Console.WriteLine(fullpath);

                TSK_FS_NAME name = file.Name.Value;

                Console.WriteLine("filePtr: " + filePtr.ToInt64());
                //Console.WriteLine("MetadataAppearsValid(): " + file.MetadataAppearsValid(isNtfs));
                Console.WriteLine(String.Empty);
                Console.WriteLine("Name.LongName: " + name.LongName);
                Console.WriteLine("Name.ShortName: " + name.ShortName);
                Console.WriteLine("Name.Type: " + name.Type);
                Console.WriteLine("Name.Flags: " + name.Flags);
                Console.WriteLine("Name.MetadataAdress: " + name.MetadataAddress);
                Console.WriteLine("Name.MetadataSequence: " + name.MetadataSequence);
                Console.WriteLine("Name.ParentAdress: " + name.ParentAddress);
                Console.WriteLine("Name.ParentSequence: " + name.ParentSequence);
                Console.WriteLine(String.Empty);

                if (file.Metadata.HasValue)
                {
                    TSK_FS_META meta = file.Metadata.Value;

                    Console.WriteLine("Metadata.Address: " + meta.Address);
                    Console.WriteLine("Metadata.AppearsValid: " + meta.AppearsValid);
                    Console.WriteLine("Metadata.MetadataType: " + meta.MetadataType);
                    Console.WriteLine("Metadata.MetadataFlags: " + meta.MetadataFlags);
                    Console.WriteLine("Metadata.Mode: " + meta.Mode);
                    Console.WriteLine("Metadata.Sequence: " + meta.Sequence);
                    Console.WriteLine("Metadata.Size: " + meta.Size);
                    Console.WriteLine("Metadata.LinkCount: " + meta.LinkCount);
                    Console.WriteLine("Metadata.AttributeState: " + meta.AttributeState);
                    Console.WriteLine(String.Empty);

                    if (meta.HasAttributeList && !meta.AttributeList.IsEmpty)
                    {
                        Console.WriteLine("Metadata.AttributeList:");

                        foreach (TSK_FS_ATTR attr in meta.AttributeList.List)
                        {
                            Console.WriteLine("  Attribute.Id: " + attr.Id);
                            Console.WriteLine("  Attribute.Name: " + attr.Name);
                            Console.WriteLine("  Attribute.FilePointer: " + attr.FilePointer.ToInt64());
                            Console.WriteLine("  Attribute.AttributeType: " + attr.AttributeType);
                            Console.WriteLine("  Attribute.AttributeFlags: " + attr.AttributeFlags);
                            Console.WriteLine("  Attribute.Size: " + attr.Size);

                            if (attr.AttributeFlags.HasFlag(AttributeFlags.NonResident))
                            {
                                Console.WriteLine("  Attribute.NonResidentBlocks:");
                                foreach (TSK_FS_ATTR_RUN run in attr.NonResidentBlocks)
                                { 
                                    Console.WriteLine(String.Format("    Address: {0}, Length: {1}, Offset: {2}, Flags: {3}", 
                                        run.Address, run.Length, run.Offset, run.Flags));
                                }
                            }

                            Console.WriteLine(String.Empty);
                        }
                    }

                    if (meta.NameList.Any())
                    {
                        Console.WriteLine("Metadata.NameList:");

                        foreach (TSK_FS_META_NAME_LIST nameEntry in meta.NameList)
                        {
                            Console.WriteLine("  Name.Name: " + nameEntry.Name);
                            Console.WriteLine("  Name.ParentAddress: " + nameEntry.ParentAddress);
                            Console.WriteLine("  Name.ParentSequence: " + nameEntry.ParentSequence);

                            Console.WriteLine(String.Empty);
                        }
                    }
                }

                Console.WriteLine(String.Empty);
                Console.WriteLine("--------------------------------");
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
