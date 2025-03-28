using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    /// <summary>
    /// TSK_FS_DIR
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct TSK_FS_DIR
    {
        internal StructureMagic tag;                // \internal Will be set to TSK_FS_DIR_TAG if structure is still allocated, 0 if not
        internal IntPtr fs_dir;// TSK_FS_FILE* fs_file;   // Pointer to the file structure for the directory.
        internal IntPtr ptr_list_names;// TSK_FS_NAME* names;     // Pointer to list of names in directory.
        internal ulong names_used;// size_t names_used;      // Number of name structures in queue being used
        internal ulong names_allocated;// size_t names_alloc;     // Number of name structures that were allocated

        /// <summary>
        /// Metadata address of this directory
        /// </summary>
        internal ulong address;// TSK_INUM_T addr;    // Metadata address of this directory

        internal uint seq; // Metadata address sequence (NTFS Only)

        private IntPtr ptr_fsinfo;// TSK_FS_INFO* fs_info;   // Pointer to file system the directory is located in

        /// <summary>
        /// Verifies tag
        /// </summary>
        public bool AppearsValid => tag == StructureMagic.FilesystemDirectoryTag;
    }
}