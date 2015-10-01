using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
#if Bit32
        Size = 16832
#elif Bit64
        Size = 17008
#endif
    )]
    public struct FATFS_INFO
    {
        [FieldOffset(0)]
        private TSK_FS_INFO fs_info;

        //[FieldOffset(120)]
        //private IntPtr boot_sector_buffer_ptr;

        /// <summary>
        /// super block
        /// </summary>
#if Bit32
        [FieldOffset(16860)]
#elif Bit64
        [FieldOffset(17024)]
#endif
        private IntPtr sb_ptr;

        internal TSK_FS_INFO tsk_fs_info
        {
            get
            {
                return fs_info;
            }
        }

        internal fatfs_sb sb
        {
            get
            {
                return ((fatfs_sb)Marshal.PtrToStructure(sb_ptr, typeof(fatfs_sb)));
            }
        }
    }
}