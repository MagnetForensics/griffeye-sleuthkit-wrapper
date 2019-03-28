using SleuthKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SleuthKit.Structs;

namespace Test.MemoryLeak
{
    public class MediaSource
    {
        private DiskImage image;
        private bool hasVolumes;
        private CancellationToken cancellationToken;

        public MediaSource(String forensicImagePath, CancellationToken cancellationToken)
        {
            image = new DiskImage(new FileInfo(forensicImagePath));

            hasVolumes = false;

            this.cancellationToken = cancellationToken;

            using (VolumeSystem vs = image.OpenVolumeSystem())
            {
                if (vs != null && vs.PartitionCount > 0 && vs.Volumes.Count() > 0)
                {
                    hasVolumes = true;
                }
            }
        }

        public Stream OpenEvidence(MediaPath path)
        {
            return new SleuthKit.FileStream(OpenEvidenceFile(path));
        }

        public SleuthKit.File OpenEvidenceFile(MediaPath path)
        {
            if (hasVolumes)
            {
                using (VolumeSystem volumeSystem = image.OpenVolumeSystem())
                {
                    Volume volume = volumeSystem.Volumes.SingleOrDefault(v => v.Address == path.VolumeAddress);

                    if (null == volume)
                    {
                        throw new ArgumentException(String.Format("Volume {0} does not exist", path.VolumeAddress));
                    }

                    //* Original
                    FileSystem fileSystem = volume.OpenFileSystem();
                    return OpenFile(fileSystem, path.FileAddress, path.DisplayPath);
                    //*/
                    /* Changed
                    using (FileSystem fileSystem = volume.OpenFileSystem())
                    {
                        return OpenFile(fileSystem, path.FileAddress, path.DisplayPath);
                    }
                    //*/
                }
            }
            else
            {
                //* Original
                FileSystem fileSystem = image.OpenFileSystem();
                return OpenFile(fileSystem, path.FileAddress, path.DisplayPath);
                //*/
                /* Changed
                using (FileSystem fileSystem = image.OpenFileSystem())
                {
                    return OpenFile(fileSystem, path.FileAddress, path.DisplayPath);
                }
                //*/
            }
        }

        private SleuthKit.File OpenFile(FileSystem fileSystem, long address, String displayPath)
        {
            SleuthKit.File file = fileSystem.OpenFile(address, null, displayPath);
            if (null == file)
            {
                throw new ArgumentException(String.Format("Evidence with address=\"{0}\" does not exist in source.", address));
            }
            else
            {
                return file;
            }
        }

        public bool EvidenceIsFile(MediaPath path)
        {
            return false;
        }

        public String EvidencePath(MediaPath path)
        {
            throw new ArgumentException(String.Format("Evidence with ipath=\"{0}\" does not exist on disk.", path.DisplayPath));
        }

        public MediaInfo EvidenceInfo(MediaPath path)
        {
            /* Original
            SleuthKit.File file = OpenEvidenceFile(path);

            if (null == file)
            {
                return new MediaInfo()
                {
                    Exists = false,
                };
            }
            else
            {
                int lastSeparatorIndex = file.Path.LastIndexOf(Path.DirectorySeparatorChar);
                int extensionIndex = file.Name.LastIndexOf('.');

                return new MediaInfo()
                {
                    DirectoryName = lastSeparatorIndex != -1 ? StripIllegalPathChars(file.Path.Substring(0, lastSeparatorIndex)) : String.Empty,
                    Exists = true,
                    Extension = extensionIndex != -1 ? file.Name.Substring(extensionIndex) : String.Empty,
                    Length = file.Size,
                    Name = StripIllegalPathChars(file.Name),
                    CreationTimeUtc = file.CreationTime,
                    LastAccessTimeUtc = file.LastAccessTime,
                    LastWriteTimeUtc = file.LastWriteTime,
                };
            }
            //*/

            //* Changed
            using (SleuthKit.File file = OpenEvidenceFile(path))
            {
                if (null == file)
                {
                    return new MediaInfo()
                    {
                        Exists = false,
                    };
                }
                else
                {
                    int lastSeparatorIndex = file.Path.LastIndexOf(Path.DirectorySeparatorChar);
                    int extensionIndex = file.Name.LastIndexOf('.');

                    return new MediaInfo()
                    {
                        DirectoryName = lastSeparatorIndex != -1 ? StripIllegalPathChars(file.Path.Substring(0, lastSeparatorIndex)) : String.Empty,
                        Exists = true,
                        Extension = extensionIndex != -1 ? file.Name.Substring(extensionIndex) : String.Empty,
                        Length = file.Size,
                        Name = StripIllegalPathChars(file.Name),
                        CreationTimeUtc = file.CreationTime,
                        LastAccessTimeUtc = file.LastAccessTime,
                        LastWriteTimeUtc = file.LastWriteTime,
                    };
                }
            }
            //*/
        }

        public IEnumerable<MediaPath> AllEvidences()
        {
            foreach (Tuple<SleuthKit.File, MediaPath> file in AllFiles())
            {
                file.Item1.Dispose();
                yield return file.Item2;
            }
        }

        public IEnumerable<Tuple<SleuthKit.File, MediaPath>> AllFiles()
        {
            if (hasVolumes)
            {
                using (VolumeSystem volumeSystem = image.OpenVolumeSystem())
                {
                    foreach (Volume volume in volumeSystem.Volumes)
                    {
                        using (FileSystem fileSystem = volume.OpenFileSystem())
                        {
                            FileEnumerator fileEnumerator = new FileEnumerator(fileSystem, volume.Address, cancellationToken);

                            while (fileEnumerator.MoveNext())
                            {
                                yield return fileEnumerator.Current;
                            }
                        }
                    }
                }
            }
            else
            {
                using (FileSystem fileSystem = image.OpenFileSystem())
                {
                    FileEnumerator fileEnumerator = new FileEnumerator(fileSystem, cancellationToken);

                    while (fileEnumerator.MoveNext())
                    {
                        yield return fileEnumerator.Current;
                    }
                }
            }
        }

        public int Count()
        {
            if (hasVolumes)
            {
                int count = 0;

                using (VolumeSystem volumeSystem = image.OpenVolumeSystem())
                {
                    foreach (Volume volume in volumeSystem.Volumes)
                    {
                        using (FileSystem fileSystem = volume.OpenFileSystem())
                        {
                            CountCallbackContainer countCallback = new CountCallbackContainer(cancellationToken);
                            fileSystem.WalkDirectories(countCallback.Callback, DirWalkFlags.Recurse | DirWalkFlags.Unallocated | DirWalkFlags.Allocated);
                            count += countCallback.Count;
                        }
                    }
                }

                return count;
            }
            else
            {
                using (FileSystem fileSystem = image.OpenFileSystem())
                {
                    CountCallbackContainer countCallback = new CountCallbackContainer(cancellationToken);
                    fileSystem.WalkDirectories(countCallback.Callback, DirWalkFlags.Recurse | DirWalkFlags.Unallocated | DirWalkFlags.Allocated);
                    return countCallback.Count;
                }
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

        #region Other wrapper classes

        private class CountCallbackContainer
        {
            public int Count { get; private set; }

            private CancellationToken cancellationToken;

            public CountCallbackContainer(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;

                Count = 0;
            }

            public WalkReturnEnum Callback(ref TSK_FS_FILE file, IntPtr utf8_path, IntPtr dataPtr)
            {
                if (file.Metadata.HasValue && file.Name.HasValue &&
                    (file.Name.Value.Type == FilesystemNameType.Regular
                    || file.Name.Value.Type == FilesystemNameType.SymbolicLink))
                {
                    Count++;
                }

                return cancellationToken.IsCancellationRequested ? WalkReturnEnum.Stop : WalkReturnEnum.Continue;
            }
        }

        private class FileCallbackContainer
        {
            private readonly bool hasVolumes;
            private readonly long volumeAddress;

            private readonly FileSystem fileSystem;
            private Tuple<SleuthKit.File, MediaPath> _nextFile;

            public bool TryGetNextFile(out Tuple<SleuthKit.File, MediaPath> value, int timeout)
            {
                if (readSemaphore.WaitOne(timeout))
                {
                    value = _nextFile;
                    writeSemaphore.Release();
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            private bool TrySetNextFile(Tuple<SleuthKit.File, MediaPath> value, int timeout)
            {
                if (writeSemaphore.WaitOne(timeout))
                {
                    _nextFile = value;
                    readSemaphore.Release();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private readonly Semaphore readSemaphore;
            private readonly Semaphore writeSemaphore;

            private readonly CancellationToken internalCancellationToken;
            private readonly CancellationToken externalCancellationToken;

            public FileCallbackContainer(FileSystem fileSystem, CancellationToken internalCancellationToken, CancellationToken externalCancellationToken)
            {
                hasVolumes = false;

                readSemaphore = new Semaphore(0, 1);
                writeSemaphore = new Semaphore(1, 1);

                this.fileSystem = fileSystem;

                this.internalCancellationToken = internalCancellationToken;
                this.externalCancellationToken = externalCancellationToken;
            }

            public FileCallbackContainer(FileSystem fileSystem, long volumeAddress, CancellationToken internalCancellationToken, CancellationToken externalCancellationToken)
            {
                hasVolumes = true;
                this.volumeAddress = volumeAddress;

                readSemaphore = new Semaphore(0, 1);
                writeSemaphore = new Semaphore(1, 1);

                this.fileSystem = fileSystem;

                this.internalCancellationToken = internalCancellationToken;
                this.externalCancellationToken = externalCancellationToken;
            }

            public WalkReturnEnum Callback(ref TSK_FS_FILE file, IntPtr utf8_path, IntPtr dataPtr)
            {
                if (file.Metadata.HasValue && file.Name.HasValue &&
                    (file.Name.Value.Type == FilesystemNameType.Regular
                    || file.Name.Value.Type == FilesystemNameType.SymbolicLink))
                {
                    var directoryPath = utf8_path.Utf8ToUtf16();

                    Tuple<SleuthKit.File, MediaPath> newFile = new Tuple<SleuthKit.File, MediaPath>(
                        fileSystem.OpenFile(file.Metadata.Value.Address),
                        hasVolumes
                            ? new MediaPath(StripIllegalPathChars(directoryPath + file.Name), volumeAddress, file.Metadata.Value.Address)
                            : new MediaPath(StripIllegalPathChars(directoryPath + file.Name), file.Metadata.Value.Address));

                    while (!internalCancellationToken.IsCancellationRequested
                        && !externalCancellationToken.IsCancellationRequested
                        && !TrySetNextFile(newFile, 100)) ;
                }

                return internalCancellationToken.IsCancellationRequested
                    || externalCancellationToken.IsCancellationRequested
                    ? WalkReturnEnum.Stop : WalkReturnEnum.Continue;
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
        }

        private class FileEnumerator : IEnumerator<Tuple<SleuthKit.File, MediaPath>>, IDisposable
        {
            private Tuple<SleuthKit.File, MediaPath> currentFile;

            private readonly bool hasVolumes;
            private readonly long volumeAddress;
            private readonly FileSystem fileSystem;
            private FileCallbackContainer callbackContainer;

            private Task enumeratorTask;
            private CancellationTokenSource internalTokenSource;
            private CancellationToken externalCancellationToken;

            public FileEnumerator(FileSystem fileSystem, CancellationToken cancellationToken)
            {
                this.hasVolumes = false;

                this.fileSystem = fileSystem;
                this.externalCancellationToken = cancellationToken;

                Start();
            }

            public FileEnumerator(FileSystem fileSystem, long volumeAddress, CancellationToken cancellationToken)
            {
                this.hasVolumes = true;
                this.volumeAddress = volumeAddress;

                this.fileSystem = fileSystem;
                this.externalCancellationToken = cancellationToken;

                Start();
            }

            private void Start()
            {
                currentFile = null;

                internalTokenSource = new CancellationTokenSource();

                callbackContainer = hasVolumes
                    ? new FileCallbackContainer(fileSystem, volumeAddress, internalTokenSource.Token, externalCancellationToken)
                    : new FileCallbackContainer(fileSystem, internalTokenSource.Token, externalCancellationToken);

                enumeratorTask = new Task(() =>
                {
                    fileSystem.WalkDirectories(callbackContainer.Callback,
                        DirWalkFlags.Recurse | DirWalkFlags.Unallocated | DirWalkFlags.Allocated);
                });
                enumeratorTask.Start();
            }

            private void Stop()
            {
                internalTokenSource.Cancel();
            }

            public Tuple<SleuthKit.File, MediaPath> Current
            {
                get { return currentFile; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                Tuple<SleuthKit.File, MediaPath> nextFile;
                bool hasNextFile = false;

                while (!internalTokenSource.IsCancellationRequested
                    && !externalCancellationToken.IsCancellationRequested
                    && !enumeratorTask.IsCompleted)
                {
                    if (callbackContainer.TryGetNextFile(out nextFile, 100))
                    {
                        currentFile = nextFile;
                        hasNextFile = true;
                        break;
                    }
                }

                return hasNextFile;
            }

            public void Reset()
            {
                Stop();
                Start();
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            public void Dispose(bool disposeManaged)
            {
                Stop();

                if (disposeManaged)
                {
                    callbackContainer = null;

                    if (fileSystem != null)
                        fileSystem.Dispose();

                    if (enumeratorTask != null)
                    {
                        enumeratorTask.Dispose();
                        enumeratorTask = null;
                    }

                    if (internalTokenSource != null)
                    {
                        internalTokenSource.Dispose();
                        internalTokenSource = null;
                    }
                }
            }

            ~FileEnumerator()
            {
                Dispose(false);
            }
        }

        #endregion Other wrapper classes
    }
}