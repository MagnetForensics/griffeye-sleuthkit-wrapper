using SleuthKit;
using SleuthKit.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EWF;
namespace Org.SleuthKit.RecycleBin
{
    public class LibEwfTest
    {
        private static HashSet<String> setFiles;
        private static int setFilesHit;
        private static int totalFilesHit;

        public static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                PrintHelp();
            }
            else
            {
                MakeCsv(args[0]);
            }
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage:\n\tsleuthkit-sharp-test.exe ImageFile\n\t(Will output CSV with metadata about all files in the image)");
        }
        
        public static void MakeCsv(String imageFile)
        {
            var handle = new Handle();

            try
            {
                handle.Open(new string[] { imageFile }, 1);
                totalFilesHit = 0;

                Console.WriteLine("FullPath,Metadata,Metadata.AppearsValid,Metadata.Type,Metadata.Flags,Name,Name.AppearsValid,Name.Type,Name.Flags,Include,InSet");

                //iterate

                Console.WriteLine("Total files hit: " + totalFilesHit);
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR : Couldnt Retrieve info due to error:{0}", exception);
            }
            finally
            {
                handle.Close();
                handle.Dispose();
            }
        }


    }
}
