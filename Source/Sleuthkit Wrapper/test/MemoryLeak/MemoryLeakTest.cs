using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.MemoryLeak
{
    public class MemoryLeakTest
    {
        public static int N_THREADS = 8;
        public static int nFiles = 0;

        public static CancellationTokenSource tokenSource = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            ImportSource iSource = new ImportSource(args[0], tokenSource.Token);

            List<Thread> processingThreads = new List<Thread>();

            for (int i = 0; i < N_THREADS; i++)
            {
                Thread tprocessingThread = new Thread(() =>
                {
                    ProcessMethod(iSource);
                }) { Name = "Processing thread " + i };

                processingThreads.Add(tprocessingThread);
                tprocessingThread.Start();            
            }

            foreach (Thread t in processingThreads)
            {
                t.Join();
            }

            Console.WriteLine("done");
        }

        public static void ProcessMethod(ImportSource iSource)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            MediaPath mediaPath;

            while (iSource.TryGetNextFile(out mediaPath))
            {
                //Process the file
                Stream originalStream = null;

                try
                {
                    MediaInfo mediaInfo = iSource.imageMediaSource.EvidenceInfo(mediaPath);

                    //*
                    originalStream = iSource.imageMediaSource.OpenEvidence(mediaPath);

                    byte[] hash = md5.ComputeHash(originalStream);
                    var builder = new StringBuilder();
                    foreach (byte b in hash) { builder.Append(b.ToString("x2")); }
                    //*/

                    Interlocked.Increment(ref nFiles);

                    Console.WriteLine(String.Format("{0} -> {1} ({2})", mediaPath.InternalPath, builder, nFiles));
                }
                catch (Exception)
                {
                    Console.WriteLine("{0} -> {1}", mediaPath.InternalPath, "FAIL");
                }
                finally
                {
                    if (originalStream != null)
                    {
                        originalStream.Dispose();
                        originalStream = null;
                    }                    
                }
            }        
        }
    }
}
