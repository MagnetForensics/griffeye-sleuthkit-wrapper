using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
    Size = 528 + 16
    )]
    public struct FFS_INFO
    {
        [FieldOffset(0)]
        private TSK_FS_INFO fs_info;

        [FieldOffset(424 + 16)]
        private IntPtr sb_ptr;

        internal TSK_FS_INFO tsk_fs_info => fs_info;

        internal ffs_sb2 sb => ((ffs_sb2)Marshal.PtrToStructure(sb_ptr, typeof(ffs_sb2)));
    }
}