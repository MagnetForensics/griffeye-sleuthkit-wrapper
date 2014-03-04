using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    struct TSK_FS_META_NAME_LIST
    {
        /// <summary>
        /// Pointer to next name (or NULL)
        /// </summary>
        IntPtr next;

        /// <summary>
        /// Name in UTF-8 (does not include parent directory name)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        byte[] name;

        /// <summary>
        /// Inode address of parent directory (NTFS only)
        /// </summary>
        ulong par_inode;

        /// <summary>
        /// Sequence number of parent directory (NTFS only)
        /// </summary>
        uint par_seq;
    };
}
