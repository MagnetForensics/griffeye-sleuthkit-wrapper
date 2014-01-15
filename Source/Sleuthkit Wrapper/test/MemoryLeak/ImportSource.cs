using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.MemoryLeak
{
    public class ImportSource
    {
        public readonly MediaSource imageMediaSource;
        public readonly int nFiles;

        private readonly ConcurrentQueue<MediaPath> fileQueue;
        private readonly Semaphore bufferSemaphore;
        private Thread queueProducerThread;
        private CancellationToken cancellationToken;

        public ImportSource(String imagePath, CancellationToken cancellationToken) 
        {
            imageMediaSource = new MediaSource(imagePath, cancellationToken);

            //Count all the paths
            //nFiles = imageMediaSource.Count();

            fileQueue = new ConcurrentQueue<MediaPath>();
            bufferSemaphore = new Semaphore(50, 50);
            this.cancellationToken = cancellationToken;

            StartQueueProducer();
        }

        private void StartQueueProducer()
        {
            queueProducerThread = new Thread(() =>
            {
                foreach (Tuple<SleuthKit.File, MediaPath> file in imageMediaSource.AllFiles())
                {
                    try
                    {
                        //Acquire from semaphore
                        while (!bufferSemaphore.WaitOne(100) && !cancellationToken.IsCancellationRequested) { }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        //Try to queue the file
                        if (!TryEnqueueFileInImage(file.Item1, file.Item2))
                        {
                            bufferSemaphore.Release();
                        }

                        file.Item1.Dispose();
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception while processing forensic image.");
                        bufferSemaphore.Release();
                    }
                }
            }) { Name = "ImportSource Queue Producer Thread" };
            queueProducerThread.Start();
        }

        private bool TryEnqueueFileInImage(SleuthKit.File file, MediaPath mediaPath)
        {
            String displayPath = mediaPath.DisplayPath;

            try
            {
                int lastSlashIndex = displayPath != null ? displayPath.LastIndexOfAny(new char[] { '\\', '/' }) : -1;
                String directory = lastSlashIndex != -1 ? displayPath.Substring(0, lastSlashIndex) : String.Empty;
                String filename = lastSlashIndex != -1 ? displayPath.Substring(lastSlashIndex + 1) : displayPath;

                //Strip illegal characters
                displayPath = StripIllegalPathChars(displayPath);

                int extensionIndex = displayPath.LastIndexOf('.');
                String extension = extensionIndex != -1 ? displayPath.Substring(extensionIndex) : "";

                long size = file.Size;
                String fileName = StripIllegalPathChars(filename);
                /*
                Evidence evidence = new Evidence()
                {
                    FileByteSize = file.Size,
                    ExtensionOriginal = extension,
                    DateEntered = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                    EvidenceFiles = new SynchronizedCollection<EvidenceFile>(),
                    AbuseCategory = ImportController.DefaultAbuseCategory,
                    DIData = new DIData() { mediaPath = mediaPath, ImportType = SourceType.ForensicImage }
                };
                EvidenceFile evidenceFile = new EvidenceFile()
                {
                    FileName = StripIllegalPathChars(filename),
                    OriginalDirectory = Path.DirectorySeparatorChar + (!string.IsNullOrEmpty(directory) ? (StripIllegalPathChars(directory) + Path.DirectorySeparatorChar) : string.Empty),
                    Evidence = evidence,
                    Source = source,
                    CreationTime = file.CreationTime,
                    LastAccessTime = file.LastAccessTime,
                    LastWriteTime = file.LastWriteTime
                };

                evidence.DIData.evidenceFile = evidenceFile;

                ImportFile tmpImportFile = new ImportFile() { MediaPath = mediaPath, PreProcessedEvidence = evidence, MediaSource = imageMediaSource };
                //*/

                fileQueue.Enqueue(mediaPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Failed to extract file {0} from forensic image.", displayPath));
                return false;
            }
        }

        private static String StripIllegalPathChars(String path)
        {
            StringBuilder legalPath = new StringBuilder();
            HashSet<char> illegalChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            illegalChars.Add('\0');
            illegalChars.Remove('\\');
            illegalChars.Remove('/');

            foreach (char c in path)
            {
                if (!illegalChars.Contains(c))
                {
                    legalPath.Append(c);
                }
            }

            return legalPath.ToString();
        }

        public bool TryGetNextFile(out MediaPath file)
        {
            for (; ; )
            {
                if (fileQueue.TryDequeue(out file))
                {
                    bufferSemaphore.Release();
                    return true;
                }
                else if (null == queueProducerThread || queueProducerThread.ThreadState == ThreadState.Stopped)
                {
                    return false;
                }
                Thread.Sleep(100);
            }
        }
    }
}
