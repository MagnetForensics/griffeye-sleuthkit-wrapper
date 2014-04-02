using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.SleuthKit.RecycleBin
{
    public class HashListMatch
    {
        public static void Main(String[] args)
        {
            String imageFile = @"C:\Users\Josef.Eklann\Desktop\test\mystery files\CompressedTest\ImageHashes.csv";
            String totalFile = @"C:\Users\Josef.Eklann\Desktop\test\mystery files\CompressedTest\AllHashes.csv";

            Dictionary<String, String> imageHashes = ReadHashes(imageFile);
            Dictionary<String, String> totalHashes = ReadHashes(totalFile);

            foreach (KeyValuePair<String, String> kvp in imageHashes)
            {
                if (totalHashes.ContainsKey(kvp.Key))
                {
                    if (!totalHashes[kvp.Key].Equals(kvp.Value))
                    {
                        Console.WriteLine(kvp.Key);
                    }
                }
                else
                { 
                    //Some other file from before
                }
            }
        }

        private static Dictionary<String, String> ReadHashes(String filepath)
        {
            Dictionary<String, String> fileHashes = new Dictionary<String, String>();

            using (TextReader imageReader = File.OpenText(filepath))
            {
                String line = imageReader.ReadLine();
                while (!String.IsNullOrEmpty(line))
                {
                    String[] data = line.Split(new char[] { ',' });

                    fileHashes.Add(data[1], data[0]);

                    line = imageReader.ReadLine();
                }
            }

            return fileHashes;
        }
    }
}
