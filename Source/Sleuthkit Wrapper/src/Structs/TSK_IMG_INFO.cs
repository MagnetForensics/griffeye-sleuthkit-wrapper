using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    /// <summary>
    /// represents TSK_IMG_INFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_IMG_INFO
    {
        /// <summary>
        /// Set to TSK_IMG_INFO_TAG when struct is alloc
        /// </summary>
        internal StructureMagic tag;

        /// <summary>
        /// Type of disk image format
        /// </summary>
        internal ImageType itype;

        /// <summary>
        /// Total size of image in bytes
        /// </summary>
        internal long size;

        /// <summary>
        /// Number of image files
        /// </summary>
        internal int num_img;

        /// <summary>
        /// sector size of device in bytes (typically 512)
        /// </summary>
        internal uint sector_size;

        /// <summary>
        /// page size of NAND page in bytes (defaults to 2048)
        /// </summary>
        internal uint page_size;

        /// <summary>
        /// spare or OOB size of NAND in bytes (defaults to 64)
        /// </summary>
        internal uint spare_size;

        /// <summary>
        /// Image names
        /// </summary>
        internal IntPtr images;

        /// <summary>
        /// Checks that size and sector_size are non-zero.
        /// </summary>
        public bool IsInitialized => size != 0 && sector_size != 0;
    };
}