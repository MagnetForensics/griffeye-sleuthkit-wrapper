using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_FS_ATTR_RUN
    {
        private IntPtr next_ptr;  ///< Pointer to the next run in the attribute (or NULL)

        private ulong offset;     ///< Offset (in blocks) of this run in the file

        private ulong addr;       ///< Starting block address (in file system) of run

        private ulong len;        ///< Number of blocks in run (0 when entry is not in use)

        private ulong crypto_id;  ///< Starting block number used for XTS encryption IV

        private AttributeRunFlags flags;        ///< Flags for run

        public bool HasNext => next_ptr != IntPtr.Zero;

        public TSK_FS_ATTR_RUN Next => ((TSK_FS_ATTR_RUN)Marshal.PtrToStructure(next_ptr, typeof(TSK_FS_ATTR_RUN)));

        public ulong Offset => offset;

        public ulong Address => addr;

        public ulong Length => len;

        public AttributeRunFlags Flags => flags;
    }
}