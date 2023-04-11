namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    using SleuthKit;

    using Directory = System.IO.Directory;
    using File = SleuthKit.File;
    using FileStream = SleuthKit.FileStream;
    using SleuthKit.Structs;

    /// <summary>
    /// Test class for SleuthKit wrapper
    /// </summary>
    public class Program
    {
        #region Static Fields

        /// <summary>
        /// The file paths.
        /// </summary>
        private static readonly IList<string> FilePaths = new List<string>();

        /// <summary>
        /// stop watch
        /// </summary>
        private static readonly Stopwatch WrapperStopwatch = new Stopwatch();

        #endregion Static Fields

        #region Public Methods and Operators

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">
        /// The arguments.
        /// </param>
        public static void Main(string[] args)
        {
            // delete the image folder if it already exists.
            var imageDir = new DirectoryInfo("Images");
            if (imageDir.Exists)
            {
                imageDir.Delete(true);
            }

            string flags = args[0];
            string fileName;
            DiskImage di;

            if (flags.Equals("-f"))
            {
                // read single image
                fileName = args[1];
                di = new DiskImage(new FileInfo(fileName));
            }
            else if (flags.Equals("-d"))
            {
                // read a set of image - given a folder
                string folderName = args[1];
                string[] files = Directory.GetFiles(folderName, "*.*");

                di = new DiskImage(files.Select(file => new FileInfo(file)).ToArray());

                // set first filename for metadata extraction
                fileName = files[0];
            }
            else
            {
                Console.WriteLine("Use ‘–d <split image directory path>’, if you want to extract files " +
                    "from a split forensic image. Use ‘–f <image file path>' if you want to extract files " +
                    "from a single forensic image.");
                return;
            }

            // method call to extract meta.
            IDictionary<string, string> metaDataDictionary = MetaDataExtractor.Extract(fileName);

            foreach (var pair in metaDataDictionary)
            {
                Console.WriteLine("{0}  :   {1}", pair.Key, pair.Value);
            }

            // temporarily pause to read console output. Press any key to continue
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();

            bool hasVolumes = false;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This disk image is {0:n0}b.", di.Size);
            Console.ResetColor();

            using (VolumeSystem vs = di.OpenVolumeSystem())
            {
                if (vs != null && vs.PartitionCount > 0)
                {
                    foreach (Volume v in vs.Volumes)
                    {
                        var withinPool = false;
                        using (var openPool = v.OpenPool())
                        {
                            if (openPool != null)
                            {
                                withinPool = true;
                                foreach (var tskPoolVolumeInfo in openPool.GetVolumeInfos())
                                {
                                    using (FileSystem fs = openPool.OpenFileSystem(tskPoolVolumeInfo))
                                    {
                                        String volumeName = fs.Label;
                                        if (fs.WalkDirectories(DirectoryWalkCallback))
                                        {
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.WriteLine("Volume \"{0}\" ({1} @ {2}):", volumeName, fs.Type, v.Address);
                                            Console.ResetColor();

                                            // temporarily pause to read console output. Press any key to continue
                                            Console.WriteLine("Press any key to continue...");
                                            Console.ReadLine();

                                            ProcessFiles(fs);
                                        }
                                    }
                                }

                            }
                        }

                        hasVolumes = true;
                        if (!withinPool)
                        {
                            try
                            {
                                using (FileSystem fs = v.OpenFileSystem())
                                {
                                    String volumeName = fs.Label;

                                    if (fs.WalkDirectories(DirectoryWalkCallback))
                                    {
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("Volume \"{0}\" ({1} @ {2}):", volumeName, fs.Type, v.Address);
                                        Console.ResetColor();

                                        // temporarily pause to read console output. Press any key to continue
                                        Console.WriteLine("Press any key to continue...");
                                        Console.ReadLine();

                                        ProcessFiles(fs);
                                    }
                                }

                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Volume \"{0}\" could not be opened", v.Description);
                                Console.ResetColor();
                            }
                        }
                    }
                }
            }

            if (!hasVolumes)
            {
                using (FileSystem fs = di.OpenFileSystem())
                {
                    String volumeName = fs.Label;

                    if (fs.WalkDirectories(DirectoryWalkCallback))
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Volume \"{0}\" ({1} @ 0):", volumeName, fs.Type);
                        Console.ResetColor();

                        // temporarily pause to read console output. Press any key to continue
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadLine();

                        ProcessFiles(fs);
                    }
                }
            }

            di.Dispose();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("This disk image {0} multiple volumes.", hasVolumes ? "has" : "does not have");
            Console.ResetColor();

            /*
            var regularHdStopWatch = new Stopwatch();
            string[] extractedfiles = Directory.GetFiles("Images", "*.*");

            regularHdStopWatch.Start();

            Parallel.ForEach(
                extractedfiles,
                extractedfile =>
                    {
                        // Construct file stream
                        var fileStream = new System.IO.FileStream(extractedfile, FileMode.Open);

                        // Calculate md5 hash
                        MD5 md5 = new MD5CryptoServiceProvider();
                        byte[] hash = md5.ComputeHash(fileStream);
                        var builder = new StringBuilder();

                        foreach (byte b in hash)
                        {
                            builder.Append(b.ToString("x2"));
                        }

                        fileStream.Close();
                        fileStream.Dispose();
                    });

            regularHdStopWatch.Stop();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nTime elapsed reading with the wrapper - {0} millisec \nTime elapsed reading with the .Net classes - {1} millisec", WrapperStopwatch.ElapsedMilliseconds, regularHdStopWatch.ElapsedMilliseconds);
            //*/
            Console.ResetColor();
            Console.ReadLine();
        }

        #endregion Public Methods and Operators

        #region Methods

        /// <summary>
        /// Callback function that is called for each file name during directory walk. (FileSystem.WalkDirectories)
        /// </summary>
        /// <param name="file">
        /// The file struct.
        /// </param>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <param name="dataPtr">
        /// Pointer to data that is passed to the callback function each time.
        /// </param>
        /// <returns>
        /// Value to control the directory walk.
        /// </returns>
        private static WalkReturnEnum DirectoryWalkCallback(ref TSK_FS_FILE file, IntPtr utf8_path, IntPtr dataPtr)
        {
            var directoryPath = utf8_path.Utf8ToUtf16();
            FilePaths.Add(string.Format("{0}{1}", directoryPath, file.Name));
            return WalkReturnEnum.Continue;
        }

        /// <summary>
        /// Callback function that is called for file content during file walk. (File.WalkFile)
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="offset">
        /// The offset.
        /// </param>
        /// <param name="address">
        /// The address.
        /// </param>
        /// <param name="buffer">
        /// The data buffer.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="dataPtr">
        /// Pointer to data that is passed to the callback function each time.
        /// </param>
        /// <returns>
        /// Value to control the file walk.
        /// </returns>
        private static WalkReturnEnum FileContentWalkCallback(
            ref TSK_FS_FILE file,
            long offset,
            long address,
            IntPtr buffer,
            int length,
            FileSystemBlockFlags flags,
            IntPtr dataPtr)
        {
            int written;
            WriteFile(dataPtr, buffer, length, out written, IntPtr.Zero);
            return WalkReturnEnum.Continue;
        }

        /// <summary>
        /// Calculate hash values and copy files in a given file system.
        /// </summary>
        /// <param name="fileSystem">
        /// The file system.
        /// </param>
        private static void ProcessFiles(FileSystem fileSystem)
        {
            // Parallel read via a for loop
            Parallel.ForEach(
                FilePaths, new ParallelOptions() { MaxDegreeOfParallelism = 8 },
                filePath =>
                {
                    WrapperStopwatch.Start();

                    // Open file
                    using (File file = fileSystem.OpenFile(filePath))
                    {
                        if (file != null)
                        {
                            // Construct file stream
                            using (var stream = new FileStream(file))
                            {
                                try
                                {
                                    // Calculate md5 hash
                                    MD5 md5 = new MD5CryptoServiceProvider();
                                    byte[] hash = md5.ComputeHash(stream);
                                    var builder = new StringBuilder();

                                    foreach (byte b in hash)
                                    {
                                        builder.Append(b.ToString("x2"));
                                    }

                                    Console.WriteLine("{0}=> {1}", filePath, builder);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("{0}=> {1}", filePath, "FAIL");
                                }
                            }

                            WrapperStopwatch.Stop();

                            /* Copy file
                            System.IO.FileStream fileStream = null;
                            try
                            {
                                const string ImageFolder = "Images";

                                Directory.CreateDirectory(ImageFolder);
                                string fileCopyPath = Path.Combine(ImageFolder, file.Path);

                                fileStream = new System.IO.FileStream(fileCopyPath, FileMode.Create, FileAccess.Write);
                                file.WalkFile(FileContentWalkCallback, fileStream.Handle);

                                Console.WriteLine("{0} copied successfully.", file.Path);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("ERROR : Couldnt write file:{0} due to error:{1}", file.Name, ex);
                            }
                            finally
                            {
                                if (fileStream != null)
                                {
                                    fileStream.Close();
                                }
                            }
                            //*/
                        }
                    }
                });
        }

        /// <summary>
        /// Writes data to a file.
        /// </summary>
        /// <param name="fileHandle">
        /// Handle to the file to be written to.
        /// </param>
        /// <param name="buffer">
        /// Pointer to the buffer containing the data to write to the file.
        /// </param>
        /// <param name="numberOfBytesToWrite">
        /// Number of bytes to write to the file.
        /// </param>
        /// <param name="numberOfBytesWritten">
        /// Pointer to the number of bytes written by this function call.
        /// </param>
        /// <param name="overlapped">
        /// Unsupported; set to NULL.
        /// </param>
        /// <returns>
        /// Successful or not.
        /// </returns>
        [DllImport("kernel32.dll")]
        private static extern bool WriteFile(
            IntPtr fileHandle, IntPtr buffer, int numberOfBytesToWrite, out int numberOfBytesWritten, IntPtr overlapped);

        #endregion Methods
    }
}